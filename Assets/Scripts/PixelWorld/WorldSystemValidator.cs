using UnityEngine;

namespace PixelWorld
{
    /// <summary>
    /// Validates that all pixel world systems are properly configured and synchronized.
    /// Use this to diagnose collision issues, rendering problems, and dimension mismatches.
    /// </summary>
    public class WorldSystemValidator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PixelWorldManager worldManager;
        [SerializeField] private PixelCollisionSystem collisionSystem;
        [SerializeField] private Transform worldRenderer;
        
        [Header("Settings")]
        [SerializeField] private float cellSize = 0.02f;
        
        [Header("Auto-Find")]
        [SerializeField] private bool autoFindReferences = true;

        private void Start()
        {
            if (autoFindReferences)
            {
                FindReferences();
            }
        }

        [ContextMenu("Find All References")]
        public void FindReferences()
        {
            if (worldManager == null)
                worldManager = FindObjectOfType<PixelWorldManager>();
            
            if (collisionSystem == null)
                collisionSystem = FindObjectOfType<PixelCollisionSystem>();
            
            if (worldRenderer == null)
            {
                // Find renderer by name or tag
                var renderers = FindObjectsOfType<Renderer>();
                foreach (var r in renderers)
                {
                    if (r.name.Contains("World") || r.name.Contains("Pixel"))
                    {
                        worldRenderer = r.transform;
                        break;
                    }
                }
            }
            
            Debug.Log($"References found:\n" +
                     $"WorldManager: {(worldManager != null ? "✅" : "❌")}\n" +
                     $"CollisionSystem: {(collisionSystem != null ? "✅" : "❌")}\n" +
                     $"WorldRenderer: {(worldRenderer != null ? "✅" : "❌")}");
        }

        [ContextMenu("🔍 Validate All Systems")]
        public void ValidateAll()
        {
            Debug.Log("=== PIXEL WORLD SYSTEM VALIDATION ===\n");
            
            FindReferences();
            
            bool allValid = true;
            
            // 1. Check PixelWorldManager
            if (worldManager == null)
            {
                Debug.LogError("❌ CRITICAL: PixelWorldManager not found!");
                allValid = false;
            }
            else
            {
                ValidateWorldManager();
            }
            
            // 2. Check World Renderer
            if (worldRenderer == null)
            {
                Debug.LogError("❌ CRITICAL: World Renderer not found!");
                allValid = false;
            }
            else
            {
                ValidateWorldRenderer();
            }
            
            // 3. Check Collision System
            if (collisionSystem == null)
            {
                Debug.LogWarning("⚠️ WARNING: PixelCollisionSystem not found (player collision won't work)");
                allValid = false;
            }
            else
            {
                ValidateCollisionSystem();
            }
            
            // 4. Cross-validate dimensions
            if (worldManager != null && worldRenderer != null)
            {
                ValidateDimensionMatch();
            }
            
            Debug.Log("\n=== VALIDATION COMPLETE ===");
            
            if (allValid)
            {
                Debug.Log("✅ ALL SYSTEMS OPERATIONAL!");
            }
            else
            {
                Debug.LogWarning("⚠️ Some issues detected - see above for details");
            }
        }

        private void ValidateWorldManager()
        {
            Debug.Log($"<b>PixelWorldManager:</b>\n" +
                     $"  Dimensions: {worldManager.Width}×{worldManager.Height}px\n" +
                     $"  World Size: {worldManager.Width * cellSize:F2}×{worldManager.Height * cellSize:F2} units\n" +
                     $"  Status: ✅ OK");
        }

        private void ValidateWorldRenderer()
        {
            Vector3 scale = worldRenderer.localScale;
            
            float expectedWidth = worldManager.Width * cellSize;
            float expectedHeight = worldManager.Height * cellSize;
            
            bool widthMatch = Mathf.Abs(scale.x - expectedWidth) < 0.1f;
            bool heightMatch = Mathf.Abs(scale.y - expectedHeight) < 0.1f;
            
            if (widthMatch && heightMatch)
            {
                Debug.Log($"<b>World Renderer:</b>\n" +
                         $"  Scale: {scale.x:F2}×{scale.y:F2}\n" +
                         $"  Expected: {expectedWidth:F2}×{expectedHeight:F2}\n" +
                         $"  Status: ✅ CORRECT");
            }
            else
            {
                Debug.LogError($"<b>World Renderer:</b>\n" +
                              $"  Scale: {scale.x:F2}×{scale.y:F2}\n" +
                              $"  Expected: {expectedWidth:F2}×{expectedHeight:F2}\n" +
                              $"  Status: ❌ MISMATCH!\n" +
                              $"  <b>FIX:</b> Add WorldRendererScaler component or manually set scale to ({expectedWidth:F2}, {expectedHeight:F2}, 1)");
            }
        }

        private void ValidateCollisionSystem()
        {
            if (!Application.isPlaying)
            {
                Debug.Log($"<b>PixelCollisionSystem:</b>\n" +
                         $"  Status: ⚠️ Not running (enter Play mode to test)");
                return;
            }
            
            if (!collisionSystem.HasData)
            {
                Debug.LogWarning($"<b>PixelCollisionSystem:</b>\n" +
                               $"  Status: ⚠️ No collision data yet (wait a frame)");
                return;
            }
            
            // Test a known ground position
            Vector2 testPosGround = new Vector2(0, -4); // Should be solid
            Vector2 testPosAir = new Vector2(0, 4); // Should be empty
            
            bool groundSolid = collisionSystem.IsSolid(testPosGround);
            bool airSolid = collisionSystem.IsSolid(testPosAir);
            
            if (groundSolid && !airSolid)
            {
                Debug.Log($"<b>PixelCollisionSystem:</b>\n" +
                         $"  Ground test: {testPosGround} = {(groundSolid ? "Solid ✅" : "Empty ❌")}\n" +
                         $"  Air test: {testPosAir} = {(airSolid ? "Solid ❌" : "Empty ✅")}\n" +
                         $"  Status: ✅ WORKING");
            }
            else
            {
                Debug.LogError($"<b>PixelCollisionSystem:</b>\n" +
                             $"  Ground test: {testPosGround} = {(groundSolid ? "Solid" : "Empty")} {(groundSolid ? "✅" : "❌")}\n" +
                             $"  Air test: {testPosAir} = {(airSolid ? "Solid" : "Empty")} {(airSolid ? "❌" : "✅")}\n" +
                             $"  Status: ❌ COLLISION NOT WORKING!\n" +
                             $"  <b>FIX:</b> Check PixelCollisionSystem has worldManager reference");
            }
        }

        private void ValidateDimensionMatch()
        {
            Vector3 scale = worldRenderer.localScale;
            int width = worldManager.Width;
            int height = worldManager.Height;
            
            float worldWidth = width * cellSize;
            float worldHeight = height * cellSize;
            
            bool match = (Mathf.Abs(scale.x - worldWidth) < 0.1f && 
                         Mathf.Abs(scale.y - worldHeight) < 0.1f);
            
            if (!match)
            {
                Debug.LogError($"\n<b>❌ DIMENSION MISMATCH DETECTED!</b>\n" +
                             $"PixelWorldManager: {width}×{height}px → {worldWidth:F2}×{worldHeight:F2} units\n" +
                             $"World Renderer Scale: {scale.x:F2}×{scale.y:F2} units\n\n" +
                             $"<b>THIS WILL CAUSE:</b>\n" +
                             $"  • Collision issues (player falls through ground)\n" +
                             $"  • Visual/physics mismatch\n" +
                             $"  • Incorrect world coverage\n\n" +
                             $"<b>HOW TO FIX:</b>\n" +
                             $"  1. Select World Renderer GameObject: '{worldRenderer.name}'\n" +
                             $"  2. Add Component → WorldRendererScaler\n" +
                             $"  3. Assign PixelWorldManager reference\n" +
                             $"  4. Play the scene (auto-fixes)\n\n" +
                             $"  OR manually set scale to: ({worldWidth:F2}, {worldHeight:F2}, 1)");
            }
        }

        [ContextMenu("🔧 Quick Fix - Add WorldRendererScaler")]
        public void QuickFixAddScaler()
        {
            if (worldRenderer == null)
            {
                Debug.LogError("❌ No world renderer found! Run 'Find All References' first.");
                return;
            }
            
            var scaler = worldRenderer.GetComponent<WorldRendererScaler>();
            if (scaler == null)
            {
                scaler = worldRenderer.gameObject.AddComponent<WorldRendererScaler>();
                Debug.Log($"✅ Added WorldRendererScaler to '{worldRenderer.name}'");
            }
            else
            {
                Debug.Log($"✅ WorldRendererScaler already exists on '{worldRenderer.name}'");
            }
        }

        private void OnDrawGizmos()
        {
            if (worldManager == null || worldRenderer == null) return;
            
            // Draw expected renderer bounds (green)
            float expectedWidth = worldManager.Width * cellSize;
            float expectedHeight = worldManager.Height * cellSize;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(expectedWidth, expectedHeight, 0));
            
            // Draw actual renderer bounds (cyan/red)
            Vector3 scale = worldRenderer.localScale;
            bool match = (Mathf.Abs(scale.x - expectedWidth) < 0.1f && 
                         Mathf.Abs(scale.y - expectedHeight) < 0.1f);
            
            Gizmos.color = match ? Color.cyan : Color.red;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(scale.x, scale.y, 0.2f));
            
            #if UNITY_EDITOR
            // Draw labels
            UnityEditor.Handles.color = match ? Color.green : Color.red;
            UnityEditor.Handles.Label(
                new Vector3(0, expectedHeight * 0.5f + 2f, 0),
                match ? "✅ Renderer matches world dimensions" : "❌ DIMENSION MISMATCH!"
            );
            #endif
        }
    }
}

