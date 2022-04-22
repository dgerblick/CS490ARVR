using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SceneCellManager))]
public class SceneGenerator : MonoBehaviour {

    public int cellsPerEdge;
    public float cellEdgeSize;
    public float roadWidth;
    public int buildingsPerBlock;
    public float minHeight;
    public float maxHeight;
    public float heightVariation;
    public float parkChance;
    public Material material;

    // Re-Generate the scene
    public void Generate() {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
            return;
        Order66(transform);

        // See if SceneCells Directory Exists
        if (Directory.Exists("Assets/Resources/SceneCells"))
            AssetDatabase.DeleteAsset("Assets/Resources/SceneCells");
        AssetDatabase.CreateFolder("Assets/Resources", "SceneCells");

        int numCells = 2 * cellsPerEdge;
        float sceneWidth = numCells * cellEdgeSize;

        SceneCellManager scm = GetComponent<SceneCellManager>();
        scm._idMap = new int[numCells, numCells];

        int id = 0;
        for (int i = 0; i < numCells; i++) {
            float x = sceneWidth * ((i + 0.5f) / numCells - 0.5f);

            for (int j = 0; j < numCells; j++) {
                float z = sceneWidth * ((j + 0.5f) / numCells - 0.5f);
                bool isborder = i == 0 || i == numCells - 1 || j == 0 || j == numCells - 1;

                SceneCell sc = GenerateCell(x, z, id, isborder);
                sc.Save();
                // GameObject.DestroyImmediate(sc.gameObject);
                scm._idMap[i, j] = id;

                id++;
            }
        }
        scm.ReloadCells();
#endif
    }

    // RotS 1:23:37
    private static void Order66(Transform anakin) {
        GameObject[] younglings = new GameObject[anakin.childCount];
        for (int i = 0; i < younglings.Length; i++)
            younglings[i] = anakin.GetChild(i).gameObject;
        foreach (GameObject youngling in younglings)
            GameObject.DestroyImmediate(youngling);
    }

    private SceneCell GenerateCell(float x, float z, int id, bool border = false) {
        GameObject cell = new GameObject("Cell" + id);
        cell.transform.SetParent(transform);
        cell.transform.localPosition = new Vector3(x, 0, z);
        cell.transform.localRotation = Quaternion.identity;
        cell.transform.localScale = Vector3.one;
        MeshFilter meshFilter = cell.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cell.AddComponent<MeshRenderer>();
        SceneCell sceneCell = cell.AddComponent<SceneCell>();
        sceneCell._id = id;

        // Generate Plane
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.SetParent(cell.transform);
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localRotation = Quaternion.identity;
        plane.transform.localScale = Vector3.one * cellEdgeSize / 10;

        if (border) {
            float oldRoadWidth = roadWidth;
            roadWidth = 0.0f;
            GenerateBlock(cell.transform);
            roadWidth = oldRoadWidth;
        } else {
            if (Random.Range(0.0f, 1.0f) >= parkChance)
                GenerateBlock(cell.transform);
        }

        // Combine Meshes
        MeshFilter[] meshFilters = cell.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        for (int i = 0; i < meshFilters.Length; i++) {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = cell.transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
        }
        meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh.CombineMeshes(combine);
        meshRenderer.material = material;

        // Remove Geometry
        Order66(cell.transform);

        return sceneCell;
    }

    private float GetBuildingCoord(int i) {
        if (i < 0)
            i = buildingsPerBlock + i;
        float buildingArea = cellEdgeSize - roadWidth;
        float buildingWidth = buildingArea / buildingsPerBlock;
        float coordFraction = ((float)i / buildingsPerBlock - 0.5f);
        return buildingArea * coordFraction + buildingWidth / 2;
    }

    private void GenerateBlock(Transform parent) {
        float buildingArea = cellEdgeSize - roadWidth;
        float buildingWidth = buildingArea / buildingsPerBlock;
        float blockHeight = Random.Range(minHeight, maxHeight);
        if (buildingsPerBlock == 1) {
            CreateBuilding(parent, 0, 0, buildingWidth, blockHeight);
            return;
        }

        for (int i = 0; i < buildingsPerBlock; i++) {
            float z = GetBuildingCoord(i);
            float height0 = blockHeight + Random.Range(heightVariation, -heightVariation);
            float height1 = blockHeight + Random.Range(heightVariation, -heightVariation);
            CreateBuilding(parent, GetBuildingCoord(0), z, buildingWidth, height0);
            CreateBuilding(parent, GetBuildingCoord(-1), z, buildingWidth, height1);
        }

        for (int i = 1; i < buildingsPerBlock - 1; i++) {
            float x = GetBuildingCoord(i);
            float height0 = blockHeight + Random.Range(heightVariation, -heightVariation);
            float height1 = blockHeight + Random.Range(heightVariation, -heightVariation);
            CreateBuilding(parent, x, GetBuildingCoord(0), buildingWidth, height0);
            CreateBuilding(parent, x, GetBuildingCoord(-1), buildingWidth, height1);
        }
    }

    private void CreateBuilding(Transform parent, float x, float z, float buildingWidth, float height) {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(parent);
        cube.transform.localPosition = new Vector3(x, height / 2, z);
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = new Vector3(buildingWidth, height, buildingWidth);
    }
}

#if UNITY_EDITOR
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
#endif
