using UnityEngine;

namespace PixelWorld
{
    /// <summary>
    /// Calculates proper world dimensions based on desired screen coverage.
    /// Helps ensure the world is large enough for player exploration.
    /// </summary>
    [ExecuteInEditMode]
    public class WorldDimensionCalculator : MonoBehaviour
    {
        [Header("Desired Play Area")]
        [Tooltip("How many screens wide should the play area be?")]
        [SerializeField] private int screensWide = 3;
        
        [Tooltip("How many screens tall/deep should the play area be?")]
        [SerializeField] private int screensDeep = 3;
        
        [Header("Camera Reference")]
        [Tooltip("The main camera (must be orthographic)")]
        [SerializeField] private Camera mainCamera;
        
        [Header("World Settings")]
        [Tooltip("World units per pixel (must match PixelWorldManager)")]
        [SerializeField] private float cellSize = 0.02f;
        
        [Header("Calculated Dimensions (Read-Only)")]
        [SerializeField] private float cameraHeight;
        [SerializeField] private float cameraWidth;
        [SerializeField] private float totalWorldWidth;
        [SerializeField] private float totalWorldHeight;
        [SerializeField] private int requiredPixelWidth;
        [SerializeField] private int requiredPixelHeight;
        
        [Header("Current vs Required")]
        [SerializeField] private PixelWorldManager worldManager;
        [SerializeField] private int currentPixelWidth;
        [SerializeField] private int currentPixelHeight;
        [SerializeField] private bool dimensionsMatch = false;

        private void OnValidate()
        {
            Calculate();
        }

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            Calculate();
            ValidateAgainstWorldManager();
        }

        [ContextMenu("Calculate Required Dimensions")]
        public void Calculate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                {
                    Debug.LogWarning("WorldDimensionCalculator: No camera assigned!");
                    return;
                }
            }

            if (!mainCamera.orthographic)
            {
                Debug.LogError("WorldDimensionCalculator: Camera must be orthographic!");
                return;
            }

            // Calculate single screen dimensions
            cameraHeight = mainCamera.orthographicSize * 2f; // orthoSize is half-height
            cameraWidth = cameraHeight * mainCamera.aspect;

            // Calculate total world dimensions for N screens
            totalWorldWidth = cameraWidth * screensWide;
            totalWorldHeight = cameraHeight * screensDeep;

            // Convert to pixel dimensions
            requiredPixelWidth = Mathf.CeilToInt(totalWorldWidth / cellSize);
            requiredPixelHeight = Mathf.CeilToInt(totalWorldHeight / cellSize);

            // Round to nice numbers (multiples of 64 for good compute dispatch)
            requiredPixelWidth = ((requiredPixelWidth + 63) / 64) * 64;
            requiredPixelHeight = ((requiredPixelHeight + 63) / 64) * 64;

            Debug.Log($"<b>World Dimension Calculator</b>\n" +
                     $"Single Screen: {cameraWidth:F2} × {cameraHeight:F2} units\n" +
                     $"Play Area ({screensWide}×{screensDeep} screens): {totalWorldWidth:F2} × {totalWorldHeight:F2} units\n" +
                     $"<b>Required Pixel Dimensions: {requiredPixelWidth} × {requiredPixelHeight}</b>");
        }

        [ContextMenu("Apply To World Manager")]
        public void ApplyToWorldManager()
        {
            if (worldManager == null)
            {
                worldManager = FindObjectOfType<PixelWorldManager>();
                if (worldManager == null)
                {
                    Debug.LogError("WorldDimensionCalculator: No PixelWorldManager found!");
                    return;
                }
            }

            Calculate();

            Debug.LogWarning($"<b>MANUAL ACTION REQUIRED:</b>\n" +
                           $"Set PixelWorldManager dimensions to:\n" +
                           $"Width: {requiredPixelWidth}\n" +
                           $"Height: {requiredPixelHeight}\n\n" +
                           $"(This script cannot modify serialized fields automatically in edit mode)");
        }

        [ContextMenu("Validate Against World Manager")]
        public void ValidateAgainstWorldManager()
        {
            if (worldManager == null)
            {
                worldManager = FindObjectOfType<PixelWorldManager>();
                if (worldManager == null) return;
            }

            Calculate();

            currentPixelWidth = worldManager.Width;
            currentPixelHeight = worldManager.Height;

            dimensionsMatch = (currentPixelWidth >= requiredPixelWidth && 
                             currentPixelHeight >= requiredPixelHeight);

            if (dimensionsMatch)
            {
                Debug.Log($"✅ World dimensions are sufficient!\n" +
                         $"Current: {currentPixelWidth}×{currentPixelHeight}\n" +
                         $"Required: {requiredPixelWidth}×{requiredPixelHeight}");
            }
            else
            {
                Debug.LogWarning($"⚠️ World dimensions are TOO SMALL!\n" +
                               $"Current: {currentPixelWidth}×{currentPixelHeight}\n" +
                               $"<b>Required: {requiredPixelWidth}×{requiredPixelHeight}</b>\n\n" +
                               $"Player will run out of world when exploring!\n" +
                               $"Set PixelWorldManager to required dimensions.");
            }
        }

        private void OnDrawGizmos()
        {
            if (mainCamera == null || worldManager == null) return;

            Calculate();

            // Draw required play area
            Gizmos.color = Color.green;
            Vector3 center = Vector3.zero;
            Vector3 size = new Vector3(totalWorldWidth, totalWorldHeight, 0.1f);
            Gizmos.DrawWireCube(center, size);

            // Draw current world size
            float currentWidth = worldManager.Width * cellSize;
            float currentHeight = worldManager.Height * cellSize;
            
            Gizmos.color = dimensionsMatch ? Color.cyan : Color.red;
            Vector3 currentSize = new Vector3(currentWidth, currentHeight, 0.2f);
            Gizmos.DrawWireCube(center, currentSize);

            // Draw screen grid
            Gizmos.color = new Color(1f, 1f, 0f, 0.5f);
            
            // Vertical lines
            for (int i = 0; i <= screensWide; i++)
            {
                float x = -totalWorldWidth / 2f + (cameraWidth * i);
                Vector3 start = new Vector3(x, -totalWorldHeight / 2f, 0);
                Vector3 end = new Vector3(x, totalWorldHeight / 2f, 0);
                Gizmos.DrawLine(start, end);
            }
            
            // Horizontal lines
            for (int i = 0; i <= screensDeep; i++)
            {
                float y = -totalWorldHeight / 2f + (cameraHeight * i);
                Vector3 start = new Vector3(-totalWorldWidth / 2f, y, 0);
                Vector3 end = new Vector3(totalWorldWidth / 2f, y, 0);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}

