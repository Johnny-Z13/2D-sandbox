using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Reflection;

public class DebugLight
{
    public static void Execute()
    {
        GameObject globalLightGO = GameObject.Find("Global Light 2D");
        if (globalLightGO != null)
        {
            Light2D light = globalLightGO.GetComponent<Light2D>();
            if (light != null)
            {
                Debug.Log($"Light Type: {light.lightType}");
                Debug.Log($"Light Intensity: {light.intensity}");
                Debug.Log($"Light Color: {light.color}");
                
                // Access renderingLayerMask via reflection if needed, or just property if available in this version
                // In newer URP versions it is 'renderingLayerMask'
                // But let's try to print what we can.
                
                // Force it to be on all layers
                // uint allLayers = uint.MaxValue;
                // light.renderingLayerMask = allLayers; 
                // Note: API might differ slightly depending on URP version.
                
                // Check target sorting layers
                // This is often an array of IDs.
                
                Debug.Log("Attempting to set Light to affect all sorting layers.");
                // There isn't a direct simple API for "All Sorting Layers" without knowing the IDs, 
                // but usually new lights affect all.
                
                // Let's try to reset the light to defaults or ensure it covers the player's layer.
            }
        }

        GameObject player = GameObject.Find("Player-01");
        if (player != null)
        {
            Transform visuals = player.transform.Find("Visuals");
            if (visuals != null)
            {
                SpriteRenderer sr = visuals.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    Debug.Log($"Sprite Rendering Layer Mask: {sr.renderingLayerMask}");
                    Debug.Log($"Sprite Sorting Layer ID: {sr.sortingLayerID}");
                    Debug.Log($"Sprite Sorting Layer Name: {sr.sortingLayerName}");
                }
            }
        }
    }
}
