using UnityEngine;
using UnityEditor;
using PixelWorld;

[CustomEditor(typeof(PixelWorldManager))]
public class PixelWorldManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        PixelWorldManager manager = (PixelWorldManager)target;

        // Draw default inspector
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Randomization & Settings", EditorStyles.boldLabel);

        if (GUILayout.Button("🎲 Randomize Settings"))
        {
            Undo.RecordObject(manager, "Randomize World Settings");
            manager.RandomizeSettings();
            EditorUtility.SetDirty(manager);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("💾 Save Settings"))
        {
            manager.SaveSettings();
        }
        if (GUILayout.Button("📂 Load Settings"))
        {
            Undo.RecordObject(manager, "Load World Settings");
            manager.LoadSettings();
            EditorUtility.SetDirty(manager);
        }
        EditorGUILayout.EndHorizontal();
    }
}
