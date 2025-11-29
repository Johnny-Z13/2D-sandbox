using UnityEngine;

namespace PixelWorld
{
    /// <summary>
    /// Debug tool to verify world generation is working correctly.
    /// </summary>
    public class WorldGenerationDebugger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PixelWorldManager worldManager;
        [SerializeField] private ComputeShader pixelShader;
        
        [Header("Debug Info")]
        [SerializeField] private bool logShaderProperties = false;
        
        private void Start()
        {
            if (worldManager == null)
                worldManager = FindObjectOfType<PixelWorldManager>();
                
            if (pixelShader == null && worldManager != null)
            {
                // Try to get it from world manager via reflection or just log warning
                Debug.LogWarning("WorldGenerationDebugger: Assign PixelWorld compute shader for full diagnostics");
            }
        }
        
        [ContextMenu("🔍 Check World Generation")]
        public void CheckWorldGeneration()
        {
            Debug.Log("=== World Generation Diagnostics ===");
            
            if (worldManager == null)
            {
                Debug.LogError("❌ PixelWorldManager not found!");
                return;
            }
            
            Debug.Log($"✅ World Dimensions: {worldManager.Width} x {worldManager.Height}");
            
            var currentTexture = worldManager.GetCurrentTexture();
            if (currentTexture == null)
            {
                Debug.LogError("❌ World texture is NULL! World may not be initialized.");
                return;
            }
            
            Debug.Log($"✅ World Texture: {currentTexture.width}x{currentTexture.height}, Format: {currentTexture.format}");
            Debug.Log($"✅ Texture Valid: IsCreated={currentTexture.IsCreated()}, EnableRandomWrite={currentTexture.enableRandomWrite}");
            
            // Sample a few pixels to verify world has content
            CheckWorldContent();
            
            Debug.Log("=== Diagnostics Complete ===");
        }
        
        private void CheckWorldContent()
        {
            if (worldManager == null) return;
            
            var texture = worldManager.GetCurrentTexture();
            if (texture == null || !texture.IsCreated()) return;
            
            // Request GPU readback to check content
            var request = UnityEngine.Rendering.AsyncGPUReadback.Request(texture);
            request.WaitForCompletion();
            
            if (request.hasError)
            {
                Debug.LogError("❌ GPU Readback failed!");
                return;
            }
            
            var data = request.GetData<int>();
            
            // Count material types
            int empty = 0, rock = 0, dirt = 0, sand = 0, water = 0, other = 0;
            int sampleSize = Mathf.Min(10000, data.Length); // Sample first 10k pixels
            
            for (int i = 0; i < sampleSize; i++)
            {
                int mat = data[i];
                switch (mat)
                {
                    case 0: empty++; break;
                    case 1: rock++; break;
                    case 2: dirt++; break;
                    case 3: sand++; break;
                    case 4: water++; break;
                    default: other++; break;
                }
            }
            
            Debug.Log($"📊 Sample Analysis (first {sampleSize} pixels):");
            Debug.Log($"   Empty: {empty} ({(empty * 100f / sampleSize):F1}%)");
            Debug.Log($"   Rock: {rock} ({(rock * 100f / sampleSize):F1}%)");
            Debug.Log($"   Dirt: {dirt} ({(dirt * 100f / sampleSize):F1}%)");
            Debug.Log($"   Sand: {sand} ({(sand * 100f / sampleSize):F1}%)");
            Debug.Log($"   Water: {water} ({(water * 100f / sampleSize):F1}%)");
            Debug.Log($"   Other: {other}");
            
            if (rock + dirt + sand == 0)
            {
                Debug.LogError("❌ NO SOLID GROUND DETECTED! World is all empty/water. Check shader parameters!");
            }
            else if (empty > sampleSize * 0.95f)
            {
                Debug.LogWarning("⚠️ World is >95% empty. Cave threshold might be too low or shader might not have initialized.");
            }
            else
            {
                Debug.Log("✅ World has solid ground!");
            }
        }
        
        [ContextMenu("🔄 Force World Regeneration")]
        public void ForceRegenerate()
        {
            if (worldManager != null)
            {
                worldManager.RegenerateWorld();
                Debug.Log("World regenerated!");
            }
        }
        
        [ContextMenu("📋 Log Current Parameters")]
        public void LogCurrentParameters()
        {
            if (worldManager == null) return;
            
            Debug.Log("=== Current World Generation Parameters ===");
            // Note: These are private in PixelWorldManager, so we can't access them directly
            // This is just a placeholder - in real use, make those fields public or add getters
            Debug.Log("Check PixelWorldManager Inspector for current parameter values");
            Debug.Log("===========================================");
        }
    }
}

