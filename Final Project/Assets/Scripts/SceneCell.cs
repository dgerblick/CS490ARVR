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

        _verticies.Clear();
        _uv.Clear();
        _triangles.Clear();
        _serialized = false;
    }

    public void ApplyMorphMesh(Mesh mesh) {
        mesh.SetVertices(_morphVerts);
        mesh.SetUVs(0, _morphSW);
        mesh.SetUVs(1, _morphNW);
        mesh.SetUVs(2, _morphSE);
        mesh.SetUVs(3, _morphNE);
        mesh.SetTriangles(_morphTris, 0);
    }

#if UNITY_EDITOR
    public void Save() {
        // Serialize Mesh
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;
        if (!_serialized) {
            _verticies = new List<Vector3>();
            _uv = new List<Vector2>();
            _triangles = new List<int>();

            mesh.GetVertices(_verticies);
            mesh.GetUVs(0, _uv);
            mesh.GetTriangles(_triangles, 0);

            _serialized = true;
        }

        // Clear Mesh
        mesh.Clear();

        // Write to File
        string path = string.Format("{0}/{1}_{2}.prefab", CELL_PATH, _i, _j);
        bool status;
        PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, path, InteractionMode.AutomatedAction, out status);
        if (!status) {
            Debug.LogFormat("Could not save Cell {0}_{1}", _i, _j);
        }
    }

    public void GenerateMorph(int divisions, float edgeSize) {
        _morphVerts = new List<Vector3>();
        _morphSW = new List<Vector3>();
        _morphNW = new List<Vector3>();
        _morphSE = new List<Vector3>();
        _morphNE = new List<Vector3>();
        _morphTris = new List<int>();

        // Generate icosahedron
        _morphVerts.Add(Vector3.zero);
        _morphVerts.Add(new Vector3(0.000000f, -1.000000f, 0.000000f).normalized);
        _morphVerts.Add(new Vector3(0.723600f, -0.447215f, 0.525720f).normalized);
        _morphVerts.Add(new Vector3(-0.276385f, -0.447215f, 0.850640f).normalized);
        _morphVerts.Add(new Vector3(-0.894425f, -0.447215f, 0.000000f).normalized);
        _morphVerts.Add(new Vector3(-0.276385f, -0.447215f, -0.850640f).normalized);
        _morphVerts.Add(new Vector3(0.723600f, -0.447215f, -0.525720f).normalized);
        _morphVerts.Add(new Vector3(0.276385f, 0.447215f, 0.850640f).normalized);
        _morphVerts.Add(new Vector3(-0.723600f, 0.447215f, 0.525720f).normalized);
        _morphVerts.Add(new Vector3(-0.723600f, 0.447215f, -0.525720f).normalized);
        _morphVerts.Add(new Vector3(0.276385f, 0.447215f, -0.850640f).normalized);
        _morphVerts.Add(new Vector3(0.894425f, 0.447215f, 0.000000f).normalized);
        _morphVerts.Add(new Vector3(0.000000f, 1.000000f, 0.000000f).normalized);

        Subdivide(1, 2, 3, divisions);
        Subdivide(2, 1, 6, divisions);
        Subdivide(1, 3, 4, divisions);
        Subdivide(1, 4, 5, divisions);
        Subdivide(1, 5, 6, divisions);
        Subdivide(2, 6, 11, divisions);
        Subdivide(3, 2, 7, divisions);
        Subdivide(4, 3, 8, divisions);
        Subdivide(5, 4, 9, divisions);
        Subdivide(6, 5, 10, divisions);
        Subdivide(2, 11, 7, divisions);
        Subdivide(3, 7, 8, divisions);
        Subdivide(4, 8, 9, divisions);
        Subdivide(5, 9, 10, divisions);
        Subdivide(6, 10, 11, divisions);
        Subdivide(7, 11, 12, divisions);
        Subdivide(8, 7, 12, divisions);
        Subdivide(9, 8, 12, divisions);
        Subdivide(10, 9, 12, divisions);
        Subdivide(11, 10, 12, divisions);


        // Calculate offset
        Vector3[] corners = new Vector3[4] {
            transform.TransformPoint(edgeSize * new Vector3(-1, 0, -1)),
            transform.TransformPoint(edgeSize * new Vector3(-1, 0,  1)),
            transform.TransformPoint(edgeSize * new Vector3( 1, 0, -1)),
            transform.TransformPoint(edgeSize * new Vector3( 1, 0,  1)),
        };
        foreach (Vector3 dir in _morphVerts) {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, dir, out hit, Mathf.Infinity)) {
                _morphSW.Add(Vector3.Normalize(hit.point - corners[0]));
                _morphNW.Add(Vector3.Normalize(hit.point - corners[1]));
                _morphSE.Add(Vector3.Normalize(hit.point - corners[2]));
                _morphNE.Add(Vector3.Normalize(hit.point - corners[3]));
            } else {
                _morphSW.Add(dir);
                _morphNW.Add(dir);
                _morphSE.Add(dir);
                _morphNE.Add(dir);
            }
        }
    }

    private void Subdivide(int v1, int v2, int v3, int n) {
        List<int> list = new List<int>();
        if (n == 0) {
            // Base case
            _morphTris.Add(v1);
            _morphTris.Add(v2);
            _morphTris.Add(v3);
            return;
        }
        int v12 = _morphVerts.Count;
        _morphVerts.Add(Vector3.Normalize((_morphVerts[v1] + _morphVerts[v2]) / 2));
        int v23 = _morphVerts.Count;
        _morphVerts.Add(Vector3.Normalize((_morphVerts[v2] + _morphVerts[v3]) / 2));
        int v31 = _morphVerts.Count;
        _morphVerts.Add(Vector3.Normalize((_morphVerts[v3] + _morphVerts[v1]) / 2));
        Subdivide(v1, v12, v31, n - 1);
        Subdivide(v12, v2, v23, n - 1);
        Subdivide(v31, v23, v3, n - 1);
        Subdivide(v12, v23, v31, n - 1);
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
