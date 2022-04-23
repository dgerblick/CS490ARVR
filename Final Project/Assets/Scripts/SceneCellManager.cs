using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class SceneCellManager : MonoBehaviour {

    public int[,] _idMap;
    public SceneCell[] _cells;

    public void ReloadCells() {
        Order66(transform);

        var cells = Resources.LoadAll("SceneCells", typeof(GameObject)).Cast<GameObject>();
        _cells = new SceneCell[cells.Count()];
        int i = 0;
        foreach (var go in cells) {
            PrefabUtility.InstantiatePrefab(go, transform);
            SceneCell sc = go.GetComponent<SceneCell>();
            sc.LoadMesh();
            _cells[sc._id] = sc;
            i++;
        }

        Debug.Log("Loaded all cells");
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
