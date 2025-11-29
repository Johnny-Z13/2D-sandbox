using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;

namespace PixelWorld
{
    /// <summary>
    /// Controls rendering presets for the pixel world shader.
    /// Allows quick switching between different visual styles.
    /// Attach to any GameObject and reference the world renderer.
    /// </summary>
    public class RenderingPresetController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Renderer worldRenderer;
        [SerializeField] private Volume postProcessVolume;

        [Header("Preset Selection")]
        [SerializeField] private VisualPreset currentPreset = VisualPreset.Default;
        
        [Header("On-Screen Notification")]
        [SerializeField] private bool showPresetNotification = true;
        [SerializeField] private float notificationDuration = 3f;
        
        private string _currentNotification = "";
        private float _notificationTimer = 0f;

        [Header("Manual Override")]
        [SerializeField] private bool useManualSettings = false;
        [SerializeField] private SandSettings manualSandSettings = new SandSettings
        {
            glitterIntensity = 2.0f,
            glitterScale = 20.0f,
            glitterSpeed = 1.0f,
            colorVariation = 0.15f
        };
        
        [SerializeField] private WaterSettings manualWaterSettings = new WaterSettings
        {
            shimmerIntensity = 0.8f
        };

        [Header("Bloom Settings")]
        [SerializeField] private BloomSettings manualBloomSettings = new BloomSettings
        {
            threshold = 0.6f,
            intensity = 0.4f,
            scatter = 0.8f
        };

        private Material _pixelMaterial;
        
        // Shader property IDs (cached for performance)
        private static readonly int PropGlitterIntensity = Shader.PropertyToID("_GlitterIntensity");
        private static readonly int PropGlitterScale = Shader.PropertyToID("_GlitterScale");
        private static readonly int PropGlitterSpeed = Shader.PropertyToID("_GlitterSpeed");
        private static readonly int PropSandColorVariation = Shader.PropertyToID("_SandColorVariation");
        private static readonly int PropWaterShimmer = Shader.PropertyToID("_WaterShimmer");

        public enum VisualPreset
        {
            Default,
            DesertGold,
            SubtleRealism,
            ExtremeShowcase,
            ScreenshotMode,
            PerformanceMode
        }

        [System.Serializable]
        public struct SandSettings
        {
            [Range(0f, 100f)] public float glitterIntensity;
            [Range(1f, 200f)] public float glitterScale;
            [Range(0f, 50f)] public float glitterSpeed;
            [Range(0f, 1f)] public float colorVariation;
        }

        [System.Serializable]
        public struct WaterSettings
        {
            [Range(0f, 2f)] public float shimmerIntensity;
        }

        [System.Serializable]
        public struct BloomSettings
        {
            [Range(0f, 1f)] public float threshold;
            [Range(0f, 2f)] public float intensity;
            [Range(0f, 1f)] public float scatter;
        }

        private void Start()
        {
            if (worldRenderer != null)
            {
                _pixelMaterial = worldRenderer.material;
            }
            else
            {
                Debug.LogWarning("RenderingPresetController: World Renderer not assigned!");
            }

            ApplyPreset(currentPreset);
        }

        private void OnValidate()
        {
            // Apply changes in editor when values change
            if (Application.isPlaying && _pixelMaterial != null)
            {
                if (useManualSettings)
                {
                    ApplyManualSettings();
                }
                else
                {
                    ApplyPreset(currentPreset);
                }
            }
        }

        /// <summary>
        /// Apply a predefined visual preset
        /// </summary>
        public void ApplyPreset(VisualPreset preset)
        {
            currentPreset = preset;

            SandSettings sand;
            WaterSettings water;
            BloomSettings bloom;

            switch (preset)
            {
                case VisualPreset.Default:
                    sand = new SandSettings
                    {
                        glitterIntensity = 2.0f,
                        glitterScale = 20.0f,
                        glitterSpeed = 1.0f,
                        colorVariation = 0.15f
                    };
                    water = new WaterSettings { shimmerIntensity = 0.8f };
                    bloom = new BloomSettings
                    {
                        threshold = 0.6f,
                        intensity = 0.4f,
                        scatter = 0.8f
                    };
                    break;

                case VisualPreset.DesertGold:
                    sand = new SandSettings
                    {
                        glitterIntensity = 5.0f, // Doubled for visibility
                        glitterScale = 35.0f,
                        glitterSpeed = 2.0f,
                        colorVariation = 0.3f
                    };
                    water = new WaterSettings { shimmerIntensity = 0.6f };
                    bloom = new BloomSettings
                    {
                        threshold = 0.5f,
                        intensity = 0.5f,
                        scatter = 0.85f
                    };
                    break;

                case VisualPreset.SubtleRealism:
                    sand = new SandSettings
                    {
                        glitterIntensity = 0.5f, // Much lower for contrast
                        glitterScale = 10.0f,
                        glitterSpeed = 0.2f,
                        colorVariation = 0.05f
                    };
                    water = new WaterSettings { shimmerIntensity = 0.2f };
                    bloom = new BloomSettings
                    {
                        threshold = 0.75f,
                        intensity = 0.25f,
                        scatter = 0.7f
                    };
                    break;

                case VisualPreset.ExtremeShowcase:
                    sand = new SandSettings
                    {
                        glitterIntensity = 50.0f, // MENTAL x100000 (well, x25 of default)
                        glitterScale = 80.0f,     // High frequency noise
                        glitterSpeed = 15.0f,     // Super fast
                        colorVariation = 0.8f     // Rainbow sand?
                    };
                    water = new WaterSettings { shimmerIntensity = 5.0f };
                    bloom = new BloomSettings
                    {
                        threshold = 0.1f, // Bloom everything
                        intensity = 5.0f, // Blinding bloom
                        scatter = 0.95f
                    };
                    break;

                case VisualPreset.ScreenshotMode:
                    sand = new SandSettings
                    {
                        glitterIntensity = 3.0f,
                        glitterScale = 22.0f,
                        glitterSpeed = 0.5f,
                        colorVariation = 0.2f
                    };
                    water = new WaterSettings { shimmerIntensity = 1.0f };
                    bloom = new BloomSettings
                    {
                        threshold = 0.5f,
                        intensity = 0.6f,
                        scatter = 0.85f
                    };
                    break;

                case VisualPreset.PerformanceMode:
                    sand = new SandSettings
                    {
                        glitterIntensity = 1.0f,
                        glitterScale = 15.0f,
                        glitterSpeed = 0.5f,
                        colorVariation = 0.08f
                    };
                    water = new WaterSettings { shimmerIntensity = 0.3f };
                    bloom = new BloomSettings
                    {
                        threshold = 0.8f,
                        intensity = 0.2f,
                        scatter = 0.6f
                    };
                    break;

                default:
                    return;
            }

            ApplySettings(sand, water, bloom);
        }

        private void ApplyManualSettings()
        {
            ApplySettings(manualSandSettings, manualWaterSettings, manualBloomSettings);
        }

        private void ApplySettings(SandSettings sand, WaterSettings water, BloomSettings bloom)
        {
            if (_pixelMaterial == null)
            {
                Debug.LogWarning("Cannot apply settings: Material not found!");
                return;
            }

            // Apply shader parameters
            _pixelMaterial.SetFloat(PropGlitterIntensity, sand.glitterIntensity);
            _pixelMaterial.SetFloat(PropGlitterScale, sand.glitterScale);
            _pixelMaterial.SetFloat(PropGlitterSpeed, sand.glitterSpeed);
            _pixelMaterial.SetFloat(PropSandColorVariation, sand.colorVariation);
            _pixelMaterial.SetFloat(PropWaterShimmer, water.shimmerIntensity);
            
            // Debug log to verify settings are applied
            Debug.Log($"✓ Shader settings applied - Glitter: {sand.glitterIntensity}, Scale: {sand.glitterScale}, Speed: {sand.glitterSpeed}, ColorVar: {sand.colorVariation}, Water: {water.shimmerIntensity}");

            // Apply post-processing (if volume is assigned)
            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                if (postProcessVolume.profile.TryGet<UnityEngine.Rendering.Universal.Bloom>(out var bloomEffect))
                {
                    bloomEffect.threshold.value = bloom.threshold;
                    bloomEffect.intensity.value = bloom.intensity;
                    bloomEffect.scatter.value = bloom.scatter;
                }
            }
        }

        // Public API for runtime preset switching
        public void SetPreset(int presetIndex)
        {
            if (presetIndex >= 0 && presetIndex < System.Enum.GetValues(typeof(VisualPreset)).Length)
            {
                ApplyPreset((VisualPreset)presetIndex);
            }
        }

        public void SetPresetByName(string presetName)
        {
            if (System.Enum.TryParse<VisualPreset>(presetName, out var preset))
            {
                ApplyPreset(preset);
            }
            else
            {
                Debug.LogWarning($"Unknown preset name: {presetName}");
            }
        }

        // Keyboard shortcuts - F1-F6 for graphics presets
        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.f1Key.wasPressedThisFrame) ApplyPresetWithNotification(VisualPreset.Default);
                if (Keyboard.current.f2Key.wasPressedThisFrame) ApplyPresetWithNotification(VisualPreset.DesertGold);
                if (Keyboard.current.f3Key.wasPressedThisFrame) ApplyPresetWithNotification(VisualPreset.SubtleRealism);
                if (Keyboard.current.f4Key.wasPressedThisFrame) ApplyPresetWithNotification(VisualPreset.ExtremeShowcase);
                if (Keyboard.current.f5Key.wasPressedThisFrame) ApplyPresetWithNotification(VisualPreset.ScreenshotMode);
                if (Keyboard.current.f6Key.wasPressedThisFrame) ApplyPresetWithNotification(VisualPreset.PerformanceMode);
            }
            
            // Update notification timer
            if (_notificationTimer > 0)
            {
                _notificationTimer -= Time.deltaTime;
                if (_notificationTimer <= 0)
                {
                    _currentNotification = "";
                }
            }
        }
        
        /// <summary>
        /// Apply preset and show on-screen notification
        /// </summary>
        private void ApplyPresetWithNotification(VisualPreset preset)
        {
            ApplyPreset(preset);
            AudioEventTriggers.OnPresetChange();
            
            if (showPresetNotification)
            {
                string presetName = preset.ToString();
                // Add spaces before capitals for readability
                string displayName = System.Text.RegularExpressions.Regex.Replace(presetName, "(\\B[A-Z])", " $1");
                
                _currentNotification = $"Graphics: {displayName}";
                _notificationTimer = notificationDuration;
                
                Debug.Log($"🎨 Graphics Preset: {displayName}");
            }
        }
        
        /// <summary>
        /// Draw on-screen notification using Unity GUI
        /// </summary>
        private void OnGUI()
        {
            if (_notificationTimer > 0 && !string.IsNullOrEmpty(_currentNotification))
            {
                // Create style for notification
                GUIStyle style = new GUIStyle(GUI.skin.box);
                style.fontSize = 24;
                style.fontStyle = FontStyle.Bold;
                style.normal.textColor = Color.white;
                style.normal.background = MakeColorTexture(new Color(0, 0, 0, 0.7f));
                style.alignment = TextAnchor.MiddleCenter;
                style.padding = new RectOffset(20, 20, 10, 10);
                
                // Calculate position (top center of screen)
                GUIContent content = new GUIContent(_currentNotification);
                Vector2 size = style.CalcSize(content);
                float x = (Screen.width - size.x) / 2f;
                float y = 50f; // Top of screen with padding
                
                // Fade out effect in last 0.5 seconds
                if (_notificationTimer < 0.5f)
                {
                    Color oldColor = GUI.color;
                    GUI.color = new Color(1, 1, 1, _notificationTimer * 2f); // Fade alpha
                    GUI.Box(new Rect(x, y, size.x, size.y), _currentNotification, style);
                    GUI.color = oldColor;
                }
                else
                {
                    GUI.Box(new Rect(x, y, size.x, size.y), _currentNotification, style);
                }
            }
        }
        
        /// <summary>
        /// Helper to create a solid color texture for GUI backgrounds
        /// </summary>
        private Texture2D MakeColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}
