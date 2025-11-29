using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using System;

namespace PixelWorld
{
    public class PixelCollisionSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float readbackInterval = 0.02f; // 50Hz update for better responsiveness
        [SerializeField] private bool debugDraw = false;

        [Header("References")]
        [SerializeField] private PixelWorldManager worldManager;

        // Raw data from GPU (Flattened 1D array)
        private NativeArray<int> _worldData;
        private bool _hasData = false;
        private float _timer;
        private int _width;
        private int _height;

        // Singleton-ish access for the player
        public static PixelCollisionSystem Instance { get; private set; }
        public bool HasData => _hasData;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (worldManager == null) worldManager = FindObjectOfType<PixelWorldManager>();
            if (worldManager == null) Debug.LogError("PixelCollisionSystem: Could not find PixelWorldManager!");
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= readbackInterval)
            {
                _timer = 0;
                RequestReadback();
            }
        }

        private void RequestReadback()
        {
            if (worldManager == null) return;

            RenderTexture rt = worldManager.GetCurrentTexture();
            if (rt == null) 
            {
                // Debug.LogWarning("PixelCollisionSystem: World texture is null!");
                return;
            }

            _width = rt.width;
            _height = rt.height;

            AsyncGPUReadback.Request(rt, 0, OnReadbackComplete);
        }

        private void OnReadbackComplete(AsyncGPUReadbackRequest request)
        {
            if (request.hasError) 
            {
                Debug.LogError("PixelCollisionSystem: GPU Readback error!");
                return;
            }

            var data = request.GetData<int>(0);
            
            if (!_worldData.IsCreated || _worldData.Length != data.Length)
            {
                if (_worldData.IsCreated) _worldData.Dispose();
                _worldData = new NativeArray<int>(data.Length, Allocator.Persistent);
            }

            NativeArray<int>.Copy(data, _worldData, data.Length);
            
            if (!_hasData)
            {
                Debug.Log("PixelCollisionSystem: First collision data received!");
                _hasData = true;
            }
        }

    // API
    public bool IsSolid(Vector2 worldPos)
    {
        if (!_hasData || !_worldData.IsCreated) return false; // Fallback: assume empty
        
        if (worldManager == null)
        {
            Debug.LogWarning("PixelCollisionSystem: worldManager reference is null!");
            return false;
        }

        // Convert World to Pixel using WorldToPixel method
        Vector2 pixelPos = WorldToPixel(worldPos);
        
        int x = Mathf.FloorToInt(pixelPos.x);
        int y = Mathf.FloorToInt(pixelPos.y);

        // Out of bounds check
        if (x < 0 || x >= _width || y < 0) 
        {
            // Left, Right, Bottom are solid
            return true; 
        }
        
        if (y >= _height)
        {
            // Top is OPEN (Sky)
            return false;
        }

        // Index in flattened array: y * width + x
        int index = y * _width + x;
        
        if (index < 0 || index >= _worldData.Length)
        {
            // Debug.LogWarning($"PixelCollisionSystem: Invalid index {index} for position {worldPos} (pixel: {pixelPos})");
            return false;
        }
        
        int matID = _worldData[index];

        // Define what is solid
        // 0=Empty, 1=Rock, 2=Dirt, 3=Sand, 4=Water
        // We treat Water as non-solid for walking (swim logic is separate)
        return (matID == 1 || matID == 2 || matID == 3);
    }

    /// <summary>
    /// Checks for collision with a volume threshold (useful for ignoring small floating pixels when falling)
    /// </summary>
    public bool IsSolidDown(Vector2 worldPos, int threshold)
    {
        if (!_hasData || !_worldData.IsCreated) return false;

        Vector2 pixelPos = WorldToPixel(worldPos);
        int cx = Mathf.FloorToInt(pixelPos.x);
        int cy = Mathf.FloorToInt(pixelPos.y);

        // Check a small area (e.g. 3x2) below the point
        // We want to ensure there is "enough" ground
        int solidCount = 0;
        
        // Check 3 pixels wide, 2 pixels deep
        for (int y = cy - 1; y <= cy; y++)
        {
            for (int x = cx - 1; x <= cx + 1; x++)
            {
                if (x < 0 || x >= _width || y < 0) 
                {
                    solidCount++; // Bounds are solid
                    continue;
                }
                if (y >= _height) continue;

                int index = y * _width + x;
                if (index >= 0 && index < _worldData.Length)
                {
                    int matID = _worldData[index];
                    if (matID == 1 || matID == 2 || matID == 3) // Rock, Dirt, Sand
                    {
                        solidCount++;
                    }
                }
            }
        }

        return solidCount > threshold;
    }
    
    /// <summary>
    /// Convert world position to pixel coordinates (matches PixelWorldManager's conversion)
    /// </summary>
    private Vector2 WorldToPixel(Vector2 worldPos)
    {
        float cellSize = worldManager != null ? worldManager.CellSize : 0.02f;
        
        float halfWidth = (_width * cellSize) / 2f;
        float halfHeight = (_height * cellSize) / 2f;

        float pixelX = (worldPos.x + halfWidth) / cellSize;
        float pixelY = (worldPos.y + halfHeight) / cellSize;
        
        return new Vector2(pixelX, pixelY);
    }
    
    /// <summary>
    /// Get material ID at world position (useful for advanced checks)
    /// </summary>
    public int GetMaterialAt(Vector2 worldPos)
    {
        if (!_hasData || !_worldData.IsCreated) return 0;
        
        Vector2 pixelPos = WorldToPixel(worldPos);
        int x = Mathf.FloorToInt(pixelPos.x);
        int y = Mathf.FloorToInt(pixelPos.y);

        if (x < 0 || x >= _width || y < 0 || y >= _height) return 1; // Out of bounds = Rock
        
        int index = y * _width + x;
        if (index < 0 || index >= _worldData.Length) return 0;
        
        return _worldData[index];
    }

        private void OnDestroy()
        {
            if (_worldData.IsCreated) _worldData.Dispose();
        }
        
        private void OnDrawGizmos()
        {
            if (!debugDraw || !_hasData) return;
            
            float cellSize = worldManager != null ? worldManager.CellSize : 0.02f;
            float worldWidth = _width * cellSize;
            float worldHeight = _height * cellSize;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(worldWidth, worldHeight, 0));
            
            #if UNITY_EDITOR
            // Draw dimension info
            UnityEditor.Handles.Label(
                new Vector3(0, worldHeight * 0.5f + 1f, 0),
                $"Collision System: {_width}×{_height}px ({worldWidth:F2}×{worldHeight:F2} units)"
            );
            #endif
        }
        
        /// <summary>
        /// Validate that the collision system matches the world dimensions
        /// </summary>
        [ContextMenu("Validate Collision System")]
        public void ValidateSystem()
        {
            if (worldManager == null)
            {
                Debug.LogError("❌ PixelCollisionSystem: No worldManager reference assigned!");
                return;
            }
            
            if (!_hasData)
            {
                Debug.LogWarning("⚠️ PixelCollisionSystem: No collision data yet (needs to run in Play mode)");
                return;
            }
            
            int expectedWidth = worldManager.Width;
            int expectedHeight = worldManager.Height;
            
            bool dimensionsMatch = (_width == expectedWidth && _height == expectedHeight);
            
            if (dimensionsMatch)
            {
                Debug.Log($"✅ Collision System Valid!\n" +
                         $"Dimensions: {_width}×{_height}px\n" +
                         $"World Size: {_width * worldManager.CellSize:F2}×{_height * worldManager.CellSize:F2} units");
            }
            else
            {
                Debug.LogError($"❌ Collision Dimension Mismatch!\n" +
                             $"Expected: {expectedWidth}×{expectedHeight}px\n" +
                             $"Current: {_width}×{_height}px\n" +
                             $"This will cause collision errors! Restart Play mode.");
            }
        }
    }
}
