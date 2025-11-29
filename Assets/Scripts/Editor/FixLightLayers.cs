using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

public class FixLightLayers
{
    public static void Execute()
    {
        GameObject globalLightGO = GameObject.Find("Global Light 2D");
        if (globalLightGO != null)
        {
            Light2D light = globalLightGO.GetComponent<Light2D>();
            if (light != null)
            {
#if UNITY_EDITOR
                SerializedObject so = new SerializedObject(light);
                
                // Fix 1: Set Rendering Layer Mask
                SerializedProperty renderingLayerMaskProp = so.FindProperty("m_RenderingLayerMask");
                if (renderingLayerMaskProp != null)
                {
                    renderingLayerMaskProp.intValue = -1; 
                    Debug.Log("Set Global Light 2D m_RenderingLayerMask to Everything (-1).");
                }

                // Fix 2: Set Target Sorting Layers
                SerializedProperty sortingLayersProp = so.FindProperty("m_ApplyToSortingLayers");
                if (sortingLayersProp != null)
                {
                    // Use reflection to get sorting layer IDs since InternalEditorUtility.sortingLayerUniqueIDs is protected/internal in some versions
                    int[] allLayerIDs = GetSortingLayerIDsReflection();
                    
                    if (allLayerIDs != null && allLayerIDs.Length > 0)
                    {
                        sortingLayersProp.ClearArray();
                        sortingLayersProp.arraySize = allLayerIDs.Length;
                        for(int i=0; i<allLayerIDs.Length; i++)
                        {
                            sortingLayersProp.GetArrayElementAtIndex(i).intValue = allLayerIDs[i];
                        }
                        Debug.Log($"Set Global Light 2D to affect {allLayerIDs.Length} sorting layers.");
                    }
                }
                
                so.ApplyModifiedProperties();
#endif
            }
        }
    }

    private static int[] GetSortingLayerIDsReflection()
    {
#if UNITY_EDITOR
        System.Type internalEditorUtilityType = typeof(InternalEditorUtility);
        PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        if (sortingLayerUniqueIDsProperty != null)
        {
            return (int[])sortingLayerUniqueIDsProperty.GetValue(null, null);
        }
#endif
        return new int[] { 0 }; // Fallback to Default layer
    }
}
