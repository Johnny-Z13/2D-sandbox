using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelWorld
{
    public class PixelWorldManager : MonoBehaviour
    {
        [Header("World Dimensions")]
        [Tooltip("World width in pixels (1024 = 1 screen wide)")]
        [SerializeField] private int width = 4096;
        [Tooltip("World height in pixels (512 = 1 screen tall)")]
        [SerializeField] private int height = 1536;
        [SerializeField] private float cellSize = 0.02f;
        
        [Header("World Size Presets")]
        [Tooltip("Quick-load world sizes: 1×6 (narrow/deep), 3×3 (balanced), 6×6 (huge)")]
        [SerializeField] private bool showPresetInfo = true;
        
        [Header("Generation Settings")]
        [SerializeField] private int seed = 12345;
        [SerializeField] private float updateRate = 0.0f; // 0 = every frame
        
        [Header("Cave Generation")]
        [Tooltip("How hollow the caves are (0.2 = very dense, 0.5 = very hollow)")]
        [Range(0.2f, 0.6f)]
        [SerializeField] private float caveThreshold = 0.35f;
        
        [Tooltip("Horizontal cave frequency (4 = tight, 12 = sprawling)")]
        [Range(4f, 16f)]
        [SerializeField] private float caveFrequencyX = 8.0f;
        
        [Tooltip("Vertical cave frequency (8 = flat, 24 = tall chambers)")]
        [Range(8f, 32f)]
        [SerializeField] private float caveFrequencyY = 16.0f;
        
        [Tooltip("Additional cave layer for complexity (0 = off, 1 = full)")]
        [Range(0f, 1f)]
        [SerializeField] private float caveLayerBlend = 0.5f;
        
        [Header("Water Generation")]
        [Tooltip("Chance of water pools in caves (0 = none, 1 = everywhere)")]
        [Range(0f, 1f)]
        [SerializeField] private float waterPoolChance = 0.3f;
        
        [Tooltip("How deep water pools form (0.1 = bottom only, 0.5 = mid-level too)")]
        [Range(0.05f, 0.6f)]
        [SerializeField] private float waterDepthThreshold = 0.25f;
        
        [Tooltip("Noise threshold for water spawning (0.5 = rare, 0.3 = common)")]
        [Range(0.3f, 0.7f)]
        [SerializeField] private float waterNoiseThreshold = 0.55f;
        
        [Header("Material Variety")]
        [Tooltip("Frequency of sand pockets (10 = scattered, 30 = dense)")]
        [Range(10f, 40f)]
        [SerializeField] private float sandFrequency = 20.0f;
        
        [Tooltip("Rarity of sand in dirt layer (0.5 = common, 0.8 = rare)")]
        [Range(0.4f, 0.9f)]
        [SerializeField] private float sandThresholdShallow = 0.6f;
        
        [Tooltip("Rarity of sand pockets deep underground (0.6 = common, 0.9 = very rare)")]
        [Range(0.5f, 0.95f)]
        [SerializeField] private float sandThresholdDeep = 0.7f;

        [Header("Physics Settings")]
        [Tooltip("Minimum number of solid neighbors required for a pixel to stay solid. If less, it crumbles to sand. (0=Strictly Isolated, 1=Hanging/Tips, 2=Aggressive Cleanup)")]
        [Range(0, 3)]
        [SerializeField] private int stabilityThreshold = 2;

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;

        [Header("References")]
        [SerializeField] private ComputeShader pixelSimShader;
        [SerializeField] private Renderer worldRenderer;

        // Double buffering for simulation
        private RenderTexture _worldA;
        private RenderTexture _worldB;
        private bool _useAAsSource = true;
        
        private int _kernelInit;
        private int _kernelMain;
        
        // Shader Property IDs
        private static readonly int PropWorldIn = Shader.PropertyToID("WorldIn");
        private static readonly int PropWorldOut = Shader.PropertyToID("WorldOut");
        private static readonly int PropWidth = Shader.PropertyToID("_Width");
        private static readonly int PropHeight = Shader.PropertyToID("_Height");
        private static readonly int PropTime = Shader.PropertyToID("_Time");
        private static readonly int PropSeed = Shader.PropertyToID("_Seed");
        private static readonly int PropWorldTex = Shader.PropertyToID("_WorldTex");
        private static readonly int PropMouseInput = Shader.PropertyToID("_MouseInput"); // x,y,radius,matID
        
        // Cave Generation Properties
        private static readonly int PropCaveThreshold = Shader.PropertyToID("_CaveThreshold");
        private static readonly int PropCaveFrequency = Shader.PropertyToID("_CaveFrequency"); // x, y
        private static readonly int PropCaveLayerBlend = Shader.PropertyToID("_CaveLayerBlend");
        private static readonly int PropWaterParams = Shader.PropertyToID("_WaterParams"); // x=chance, y=depthThreshold, z=noiseThreshold
        private static readonly int PropSandParams = Shader.PropertyToID("_SandParams"); // x=frequency, y=shallowThreshold, z=deepThreshold
        private static readonly int PropStabilityThreshold = Shader.PropertyToID("_StabilityThreshold");

        private float _timer;
        private Camera _mainCam;
        private Vector4 _externalInput = new Vector4(-1, -1, 0, 0);

        public static PixelWorldManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _mainCam = Camera.main;
            InitializeWorld();
        }

        private void InitializeWorld()
        {
            if (pixelSimShader == null)
            {
                Debug.LogError("PixelWorldManager: Compute Shader not assigned!");
                return;
            }

            ReleaseTexture(ref _worldA);
            ReleaseTexture(ref _worldB);

            _worldA = CreateWorldTexture();
            _worldB = CreateWorldTexture();

            _kernelInit = pixelSimShader.FindKernel("CSInit");
            _kernelMain = pixelSimShader.FindKernel("CSMain");
            
            if (_kernelInit < 0)
            {
                Debug.LogError("PixelWorldManager: CSInit kernel not found in compute shader! Shader may need to recompile.");
                return;
            }
            if (_kernelMain < 0)
            {
                Debug.LogError("PixelWorldManager: CSMain kernel not found in compute shader! Shader may need to recompile.");
                return;
            }

            // Set all shader parameters BEFORE dispatching init kernel
            pixelSimShader.SetInt(PropWidth, width);
            pixelSimShader.SetInt(PropHeight, height);
            pixelSimShader.SetInt(PropSeed, seed);
            
            // Cave generation parameters (with safety checks)
            pixelSimShader.SetFloat(PropCaveThreshold, Mathf.Clamp(caveThreshold, 0.2f, 0.6f));
            pixelSimShader.SetVector(PropCaveFrequency, new Vector2(
                Mathf.Clamp(caveFrequencyX, 4f, 16f), 
                Mathf.Clamp(caveFrequencyY, 8f, 32f)
            ));
            pixelSimShader.SetFloat(PropCaveLayerBlend, Mathf.Clamp01(caveLayerBlend));
            
            // Water generation parameters (with safety checks)
            pixelSimShader.SetVector(PropWaterParams, new Vector3(
                Mathf.Clamp01(waterPoolChance), 
                Mathf.Clamp(waterDepthThreshold, 0.05f, 0.6f), 
                Mathf.Clamp(waterNoiseThreshold, 0.3f, 0.7f)
            ));
            
            // Sand generation parameters (with safety checks)
            pixelSimShader.SetVector(PropSandParams, new Vector3(
                Mathf.Clamp(sandFrequency, 10f, 40f), 
                Mathf.Clamp(sandThresholdShallow, 0.4f, 0.9f), 
                Mathf.Clamp(sandThresholdDeep, 0.5f, 0.95f)
            ));
            
            // Set texture and dispatch
            pixelSimShader.SetTexture(_kernelInit, PropWorldOut, _worldA);
            
            Debug.Log($"PixelWorldManager: Initializing world {width}x{height} with seed {seed}");
            Debug.Log($"  Cave: threshold={caveThreshold:F2}, freq=({caveFrequencyX:F1},{caveFrequencyY:F1}), blend={caveLayerBlend:F2}");
            Debug.Log($"  Water: chance={waterPoolChance:F2}, depth<{waterDepthThreshold:F2}");
            Debug.Log($"  Sand: freq={sandFrequency:F1}");
            
            DispatchKernel(_kernelInit);
            
            Debug.Log("PixelWorldManager: World initialization complete!");

            if (worldRenderer != null)
            {
                worldRenderer.material.SetTexture(PropWorldTex, _worldA);
                // Ensure the renderer scale matches the simulation dimensions
                worldRenderer.transform.localScale = new Vector3(width * cellSize, height * cellSize, 1f);
            }
            
            _useAAsSource = true;
        }

        private RenderTexture CreateWorldTexture()
        {
            var rt = new RenderTexture(width, height, 0, RenderTextureFormat.RInt);
            rt.enableRandomWrite = true;
            rt.filterMode = FilterMode.Point;
            rt.wrapMode = TextureWrapMode.Clamp;
            rt.Create();
            return rt;
        }

        private void Update()
        {
            HandleInput();

            _timer += Time.deltaTime;
            if (updateRate > 0 && _timer < updateRate) return;
            _timer = 0;

            SimulateFrame();
        }

        public void ModifyWorld(Vector2 worldPos, float radius, int matID)
        {
            Vector2 pixelPos = WorldToPixel(worldPos);
            _externalInput = new Vector4(pixelPos.x, pixelPos.y, radius, matID);
        }

        private Vector2 WorldToPixel(Vector2 worldPos)
        {
            float halfWidth = (width * cellSize) / 2f;
            float halfHeight = (height * cellSize) / 2f;

            float pixelX = (worldPos.x + halfWidth) / cellSize;
            float pixelY = (worldPos.y + halfHeight) / cellSize;
            
            return new Vector2(pixelX, pixelY);
        }

        private void HandleInput()
        {
            // Priority: External Input > Mouse Input
            if (_externalInput.z > 0)
            {
                pixelSimShader.SetVector(PropMouseInput, _externalInput);
                _externalInput = new Vector4(-1, -1, 0, 0); // Reset
                return;
            }

            // Simple Mouse Painting using New Input System
            // Left Click = Sand (3)
            // Right Click = Water (4)
            // Middle Click = Empty (0) (Eraser)
            
            Vector4 mouseParams = new Vector4(-1, -1, 0, 0);

            if (Mouse.current != null)
            {
                bool left = Mouse.current.leftButton.isPressed;
                bool right = Mouse.current.rightButton.isPressed;
                bool middle = Mouse.current.middleButton.isPressed;

                if (left || right || middle)
                {
                    Vector2 mousePos = Mouse.current.position.ReadValue();
                    Vector3 worldPos = _mainCam.ScreenToWorldPoint(mousePos);
                    Vector2 pixelPos = WorldToPixel(worldPos);

                    int matID = 3; // Sand default
                    if (right) matID = 4; // Water
                    if (middle) matID = 0; // Empty

                    // Radius 10 pixels for pouring, larger for erasing
                    float radius = (middle) ? 20 : 10; 
                    mouseParams = new Vector4(pixelPos.x, pixelPos.y, radius, matID);
                }
            }
            
            pixelSimShader.SetVector(PropMouseInput, mouseParams);
        }

        private void SimulateFrame()
        {
            if (pixelSimShader == null) return;

            var source = _useAAsSource ? _worldA : _worldB;
            var dest = _useAAsSource ? _worldB : _worldA;

            pixelSimShader.SetInt(PropWidth, width);
            pixelSimShader.SetInt(PropHeight, height);
            pixelSimShader.SetFloat(PropTime, Time.time);
            pixelSimShader.SetInt(PropStabilityThreshold, stabilityThreshold);

            pixelSimShader.SetTexture(_kernelMain, PropWorldIn, source);
            pixelSimShader.SetTexture(_kernelMain, PropWorldOut, dest);

            DispatchKernel(_kernelMain);

            if (worldRenderer != null)
            {
                worldRenderer.material.SetTexture(PropWorldTex, dest);
            }

            _useAAsSource = !_useAAsSource;
        }

        private void DispatchKernel(int kernel)
        {
            int threadGroupsX = Mathf.CeilToInt(width / 8.0f);
            int threadGroupsY = Mathf.CeilToInt(height / 8.0f);
            pixelSimShader.Dispatch(kernel, threadGroupsX, threadGroupsY, 1);
        }

        public RenderTexture GetCurrentTexture()
        {
            // Return the one that was just WRITTEN to in the last frame
            // If _useAAsSource is true, it means next frame we read A.
            // So currently, the valid data is in A. 
            // Wait, SimulateFrame swaps at the end.
            // Start: Read A, Write B. End: Swap (Source becomes B).
            // So valid data is in the NEW Source.
            return _useAAsSource ? _worldA : _worldB;
        }

        /// <summary>
        /// Regenerate the world with current settings (useful for testing different configurations)
        /// </summary>
        [ContextMenu("Regenerate World")]
        public void RegenerateWorld()
        {
            InitializeWorld();
            Debug.Log("PixelWorldManager: World regenerated with current settings!");
        }
        
        /// <summary>
        /// Load a preset configuration for quick testing
        /// </summary>
        public void LoadPreset(WorldPreset preset)
        {
            switch (preset)
            {
                case WorldPreset.DefaultBalanced:
                    caveThreshold = 0.35f;
                    caveFrequencyX = 8.0f;
                    caveFrequencyY = 16.0f;
                    caveLayerBlend = 0.5f;
                    waterPoolChance = 0.3f;
                    waterDepthThreshold = 0.25f;
                    waterNoiseThreshold = 0.55f;
                    sandFrequency = 20.0f;
                    sandThresholdShallow = 0.6f;
                    sandThresholdDeep = 0.7f;
                    break;
                    
                case WorldPreset.CaveExplorer:
                    caveThreshold = 0.25f; // Very hollow
                    caveFrequencyX = 10.0f;
                    caveFrequencyY = 20.0f;
                    caveLayerBlend = 0.8f; // Complex cave systems
                    waterPoolChance = 0.5f;
                    waterDepthThreshold = 0.35f;
                    waterNoiseThreshold = 0.5f;
                    sandFrequency = 25.0f;
                    sandThresholdShallow = 0.5f;
                    sandThresholdDeep = 0.6f;
                    break;
                    
                case WorldPreset.DenseSolid:
                    caveThreshold = 0.5f; // Very dense
                    caveFrequencyX = 6.0f;
                    caveFrequencyY = 12.0f;
                    caveLayerBlend = 0.2f;
                    waterPoolChance = 0.15f;
                    waterDepthThreshold = 0.15f;
                    waterNoiseThreshold = 0.65f;
                    sandFrequency = 15.0f;
                    sandThresholdShallow = 0.7f;
                    sandThresholdDeep = 0.8f;
                    break;
                    
                case WorldPreset.UnderwaterCaves:
                    caveThreshold = 0.32f;
                    caveFrequencyX = 9.0f;
                    caveFrequencyY = 18.0f;
                    caveLayerBlend = 0.6f;
                    waterPoolChance = 0.8f; // Lots of water!
                    waterDepthThreshold = 0.5f; // Water even mid-level
                    waterNoiseThreshold = 0.4f;
                    sandFrequency = 30.0f;
                    sandThresholdShallow = 0.45f; // More sand (underwater beaches)
                    sandThresholdDeep = 0.55f;
                    break;
                    
                case WorldPreset.DesertCaves:
                    caveThreshold = 0.38f;
                    caveFrequencyX = 7.0f;
                    caveFrequencyY = 14.0f;
                    caveLayerBlend = 0.4f;
                    waterPoolChance = 0.1f; // Very little water
                    waterDepthThreshold = 0.1f;
                    waterNoiseThreshold = 0.7f;
                    sandFrequency = 35.0f; // Lots of sand!
                    sandThresholdShallow = 0.4f;
                    sandThresholdDeep = 0.5f;
                    break;
            }
            
            RegenerateWorld();
            Debug.Log($"PixelWorldManager: Loaded preset '{preset}'");
        }
        
        [ContextMenu("Load Preset: Default Balanced")]
        private void LoadPresetDefault() => LoadPreset(WorldPreset.DefaultBalanced);
        
        [ContextMenu("Load Preset: Cave Explorer")]
        private void LoadPresetCaveExplorer() => LoadPreset(WorldPreset.CaveExplorer);
        
        [ContextMenu("Load Preset: Dense Solid")]
        private void LoadPresetDenseSolid() => LoadPreset(WorldPreset.DenseSolid);
        
        [ContextMenu("Load Preset: Underwater Caves")]
        private void LoadPresetUnderwaterCaves() => LoadPreset(WorldPreset.UnderwaterCaves);
        
        [ContextMenu("Load Preset: Desert Caves")]
        private void LoadPresetDesertCaves() => LoadPreset(WorldPreset.DesertCaves);
        
        // === World Size Presets ===
        
        /// <summary>
        /// Load a world size preset and regenerate the world
        /// </summary>
        public void LoadWorldSizePreset(WorldSizePreset preset)
        {
            switch (preset)
            {
                case WorldSizePreset.Narrow1x6:
                    width = 1024;
                    height = 3072;
                    Debug.Log("World Size: 1×6 (Narrow & Deep) - 1024×3072 pixels");
                    break;
                    
                case WorldSizePreset.Balanced3x3:
                    width = 3072;
                    height = 1536;
                    Debug.Log("World Size: 3×3 (Balanced) - 3072×1536 pixels");
                    break;
                    
                case WorldSizePreset.Huge6x6:
                    width = 6144;
                    height = 3072;
                    Debug.Log("World Size: 6×6 (Huge) - 6144×3072 pixels");
                    break;
            }
            
            // Regenerate world with new dimensions
            InitializeWorld();
            
            // Update renderer scale if it exists
            if (worldRenderer != null)
            {
                worldRenderer.transform.localScale = new Vector3(width * cellSize, height * cellSize, 1f);
            }
            
            Debug.Log($"World regenerated: {width}×{height} pixels = {width * cellSize:F2}×{height * cellSize:F2} units");
        }
        
        [ContextMenu("⚡ World Size: 1×6 (Narrow & Deep)")]
        private void LoadWorldSize1x6() => LoadWorldSizePreset(WorldSizePreset.Narrow1x6);
        
        [ContextMenu("⚡ World Size: 3×3 (Balanced)")]
        private void LoadWorldSize3x3() => LoadWorldSizePreset(WorldSizePreset.Balanced3x3);
        
        [ContextMenu("⚡ World Size: 6×6 (Huge)")]
        private void LoadWorldSize6x6() => LoadWorldSizePreset(WorldSizePreset.Huge6x6);

        private void OnDestroy()
        {
            ReleaseTexture(ref _worldA);
            ReleaseTexture(ref _worldB);
        }

        private void ReleaseTexture(ref RenderTexture rt)
        {
            if (rt != null)
            {
                rt.Release();
                rt = null;
            }
        }
    }
    
    /// <summary>
    /// Preset configurations for different world types
    /// </summary>
    public enum WorldPreset
    {
        DefaultBalanced,
        CaveExplorer,
        DenseSolid,
        UnderwaterCaves,
        DesertCaves
    }
    
    /// <summary>
    /// World size presets (screens wide × screens deep)
    /// 1 screen ≈ 1024×512 pixels = 20.48×10.24 units
    /// </summary>
    public enum WorldSizePreset
    {
        Narrow1x6,    // 1024×3072 - Narrow vertical shaft, deep exploration
        Balanced3x3,  // 3072×1536 - Balanced exploration & digging
        Huge6x6       // 6144×3072 - Massive open world
    }
}
