using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class SceneCell : MonoBehaviour {

    public int _id;
    public Vector3[] _verticies;
    public Vector2[] _uv;
    public int[] _triangles;
    public bool _serialized = false;

    private void Awake() {
        if (_serialized)
            LoadMesh();
    }

    public void LoadMesh() {
        if (!_serialized)
            return;
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = new Mesh();
        mf.sharedMesh.vertices = _verticies;
        mf.sharedMesh.uv = _uv;
        mf.sharedMesh.triangles = _triangles;
        mf.sharedMesh.RecalculateNormals();
        mf.sharedMesh.RecalculateBounds();
    }

    public void Save() {
#if UNITY_EDITOR
        // Serialize Mesh
        if (!_serialized) {
            Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
            _verticies = mesh.vertices;
            _uv = mesh.uv;
            _triangles = mesh.triangles;
            _serialized = true;
        }

        // Write to File
        string path = "Assets/Resources/SceneCells/Cell" + _id + ".prefab";
        bool status;
        PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, path, InteractionMode.AutomatedAction, out status);
        if (!status) {
            Debug.Log("Could not save Cell " + _id);
        }
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneCell))]
public class SceneCellEditor : Editor {
    public override void OnInspectorGUI() {
        SceneCell myTarget = (SceneCell)target;

        if (GUILayout.Button("Load Mesh")) {
            myTarget.LoadMesh();
        }

        base.OnInspectorGUI();
    }
}
#endif
