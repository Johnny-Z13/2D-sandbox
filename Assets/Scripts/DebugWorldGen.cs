using UnityEngine;
using PixelWorld;

public class DebugWorldGen
{
    public static void Execute()
    {
        var debugger = Object.FindFirstObjectByType<WorldGenerationDebugger>();
        if (debugger != null)
        {
            debugger.CheckWorldGeneration();
        }
        else
        {
            Debug.LogError("WorldGenerationDebugger not found in scene!");
            // Try to find manager directly
            var manager = Object.FindFirstObjectByType<PixelWorldManager>();
            if (manager != null)
            {
                Debug.Log($"Found Manager: {manager.Width}x{manager.Height}");
                var tex = manager.GetCurrentTexture();
                if (tex != null)
                {
                    Debug.Log($"Texture: {tex.width}x{tex.height}");
                }
                else
                {
                    Debug.LogError("Texture is null");
                }
            }
        }
    }
}
