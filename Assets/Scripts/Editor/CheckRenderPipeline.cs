using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CheckRenderPipeline
{
    public static void Execute()
    {
        Debug.Log($"Current Render Pipeline: {GraphicsSettings.currentRenderPipeline?.name ?? "Built-in"}");
        
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            Debug.Log($"URP Asset found: {urpAsset.name}");
            // Removed supports2D check as it's not available in this version
        }
        else
        {
            Debug.LogError("Current Render Pipeline is NOT UniversalRenderPipelineAsset!");
        }

        // Check Player Material Shader
        GameObject player = GameObject.Find("Player-01");
        if (player != null)
        {
            Transform visuals = player.transform.Find("Visuals");
            if (visuals != null)
            {
                SpriteRenderer sr = visuals.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Debug.Log($"Player Material: {sr.sharedMaterial.name}");
                    Debug.Log($"Player Shader: {sr.sharedMaterial.shader.name}");
                }
            }
        }
    }
}
