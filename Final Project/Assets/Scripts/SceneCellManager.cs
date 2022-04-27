using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SceneCellManager : MonoBehaviour {

    public int _loadDist = 2;
    public int _cubemapSize = 1024;
    private float _cellEdgeSize;
    private SceneCell[,] _cells;
    private MeshRenderer[,] _cellRenderers;
    private int[,] _cameraCount;
    private Texture2D _topFace;
    private Texture2D _bottomFace;
    private Texture2D[,,] _cubemapFaces;

    private const string CUBEMAP_DIR = "SceneCells/Cubemaps";

    public void ReloadCells() {
        Order66(transform);

        var cells = Resources.LoadAll("SceneCells", typeof(GameObject)).Cast<GameObject>();
        int maxI = 0;
        int maxJ = 0;
        foreach (var go in cells) {
            Vector2Int ij = UnparseName(go.name);
            maxI = Mathf.Max(ij.x + 1, maxI);
            maxJ = Mathf.Max(ij.y + 1, maxJ);
        }

        _cells = new SceneCell[maxI, maxJ];
        _cellRenderers = new MeshRenderer[maxI, maxJ];
        _cameraCount = new int[maxI, maxJ];
        _cubemapFaces = new Texture2D[maxI + 1, maxJ + 1, 4];

        foreach (var prefab in cells) {
            Vector2Int ij = UnparseName(prefab.name);
            GameObject go = PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
            SceneCell sc = go.GetComponent<SceneCell>();
            sc.LoadMesh();
            _cells[ij.x, ij.y] = sc;
            _cellRenderers[ij.x, ij.y] = sc.GetComponent<MeshRenderer>();
        }
        _cellEdgeSize = Vector3.Distance(_cells[0, 0].transform.position, _cells[0, 1].transform.position);

        Debug.LogFormat("Loaded all cells, {0}x{1}, Edge Size={2}", _cells.GetLength(0), _cells.GetLength(1), _cellEdgeSize);

        if (Application.isPlaying) {
            CubemapFace[] faces = new CubemapFace[] { CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveZ, CubemapFace.NegativeZ };
            for (int i = 0; i < _cubemapFaces.GetLength(0); i++) {
                for (int j = 0; j < _cubemapFaces.GetLength(1); j++) {
                    for (int k = 0; k < faces.Length; k++) {
                        string filename = string.Format("{0}/{1}x{2}_{3}", CUBEMAP_DIR, i, j, faces[k]);
                        _cubemapFaces[i, j, k] = Resources.Load<Texture2D>(filename);
                    }
                }
            }
        }
    }

    public void UpdateCellCount() {
        SceneCell[] cells = GetComponentsInChildren<SceneCell>();
        int maxI = 0;
        int maxJ = 0;
        foreach (SceneCell sc in cells) {
            Vector2Int ij = UnparseName(sc.gameObject.name);
            maxI = Mathf.Max(ij.x + 1, maxI);
            maxJ = Mathf.Max(ij.y + 1, maxJ);
        }

        _cells = new SceneCell[maxI, maxJ];
        _cellRenderers = new MeshRenderer[maxI, maxJ];
        _cameraCount = new int[maxI, maxJ];
        _cubemapFaces = new Texture2D[maxI + 1, maxJ + 1, 4];
        
        foreach (SceneCell sc in cells) {
            Vector2Int ij = UnparseName(sc.gameObject.name);
            _cells[ij.x, ij.y] = sc;
            _cellRenderers[ij.x, ij.y] = sc.GetComponent<MeshRenderer>();
        }
        _cellEdgeSize = Vector3.Distance(_cells[0, 0].transform.position, _cells[0, 1].transform.position);
    }

    // RotS 1:23:37
    public static void Order66(Transform anakin) {
        GameObject[] younglings = new GameObject[anakin.childCount];
        for (int i = 0; i < younglings.Length; i++)
            younglings[i] = anakin.GetChild(i).gameObject;
        foreach (GameObject youngling in younglings)
            GameObject.DestroyImmediate(youngling);
    }

    public Vector2Int GetNearestCubemap(Vector3 pos) {
        Vector3 localPos = transform.worldToLocalMatrix * pos;
        int x = Mathf.RoundToInt(localPos.x / _cellEdgeSize) + _cells.GetLength(0) / 2;
        x = Mathf.Clamp(x, 0, _cells.GetLength(0));
        int z = Mathf.RoundToInt(localPos.z / _cellEdgeSize) + _cells.GetLength(1) / 2;
        z = Mathf.Clamp(z, 0, _cells.GetLength(1));
        return new Vector2Int(x, z);
    }

    public Tuple<Vector2Int, Vector2> GetCellPos(Vector3 pos) {
        Vector3 localPos = transform.worldToLocalMatrix * pos;
        float xPos = localPos.x / _cellEdgeSize + (_cells.GetLength(0) - 1) * 0.5f + 0.5f;
        int x = Mathf.Clamp((int)xPos, 0, _cells.GetLength(0) - 1);
        xPos = xPos - x;
        float zPos = localPos.z / _cellEdgeSize + (_cells.GetLength(1) - 1) * 0.5f + 0.5f;
        int z = Mathf.Clamp((int)zPos, 0, _cells.GetLength(1) - 1);
        zPos = zPos - z;

        Vector2Int vi = new Vector2Int(x, z);
        Vector2 vf = new Vector2(xPos, zPos);
        return new Tuple<Vector2Int, Vector2>(vi, vf);
    }

    public void ChangeCubemap(Vector2Int oldPos, Vector2Int newPos) {
        // Calculate visible cells
        Vector2Int start = newPos - Vector2Int.one * _loadDist;
        Vector2Int end = newPos + Vector2Int.one * _loadDist;
        for (int i = start.x; i < end.x; i++)
            for (int j = start.y; j < end.y; j++)
                if (0 <= i && i < _cameraCount.GetLength(0) && 0 <= j && j < _cameraCount.GetLength(1))
                    _cameraCount[i, j]++;

        if (oldPos != -Vector2Int.one) {
            start = oldPos - Vector2Int.one * _loadDist;
            end = oldPos + Vector2Int.one * _loadDist;
            for (int i = start.x; i < end.x; i++)
                for (int j = start.y; j < end.y; j++)
                    if (0 <= i && i < _cameraCount.GetLength(0) && 0 <= j && j < _cameraCount.GetLength(1))
                        _cameraCount[i, j]--;
        }

        for (int i = 0; i < _cameraCount.GetLength(0); i++)
            for (int j = 0; j < _cameraCount.GetLength(1); j++)
                _cellRenderers[i, j].enabled = _cameraCount[i, j] > 0;
    }

    public void ChangeCell(Vector2Int pos, Cubemap[] cubemaps) {
        CubemapFace[] faces = new CubemapFace[] { CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveZ, CubemapFace.NegativeZ };
        Vector2Int[] offsets = new Vector2Int[] {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
        };
        for (int i = 0; i < 4; i++) {
            Vector2Int target = pos + offsets[i];
            for (int j = 0; j < faces.Length; j++)
                cubemaps[i].SetPixels(_cubemapFaces[target.x, target.y, j].GetPixels(), faces[j]);
            cubemaps[i].Apply();
        }
    }

    public void LoadCubemapTopBottom(Cubemap cubemap) {
        if (_topFace == null) {
            string topFile = string.Format("{0}/{1}", CUBEMAP_DIR, CubemapFace.PositiveY.ToString());
            _topFace = Resources.Load<Texture2D>(topFile);

        }
        if (_bottomFace == null) {
            string bottomFile = string.Format("{0}/{1}", CUBEMAP_DIR, CubemapFace.NegativeY.ToString());
            _bottomFace = Resources.Load<Texture2D>(bottomFile);
        }
        cubemap.SetPixels(_topFace.GetPixels(), CubemapFace.PositiveY);
        cubemap.SetPixels(_bottomFace.GetPixels(), CubemapFace.NegativeY);
        cubemap.Apply();
    }

    public void HideForCubemapRender(Vector2Int pos) {
        for (int i = 0; i < _cells.GetLength(0); i++)
            for (int j = 0; j < _cells.GetLength(1); j++)
                _cellRenderers[i, j].enabled = true;

        if (pos != -Vector2Int.one) {
            Vector2Int start = pos - Vector2Int.one * _loadDist;
            Vector2Int end = pos + Vector2Int.one * _loadDist;
            for (int i = start.x; i < end.x; i++)
                for (int j = start.y; j < end.y; j++)
                    if (0 <= i && i < _cells.GetLength(0) && 0 <= j && j < _cells.GetLength(1))
                        _cellRenderers[i, j].enabled = false;
        }
    }

    public void GenerateMorph(Vector2Int pos, int numVerts) {
        HideForCubemapRender(pos);

        List<Vector4> verts = new List<Vector4>();

        int hits = 0;
        for (int i = 0; i < numVerts; i++) {
            Vector3 dir = UnityEngine.Random.onUnitSphere;
            RaycastHit hit;
            if (Physics.Raycast(_cells[pos.x, pos.y].transform.position, dir, out hit)) {
                hits++;
                verts.Add(new Vector4(hit.point.x, hit.point.y, hit.point.z, 1));
            } else {
                verts.Add(new Vector4(dir.x, dir.y, dir.z, 0));
            }
        }
        HideForCubemapRender(-Vector2Int.one);

        _cells[pos.x, pos.y].GenerateMorph(verts, _cellEdgeSize);

        Debug.LogFormat("{0}x{1}: {2}/{3} hits", pos.x, pos.y, hits, numVerts);
    }

    private void Start() {
        ReloadCells();
    }

    private static Vector2Int UnparseName(string name) {
        string[] strs = name.Split('_');
        return new Vector2Int(int.Parse(strs[0]), int.Parse(strs[1]));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneCellManager))]
public class SceneCellManagerEditor : Editor {
    public override void OnInspectorGUI() {
        SceneCellManager myTarget = (SceneCellManager)target;

        if (GUILayout.Button("Unload Cells")) {
            SceneCellManager.Order66(myTarget.transform);
        }
        if (GUILayout.Button("Load/Reload Cells")) {
            myTarget.ReloadCells();
        }

        base.OnInspectorGUI();
    }
}
#endif
