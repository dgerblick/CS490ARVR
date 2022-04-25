using System.Collections;
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
    }

    private void Start() {
        ReloadCells();
    }

    // RotS 1:23:37
    public static void Order66(Transform anakin) {
        GameObject[] younglings = new GameObject[anakin.childCount];
        for (int i = 0; i < younglings.Length; i++)
            younglings[i] = anakin.GetChild(i).gameObject;
        foreach (GameObject youngling in younglings)
            GameObject.DestroyImmediate(youngling);
    }

    public Vector2Int GetCubemapIdx(Vector3 pos) {
        Vector3 localPos = transform.worldToLocalMatrix * pos;
        int x = Mathf.RoundToInt(localPos.x / _cellEdgeSize) + _cells.GetLength(0) / 2;
        x = Mathf.Clamp(x, 0, _cells.GetLength(0));
        int z = Mathf.RoundToInt(localPos.z / _cellEdgeSize) + _cells.GetLength(1) / 2;
        z = Mathf.Clamp(z, 0, _cells.GetLength(1));
        return new Vector2Int(x, z);
    }

    private static Vector2Int UnparseName(string name) {
        string[] strs = name.Split('_');
        return new Vector2Int(int.Parse(strs[0]), int.Parse(strs[1]));
    }

    public void ChangeCubemap(Vector2Int oldPos, Vector2Int newPos) {
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

    public void HideForCubemapRender(Vector2Int pos) {
        for (int i = 0; i < _cameraCount.GetLength(0); i++)
            for (int j = 0; j < _cameraCount.GetLength(1); j++)
                _cellRenderers[i, j].enabled = true;

        if (pos != -Vector2Int.one) {
            Vector2Int start = pos - Vector2Int.one * _loadDist;
            Vector2Int end = pos + Vector2Int.one * _loadDist;
            for (int i = start.x; i < end.x; i++)
                for (int j = start.y; j < end.y; j++)
                    if (0 <= i && i < _cameraCount.GetLength(0) && 0 <= j && j < _cameraCount.GetLength(1))
                        _cellRenderers[i, j].enabled = false;
        }
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
