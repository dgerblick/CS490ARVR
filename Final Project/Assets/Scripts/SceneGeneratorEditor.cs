using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SceneGenerator))]
public class SceneGeneratorEditor : Editor {
    public override void OnInspectorGUI() {
        SceneGenerator myTarget = (SceneGenerator)target;

        if (GUILayout.Button("Re-Generate City")) {
            myTarget.Generate();
        }

        base.OnInspectorGUI();
    }
}