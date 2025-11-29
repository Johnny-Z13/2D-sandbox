using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FixLighting
{
    public static void Execute()
    {
        // Fix Global Light
        GameObject globalLightGO = GameObject.Find("Global Light 2D");
        if (globalLightGO != null)
        {
            Light2D light2D = globalLightGO.GetComponent<Light2D>();
            if (light2D != null)
            {
                light2D.lightType = Light2D.LightType.Global;
                Debug.Log("Set Global Light 2D to Global type.");
            }
            else
            {
                Debug.LogError("Light2D component not found on Global Light 2D object.");
            }
        }
        else
        {
            Debug.LogError("Global Light 2D object not found.");
        }

        // Check Player Material
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
                    // Ensure it's using a Lit material if we want it to react to light, 
                    // or Unlit if we want it to ignore light (but user said it's black, so likely Lit with no light).
                    // If it's Lit, the Global Light fix should solve it.
                }
            }
        }
    }
}
