using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SceneCellManager : MonoBehaviour {

    public SceneCell[,] _cells;
    public int _loadDist = 2;
    public int _cubemapSize = 1024;
    private float _cellEdgeSize;

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
        foreach (var go in cells) {
            Vector2Int ij = UnparseName(go.name);
            PrefabUtility.InstantiatePrefab(go, transform);
            SceneCell sc = go.GetComponent<SceneCell>();
            sc.LoadMesh();
            _cells[ij.x, ij.y] = sc;
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
        int x = Mathf.RoundToInt(localPos.x / _cellEdgeSize) + _cells.GetLength(0) / 2 - 1;
        x = Mathf.Clamp(x, 0, _cells.GetLength(0) - 1);
        int z = Mathf.RoundToInt(localPos.z / _cellEdgeSize) + _cells.GetLength(1) / 2 - 1;
        z = Mathf.Clamp(z, 0, _cells.GetLength(1) - 1);
        return new Vector2Int(x, z);
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
