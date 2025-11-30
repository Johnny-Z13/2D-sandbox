using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PixelWorld
{
    /// <summary>
    /// Helper script to automatically setup graphics presets in the scene.
    /// Use: Unity Menu → PixelWorld → Setup Graphics Presets
    /// </summary>
    #if UNITY_EDITOR
    public class GraphicsSetupHelper
    {
        [MenuItem("PixelWorld/Setup Graphics Presets")]
        static void SetupGraphicsPresets()
        {
            Debug.Log("🔧 Starting Graphics Presets setup...");
            
            // Find PixelWorldManager
            var worldManager = Object.FindFirstObjectByType<PixelWorldManager>();
            if (worldManager == null)
            {
                Debug.LogError("❌ PixelWorldManager not found in scene! Please add it first.");
                EditorUtility.DisplayDialog("Setup Failed", 
                    "PixelWorldManager not found in scene!\n\nPlease make sure you have PixelWorldManager in your scene before setting up graphics presets.", 
                    "OK");
                return;
            }
            
            Debug.Log($"✓ Found PixelWorldManager on GameObject: {worldManager.name}");
            
            // Add RenderingPresetController if not present
            var controller = worldManager.GetComponent<RenderingPresetController>();
            if (controller == null)
            {
                controller = worldManager.gameObject.AddComponent<RenderingPresetController>();
                Debug.Log("✅ Added RenderingPresetController to " + worldManager.name);
            }
            else
            {
                Debug.Log("✓ RenderingPresetController already exists");
            }
            
            // Find World Renderer
            var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            Renderer worldRenderer = null;
            
            Debug.Log($"Searching through {renderers.Length} renderers...");
            
            foreach (var r in renderers)
            {
                Debug.Log($"  Checking: {r.gameObject.name}");
                if (r.gameObject.name.Contains("World") || 
                    r.gameObject.name.Contains("Renderer") ||
                    r.gameObject.name.Contains("Pixel"))
                {
                    worldRenderer = r;
                    Debug.Log($"  ✓ Found potential World Renderer: {r.gameObject.name}");
                    break;
                }
            }
            
            // If not found by name, try to find the one with PixelWorldManager
            if (worldRenderer == null)
            {
                var managerRenderer = worldManager.GetComponentInChildren<Renderer>();
                if (managerRenderer != null)
                {
                    worldRenderer = managerRenderer;
                    Debug.Log($"✓ Found World Renderer as child of PixelWorldManager: {worldRenderer.gameObject.name}");
                }
            }
            
            if (worldRenderer != null)
            {
                // Use SerializedObject to properly set the field
                SerializedObject so = new SerializedObject(controller);
                SerializedProperty worldRendererProp = so.FindProperty("worldRenderer");
                
                if (worldRendererProp != null)
                {
                    worldRendererProp.objectReferenceValue = worldRenderer;
                    so.ApplyModifiedProperties();
                    Debug.Log($"✅ Assigned World Renderer: {worldRenderer.gameObject.name}");
                }
                else
                {
                    Debug.LogWarning("Could not find worldRenderer property");
                }
                
                // Set notification settings
                SerializedProperty showNotificationProp = so.FindProperty("showPresetNotification");
                SerializedProperty durationProp = so.FindProperty("notificationDuration");
                
                if (showNotificationProp != null)
                {
                    showNotificationProp.boolValue = true;
                    Debug.Log("✅ Enabled preset notifications");
                }
                
                if (durationProp != null)
                {
                    durationProp.floatValue = 3f;
                    Debug.Log("✅ Set notification duration to 3 seconds");
                }
                
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(controller);
                
                Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.Log("✅ Graphics Presets setup COMPLETE!");
                Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.Log("🎮 Press Play and try F1-F6 to change graphics");
                Debug.Log("📺 You should see on-screen notifications");
                Debug.Log("✨ Visual effects should change");
                Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                
                EditorUtility.DisplayDialog("Setup Complete!", 
                    $"Graphics Presets setup successfully!\n\n" +
                    $"✓ RenderingPresetController added\n" +
                    $"✓ World Renderer assigned: {worldRenderer.gameObject.name}\n" +
                    $"✓ Notifications enabled\n\n" +
                    $"Press Play and try F1-F6 keys to test!", 
                    "Awesome!");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find World Renderer automatically");
                Debug.LogWarning("Please assign it manually:");
                Debug.LogWarning("1. Select " + worldManager.name + " in Hierarchy");
                Debug.LogWarning("2. Find 'World Renderer' field in RenderingPresetController");
                Debug.LogWarning("3. Drag the GameObject with the world mesh/renderer into that field");
                
                EditorUtility.DisplayDialog("Manual Setup Required", 
                    "RenderingPresetController added, but World Renderer needs manual assignment.\n\n" +
                    "Steps:\n" +
                    "1. Select '" + worldManager.name + "' in Hierarchy\n" +
                    "2. Find 'World Renderer' field in Inspector\n" +
                    "3. Drag your world renderer GameObject into that field\n\n" +
                    "Look for a GameObject with MeshRenderer that displays the pixel world.", 
                    "Got it");
                
                // Select the GameObject so user can configure it
                Selection.activeGameObject = worldManager.gameObject;
                EditorGUIUtility.PingObject(worldManager.gameObject);
            }
        }
        
        [MenuItem("PixelWorld/Verify Graphics Presets Setup")]
        static void VerifyGraphicsSetup()
        {
            Debug.Log("🔍 Verifying Graphics Presets setup...");
            
            var controller = Object.FindFirstObjectByType<RenderingPresetController>();
            if (controller == null)
            {
                Debug.LogError("❌ RenderingPresetController not found in scene!");
                EditorUtility.DisplayDialog("Verification Failed", 
                    "RenderingPresetController not found!\n\nRun 'PixelWorld → Setup Graphics Presets' first.", 
                    "OK");
                return;
            }
            
            Debug.Log($"✓ Found RenderingPresetController on: {controller.gameObject.name}");
            
            // Check if worldRenderer is assigned using SerializedObject
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty worldRendererProp = so.FindProperty("worldRenderer");
            SerializedProperty showNotificationProp = so.FindProperty("showPresetNotification");
            SerializedProperty durationProp = so.FindProperty("notificationDuration");
            
            bool allGood = true;
            
            if (worldRendererProp != null && worldRendererProp.objectReferenceValue != null)
            {
                Debug.Log($"✅ World Renderer assigned: {worldRendererProp.objectReferenceValue.name}");
            }
            else
            {
                Debug.LogError("❌ World Renderer NOT assigned!");
                allGood = false;
            }
            
            if (showNotificationProp != null && showNotificationProp.boolValue)
            {
                Debug.Log("✅ Show Preset Notification: Enabled");
            }
            else
            {
                Debug.LogWarning("⚠️ Show Preset Notification: Disabled");
                allGood = false;
            }
            
            if (durationProp != null)
            {
                Debug.Log($"✅ Notification Duration: {durationProp.floatValue} seconds");
                if (durationProp.floatValue != 3f)
                {
                    Debug.LogWarning($"⚠️ Duration is not 3 seconds (currently: {durationProp.floatValue})");
                }
            }
            
            if (allGood)
            {
                Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.Log("✅ Graphics Presets setup verified!");
                Debug.Log("🎮 Ready to test! Press Play and try F1-F6");
                Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                
                EditorUtility.DisplayDialog("Verification Passed!", 
                    "Graphics Presets are properly configured!\n\n" +
                    "✓ RenderingPresetController present\n" +
                    "✓ World Renderer assigned\n" +
                    "✓ Notifications enabled\n\n" +
                    "Press Play and try F1-F6!", 
                    "Great!");
            }
            else
            {
                Debug.LogError("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                Debug.LogError("❌ Setup incomplete! See errors above.");
                Debug.LogError("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                
                EditorUtility.DisplayDialog("Verification Failed", 
                    "Graphics Presets setup is incomplete.\n\n" +
                    "Please check Console for details and run:\n" +
                    "PixelWorld → Setup Graphics Presets", 
                    "OK");
                
                // Select the controller for manual configuration
                Selection.activeGameObject = controller.gameObject;
                EditorGUIUtility.PingObject(controller.gameObject);
            }
        }
    }
    #endif
}



