using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
public class SceneCell : MonoBehaviour {

    public int _i;
    public int _j;
    public List<Vector3> _verticies;
    public List<Vector2> _uv;
    public List<int> _triangles;
    public List<Vector3> _morphVerts;
    public List<Vector3> _morphSW;
    public List<Vector3> _morphNW;
    public List<Vector3> _morphSE;
    public List<Vector3> _morphNE;
    public List<int> _morphTris;
    public bool _serialized = false;
    public Mesh _morph;

    private const string CELL_PATH = "Assets/Resources/SceneCells";

    private void Awake() {
        if (_serialized)
            LoadMesh();
    }

    public void LoadMesh() {
        if (!_serialized)
            return;
        MeshFilter mf = GetComponent<MeshFilter>();
        mf.sharedMesh = new Mesh();
        mf.sharedMesh.SetVertices(_verticies);
        mf.sharedMesh.SetUVs(0, _uv);
        mf.sharedMesh.SetTriangles(_triangles, 0, true);
        mf.sharedMesh.RecalculateNormals();
        mf.sharedMesh.RecalculateBounds();
        GetComponent<MeshCollider>().sharedMesh = mf.sharedMesh;

        _morph = new Mesh();
        _morph.SetVertices(_morphVerts);
        _morph.SetUVs(0, _morphSW);
        _morph.SetUVs(1, _morphNW);
        _morph.SetUVs(2, _morphSE);
        _morph.SetUVs(3, _morphNE);
        _morph.SetTriangles(_morphTris, 0);

        _verticies.Clear();
        _uv.Clear();
        _triangles.Clear();

        _morphVerts.Clear();
        _morphSW.Clear();
        _morphNW.Clear();
        _morphSE.Clear();
        _morphNE.Clear();
        _morphTris.Clear();
        _serialized = false;
    }

#if UNITY_EDITOR
    public void Save() {
        // Serialize Mesh
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        if (!_serialized) {
            _verticies = new List<Vector3>();
            _uv = new List<Vector2>();
            _triangles = new List<int>();
            _morphVerts = new List<Vector3>();
            _morphSW = new List<Vector3>();
            _morphNW = new List<Vector3>();
            _morphSE = new List<Vector3>();
            _morphNE = new List<Vector3>();
            _morphTris = new List<int>();

            mesh.GetVertices(_verticies);
            mesh.GetUVs(0, _uv);
            mesh.GetTriangles(_triangles, 0);

            _morph.GetVertices(_morphVerts);
            _morph.GetUVs(0, _morphSW);
            _morph.GetUVs(1, _morphNW);
            _morph.GetUVs(2, _morphSE);
            _morph.GetUVs(3, _morphNE);
            _morph.GetTriangles(_morphTris, 0);

            _serialized = true;
        }

        // Clear Mesh
        mesh.Clear();
        _morph.Clear();

        // Write to File
        string path = string.Format("{0}/{1}_{2}.prefab", CELL_PATH, _i, _j);
        bool status;
        PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, path, InteractionMode.AutomatedAction, out status);
        if (!status) {
            Debug.LogFormat("Could not save Cell {0}_{1}", _i, _j);
        }
    }

    public void GenerateMorph(List<Vector4> points, float edgeSize) {
        _morph = new Mesh();
        var localPoints = points.Select(v => {
            if (v.w == 0)
                return Vector3.Normalize(new Vector3(v.x, v.y, v.z) - transform.position);
            else
                return Vector3.Normalize(new Vector3(v.x, v.y, v.z));
        });
        _morph.SetVertices(localPoints.ToArray());

        Vector3[] corners = new Vector3[4] {
            transform.TransformPoint(edgeSize * new Vector3(-1, 0, -1)),
            transform.TransformPoint(edgeSize * new Vector3(-1, 0,  1)),
            transform.TransformPoint(edgeSize * new Vector3( 1, 0, -1)),
            transform.TransformPoint(edgeSize * new Vector3( 1, 0,  1)),
        };
        for (int i = 0; i < 4; i++) {
            var cornerPoints = points.Select(v => {
                if (v.w == 0)
                    return Vector3.Normalize(new Vector3(v.x, v.y, v.z) - corners[i]);
                else
                    return Vector3.Normalize(new Vector3(v.x, v.y, v.z));
            });
            _morph.SetUVs(i, cornerPoints.ToList());
        }
        // Debug.LogFormat("{0}x{1}: {2}", _i, _j);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneCell))]
public class SceneCellEditor : Editor {
    public override void OnInspectorGUI() {
        SceneCell myTarget = (SceneCell)target;

        if (GUILayout.Button("Save Mesh")) {
            myTarget.Save();
        }
        if (GUILayout.Button("Load Mesh")) {
            myTarget.LoadMesh();
        }

        base.OnInspectorGUI();
    }
}
#endif
