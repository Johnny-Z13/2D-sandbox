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
        [SerializeField] private WorldSizePreset worldSizePreset = WorldSizePreset.Balanced3x3;
        private WorldSizePreset _previousPreset = WorldSizePreset.Balanced3x3;
        
        [Header("Generation Settings")]
        [SerializeField] private bool randomizeOnStart = false;
        [SerializeField] private int seed = 12345;
        [SerializeField] private WorldStyle worldStyle = WorldStyle.Organic;
        [SerializeField] private float updateRate = 0.0f; // 0 = every frame
        [Tooltip("Y-coordinate for the surface level in world units. Keeps spawn point consistent across world sizes.")]
        [SerializeField] private float surfaceHeightWorldY = 13.8f;
        
        [Header("Cave Generation")]
        [Tooltip("Threshold for cave generation. Higher = More Solid/Dense, Lower = More Hollow/Open. (0.5 is balanced)")]
        [Range(0.3f, 0.7f)]
        [SerializeField] private float caveThreshold = 0.5f;
        
        [Tooltip("Horizontal cave frequency (Cycles per screen height)")]
        [Range(2f, 20f)]
        [SerializeField] private float caveFrequencyX = 4.0f;
        
        [Tooltip("Vertical cave frequency (Cycles per screen height)")]
        [Range(2f, 20f)]
        [SerializeField] private float caveFrequencyY = 4.0f;
        
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
        [Tooltip("Frequency of sand pockets (Cycles per screen height)")]
        [Range(4f, 60f)]
        [SerializeField] private float sandFrequency = 20.0f;
        
        [Tooltip("Rarity of sand in dirt layer (0.5 = common, 0.8 = rare)")]
        [Range(0.4f, 0.9f)]
        [SerializeField] private float sandThresholdShallow = 0.6f;
        
        [Tooltip("Rarity of sand pockets deep underground (0.5 = common, 0.9 = very rare)")]
        [Range(0.3f, 0.95f)]
        [SerializeField] private float sandThresholdDeep = 0.55f;

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
        private static readonly int PropSurfaceLevel = Shader.PropertyToID("_SurfaceLevel");
        private static readonly int PropWorldStyle = Shader.PropertyToID("_WorldStyle");
        
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

        private void OnValidate()
        {
            if (worldSizePreset != _previousPreset)
            {
                // Preset changed via Inspector
                if (worldSizePreset != WorldSizePreset.Custom)
                {
                    switch (worldSizePreset)
                    {
                        case WorldSizePreset.Narrow1x6: width = 1024; height = 3072; break;
                        case WorldSizePreset.Balanced3x3: width = 3072; height = 1536; break;
                        case WorldSizePreset.Huge6x6: width = 6144; height = 3072; break;
                    }
                    
                    if (Application.isPlaying)
                    {
                        InitializeWorld();
                    }
                }
                _previousPreset = worldSizePreset;
            }
            else
            {
                // Check if manual changes invalidate the preset
                if (worldSizePreset != WorldSizePreset.Custom)
                {
                    bool match = false;
                    switch (worldSizePreset)
                    {
                        case WorldSizePreset.Narrow1x6: match = (width == 1024 && height == 3072); break;
                        case WorldSizePreset.Balanced3x3: match = (width == 3072 && height == 1536); break;
                        case WorldSizePreset.Huge6x6: match = (width == 6144 && height == 3072); break;
                    }
                    
                    if (!match)
                    {
                        worldSizePreset = WorldSizePreset.Custom;
                        _previousPreset = WorldSizePreset.Custom;
                    }
                }
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _mainCam = Camera.main;
            
            if (randomizeOnStart)
            {
                RandomizeSettings();
            }
            
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
            pixelSimShader.SetInt(PropWorldStyle, (int)worldStyle);

            // Calculate normalized surface level based on world height
            // 0.5 is center. We want surfaceHeightWorldY units above center.
            float worldHeightUnits = height * cellSize;
            float normalizedSurface = 0.5f + (surfaceHeightWorldY / worldHeightUnits);
            pixelSimShader.SetFloat(PropSurfaceLevel, normalizedSurface);
            
            // Cave generation parameters (with safety checks)
            pixelSimShader.SetFloat(PropCaveThreshold, Mathf.Clamp(caveThreshold, 0.3f, 0.8f));
            pixelSimShader.SetVector(PropCaveFrequency, new Vector2(
                Mathf.Clamp(caveFrequencyX, 1f, 64f), 
                Mathf.Clamp(caveFrequencyY, 1f, 64f)
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
                Mathf.Clamp(sandFrequency, 4f, 60f), 
                Mathf.Clamp(sandThresholdShallow, 0.2f, 0.9f), 
                Mathf.Clamp(sandThresholdDeep, 0.2f, 0.95f)
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
                case WorldPreset.Organic:
                    worldStyle = WorldStyle.Organic;
                    caveThreshold = 0.52f; // Balanced solid/hollow
                    caveFrequencyX = 6.0f; // Balanced aspect
                    caveFrequencyY = 6.0f; // Balanced aspect
                    caveLayerBlend = 0.5f;
                    waterPoolChance = 0.3f; // Occasional water
                    waterDepthThreshold = 0.3f;
                    waterNoiseThreshold = 0.5f;
                    sandFrequency = 20.0f; // Visible pockets
                    sandThresholdShallow = 0.6f;
                    sandThresholdDeep = 0.55f;
                    break;
                    
                case WorldPreset.Geometric:
                    worldStyle = WorldStyle.Geometric;
                    caveThreshold = 0.5f;
                    caveFrequencyX = 10.0f; // Block frequency
                    caveFrequencyY = 10.0f; // Block frequency
                    caveLayerBlend = 0.0f;
                    waterPoolChance = 0.2f;
                    waterDepthThreshold = 0.2f;
                    waterNoiseThreshold = 0.6f;
                    sandFrequency = 15.0f;
                    sandThresholdShallow = 0.6f;
                    sandThresholdDeep = 0.7f;
                    break;
                    
                case WorldPreset.WaterWorld:
                    worldStyle = WorldStyle.Organic;
                    caveThreshold = 0.45f;
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
                    
                case WorldPreset.Rocky:
                    worldStyle = WorldStyle.Organic;
                    caveThreshold = 0.7f; // Very dense
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
            }
            
            RegenerateWorld();
            Debug.Log($"PixelWorldManager: Loaded preset '{preset}'");
        }
        
        [ContextMenu("Load Preset: Organic")]
        private void LoadPresetOrganic() => LoadPreset(WorldPreset.Organic);
        
        [ContextMenu("Load Preset: Geometric")]
        private void LoadPresetGeometric() => LoadPreset(WorldPreset.Geometric);
        
        [ContextMenu("Load Preset: Water World")]
        private void LoadPresetWaterWorld() => LoadPreset(WorldPreset.WaterWorld);
        
        [ContextMenu("Load Preset: Rocky")]
        private void LoadPresetRocky() => LoadPreset(WorldPreset.Rocky);
        
        // === World Size Presets ===
        
        /// <summary>
        /// Load a world size preset and regenerate the world
        /// </summary>
        public void LoadWorldSizePreset(WorldSizePreset preset)
        {
            worldSizePreset = preset;
            _previousPreset = preset;

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
                case WorldSizePreset.Custom:
                    Debug.Log($"World Size: Custom - {width}×{height} pixels");
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

        [ContextMenu("🎲 Randomize Settings")]
        public void RandomizeSettings()
        {
            seed = UnityEngine.Random.Range(0, 100000);
            worldStyle = (WorldStyle)UnityEngine.Random.Range(0, 2); // Randomize Style
            
            if (worldStyle == WorldStyle.Organic)
            {
                // Cave - Organic ranges
                caveThreshold = UnityEngine.Random.Range(0.45f, 0.55f); 
                caveFrequencyX = UnityEngine.Random.Range(3f, 8f); 
                caveFrequencyY = UnityEngine.Random.Range(3f, 8f); 
                caveLayerBlend = UnityEngine.Random.Range(0.3f, 0.7f);
                
                // Sand
                sandFrequency = UnityEngine.Random.Range(10f, 30f);
            }
            else // Geometric
            {
                // Cave - Geometric ranges (slightly higher freq for blocks)
                caveThreshold = UnityEngine.Random.Range(0.45f, 0.55f);
                caveFrequencyX = UnityEngine.Random.Range(5f, 15f); 
                caveFrequencyY = UnityEngine.Random.Range(5f, 15f);
                caveLayerBlend = 0f; 
                
                // Sand
                sandFrequency = UnityEngine.Random.Range(10f, 25f);
            }
            
            // Water (Common)
            waterPoolChance = UnityEngine.Random.Range(0.2f, 0.6f);
            waterDepthThreshold = UnityEngine.Random.Range(0.2f, 0.5f);
            waterNoiseThreshold = UnityEngine.Random.Range(0.45f, 0.65f);
            
            // Sand Thresholds (Common)
            sandThresholdShallow = UnityEngine.Random.Range(0.45f, 0.75f);
            sandThresholdDeep = UnityEngine.Random.Range(0.45f, 0.75f);
            
            Debug.Log($"PixelWorldManager: Settings Randomized! Seed: {seed}, Style: {worldStyle}");
            
            if (Application.isPlaying)
            {
                InitializeWorld();
            }
        }

        [ContextMenu("💾 Save Settings")]
        public void SaveSettings()
        {
            WorldSettingsData data = new WorldSettingsData
            {
                seed = seed,
                worldStyle = (int)worldStyle,
                caveThreshold = caveThreshold,
                caveFrequencyX = caveFrequencyX,
                caveFrequencyY = caveFrequencyY,
                caveLayerBlend = caveLayerBlend,
                waterPoolChance = waterPoolChance,
                waterDepthThreshold = waterDepthThreshold,
                waterNoiseThreshold = waterNoiseThreshold,
                sandFrequency = sandFrequency,
                sandThresholdShallow = sandThresholdShallow,
                sandThresholdDeep = sandThresholdDeep,
                stabilityThreshold = stabilityThreshold
            };
            
            string json = JsonUtility.ToJson(data, true);
            string path = System.IO.Path.Combine(Application.persistentDataPath, "saved_world_settings.json");
            System.IO.File.WriteAllText(path, json);
            
            Debug.Log($"PixelWorldManager: Settings saved to {path}");
            
            // Also save to Assets folder for easy access in Editor
            #if UNITY_EDITOR
            string assetPath = System.IO.Path.Combine(Application.dataPath, "saved_world_settings.json");
            System.IO.File.WriteAllText(assetPath, json);
            Debug.Log($"PixelWorldManager: Settings also saved to {assetPath}");
            #endif
        }

        [ContextMenu("📂 Load Settings")]
        public void LoadSettings()
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, "saved_world_settings.json");
            
            #if UNITY_EDITOR
            // Prefer the one in Assets if in Editor
            string assetPath = System.IO.Path.Combine(Application.dataPath, "saved_world_settings.json");
            if (System.IO.File.Exists(assetPath))
            {
                path = assetPath;
            }
            #endif
            
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                WorldSettingsData data = JsonUtility.FromJson<WorldSettingsData>(json);
                
                seed = data.seed;
                worldStyle = (WorldStyle)data.worldStyle;
                caveThreshold = data.caveThreshold;
                caveFrequencyX = data.caveFrequencyX;
                caveFrequencyY = data.caveFrequencyY;
                caveLayerBlend = data.caveLayerBlend;
                waterPoolChance = data.waterPoolChance;
                waterDepthThreshold = data.waterDepthThreshold;
                waterNoiseThreshold = data.waterNoiseThreshold;
                sandFrequency = data.sandFrequency;
                sandThresholdShallow = data.sandThresholdShallow;
                sandThresholdDeep = data.sandThresholdDeep;
                stabilityThreshold = data.stabilityThreshold;
                
                Debug.Log($"PixelWorldManager: Settings loaded from {path}");
                
                if (Application.isPlaying)
                {
                    InitializeWorld();
                }
            }
            else
            {
                Debug.LogWarning($"PixelWorldManager: No settings file found at {path}");
            }
        }

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
        Organic,
        Geometric,
        WaterWorld,
        Rocky
    }
    
    public enum WorldStyle
    {
        Organic = 0,
        Geometric = 1
    }
    
    /// <summary>
    /// World size presets (screens wide × screens deep)
    /// 1 screen ≈ 1024×512 pixels = 20.48×10.24 units
    /// </summary>
    public enum WorldSizePreset
    {
        Custom,       // Manual dimensions
        Narrow1x6,    // 1024×3072 - Narrow vertical shaft, deep exploration
        Balanced3x3,  // 3072×1536 - Balanced exploration & digging
        Huge6x6       // 6144×3072 - Massive open world
    }
}
