using UnityEngine;
using UnityEngine.InputSystem;

namespace PixelWorld
{
    /// <summary>
    /// Quick test script to verify graphics presets are working correctly.
    /// Attach to any GameObject in the scene with RenderingPresetController.
    /// </summary>
    public class GraphicsPresetTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool enableLogging = true;
        [SerializeField] private Renderer worldRenderer;
        
        private Material _pixelMaterial;
        private float _lastGlitterIntensity = -1f;
        
        // Shader property IDs (must match RenderingPresetController)
        private static readonly int PropGlitterIntensity = Shader.PropertyToID("_GlitterIntensity");
        private static readonly int PropGlitterScale = Shader.PropertyToID("_GlitterScale");
        private static readonly int PropGlitterSpeed = Shader.PropertyToID("_GlitterSpeed");
        private static readonly int PropSandColorVariation = Shader.PropertyToID("_SandColorVariation");
        private static readonly int PropWaterShimmer = Shader.PropertyToID("_WaterShimmer");
        
        private void Start()
        {
            if (worldRenderer != null)
            {
                _pixelMaterial = worldRenderer.material;
                Debug.Log("GraphicsPresetTester: Ready to monitor shader property changes");
            }
            else
            {
                Debug.LogWarning("GraphicsPresetTester: World Renderer not assigned!");
            }
        }
        
        private void Update()
        {
            if (_pixelMaterial == null || !enableLogging) return;
            
            // Check if F-keys are pressed and log current state
            if (Keyboard.current != null)
            {
                bool keyPressed = false;
                string keyName = "";
                
                if (Keyboard.current.f1Key.wasPressedThisFrame) { keyPressed = true; keyName = "F1"; }
                if (Keyboard.current.f2Key.wasPressedThisFrame) { keyPressed = true; keyName = "F2"; }
                if (Keyboard.current.f3Key.wasPressedThisFrame) { keyPressed = true; keyName = "F3"; }
                if (Keyboard.current.f4Key.wasPressedThisFrame) { keyPressed = true; keyName = "F4"; }
                if (Keyboard.current.f5Key.wasPressedThisFrame) { keyPressed = true; keyName = "F5"; }
                if (Keyboard.current.f6Key.wasPressedThisFrame) { keyPressed = true; keyName = "F6"; }
                
                if (keyPressed)
                {
                    // Wait a frame for settings to apply, then log
                    StartCoroutine(LogShaderPropertiesAfterFrame(keyName));
                }
            }
            
            // Monitor for any changes to glitter intensity (indicates preset change)
            float currentGlitter = _pixelMaterial.GetFloat(PropGlitterIntensity);
            if (Mathf.Abs(currentGlitter - _lastGlitterIntensity) > 0.01f)
            {
                _lastGlitterIntensity = currentGlitter;
                if (enableLogging)
                {
                    Debug.Log($"🔄 Graphics setting changed detected! Glitter Intensity now: {currentGlitter}");
                }
            }
        }
        
        private System.Collections.IEnumerator LogShaderPropertiesAfterFrame(string keyName)
        {
            // Wait one frame for RenderingPresetController to apply settings
            yield return null;
            
            if (_pixelMaterial != null)
            {
                float glitterIntensity = _pixelMaterial.GetFloat(PropGlitterIntensity);
                float glitterScale = _pixelMaterial.GetFloat(PropGlitterScale);
                float glitterSpeed = _pixelMaterial.GetFloat(PropGlitterSpeed);
                float colorVariation = _pixelMaterial.GetFloat(PropSandColorVariation);
                float waterShimmer = _pixelMaterial.GetFloat(PropWaterShimmer);
                
                Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.Log($"🎨 {keyName} PRESSED - Current Shader Properties:");
                Debug.Log($"  • Glitter Intensity: {glitterIntensity}");
                Debug.Log($"  • Glitter Scale: {glitterScale}");
                Debug.Log($"  • Glitter Speed: {glitterSpeed}");
                Debug.Log($"  • Color Variation: {colorVariation}");
                Debug.Log($"  • Water Shimmer: {waterShimmer}");
                Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
        }
        
        /// <summary>
        /// Manually log current shader properties (call from inspector or other scripts)
        /// </summary>
        [ContextMenu("Log Current Shader Properties")]
        public void LogCurrentProperties()
        {
            if (_pixelMaterial != null)
            {
                float glitterIntensity = _pixelMaterial.GetFloat(PropGlitterIntensity);
                float glitterScale = _pixelMaterial.GetFloat(PropGlitterScale);
                float glitterSpeed = _pixelMaterial.GetFloat(PropGlitterSpeed);
                float colorVariation = _pixelMaterial.GetFloat(PropSandColorVariation);
                float waterShimmer = _pixelMaterial.GetFloat(PropWaterShimmer);
                
                Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.Log($"📊 Current Shader Properties:");
                Debug.Log($"  • Glitter Intensity: {glitterIntensity}");
                Debug.Log($"  • Glitter Scale: {glitterScale}");
                Debug.Log($"  • Glitter Speed: {glitterSpeed}");
                Debug.Log($"  • Color Variation: {colorVariation}");
                Debug.Log($"  • Water Shimmer: {waterShimmer}");
                Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            }
            else
            {
                Debug.LogWarning("Cannot log properties: Material not found!");
            }
        }
        
        /// <summary>
        /// Test all presets in sequence (call from context menu)
        /// </summary>
        [ContextMenu("Test All Presets (Auto-Cycle)")]
        public void TestAllPresetsSequence()
        {
            StartCoroutine(CycleAllPresets());
        }
        
        private System.Collections.IEnumerator CycleAllPresets()
        {
            var controller = GetComponent<RenderingPresetController>();
            if (controller == null)
            {
                Debug.LogError("RenderingPresetController not found on this GameObject!");
                yield break;
            }
            
            Debug.Log("🔄 Starting auto-cycle of all presets...");
            
            string[] presets = { "Default", "DesertGold", "SubtleRealism", "ExtremeShowcase", "ScreenshotMode", "PerformanceMode" };
            
            foreach (string presetName in presets)
            {
                Debug.Log($"⏩ Testing preset: {presetName}");
                controller.SetPresetByName(presetName);
                yield return new WaitForSeconds(3.5f); // Wait for notification to display + fade
                LogCurrentProperties();
            }
            
            Debug.Log("✅ Auto-cycle complete!");
        }
    }
}



