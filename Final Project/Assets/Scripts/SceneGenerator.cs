using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SceneCellManager))]
public class SceneGenerator : MonoBehaviour {

    public int _cellsPerEdge = 5;
    public float _cellEdgeSize = 100.0f;
    public float _roadWidth = 10.0f;
    public int _buildingsPerBlock = 5;
    public float _minHeight = 10.0f;
    public float _maxHeight = 50.0f;
    public float _heightVariation = 10.0f;
    public float _parkChance = 0.2f;
    public Material _material;
    public int _cubemapSize = 1024;
    public float _renderHeight = 1.0f;

#if UNITY_EDITOR
    // Re-Generate the scene
    public void Generate() {
        if (EditorApplication.isPlaying)
            return;
        Order66(transform);

        // See if SceneCells Directory Exists
        if (Directory.Exists("Assets/Resources/SceneCells"))
            AssetDatabase.DeleteAsset("Assets/Resources/SceneCells");
        AssetDatabase.CreateFolder("Assets/Resources", "SceneCells");

        int numCells = 2 * _cellsPerEdge;
        float sceneWidth = numCells * _cellEdgeSize;

        SceneCellManager scm = GetComponent<SceneCellManager>();
        scm._idMap = new int[numCells, numCells];

        // Create Cells
        int id = 0;
        for (int i = 0; i < numCells; i++) {
            float x = sceneWidth * ((i + 0.5f) / numCells - 0.5f);

            for (int j = 0; j < numCells; j++) {
                float z = sceneWidth * ((j + 0.5f) / numCells - 0.5f);
                bool isborder = i == 0 || i == numCells - 1 || j == 0 || j == numCells - 1;

                SceneCell sc = GenerateCell(x, z, id, isborder);
                sc.Save();
                scm._idMap[i, j] = id;

                id++;
            }
        }
        scm.ReloadCells();
        GenerateCubemaps();
    }

    public void GenerateCubemaps() {
        string cubemapDir = "Assets/Resources/SceneCells/Cubemaps";
        // See if SceneCells/Cubemaps Directory Exists
        if (Directory.Exists(cubemapDir))
            AssetDatabase.DeleteAsset(cubemapDir);
        AssetDatabase.CreateFolder("Assets/Resources/SceneCells", "Cubemaps");

        // Render into Cubemap
        Camera camera = new GameObject().AddComponent<Camera>();
        Cubemap cubemap = new Cubemap(_cubemapSize, TextureFormat.RGBA32, false);
        camera.transform.position = new Vector3(0, _renderHeight, 0);
        camera.transform.rotation = Quaternion.identity;
        camera.RenderToCubemap(cubemap);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Write to Files
        Texture2D tex = new Texture2D(cubemap.width, cubemap.height, TextureFormat.RGB24, false);
        CubemapFace[] faces = new CubemapFace[] {
            CubemapFace.PositiveX, CubemapFace.NegativeX,
            CubemapFace.PositiveY, CubemapFace.NegativeY,
            CubemapFace.PositiveZ, CubemapFace.NegativeZ
        };
        foreach (CubemapFace face in faces) {
            // Write Face
            tex.SetPixels(cubemap.GetPixels(face));
            byte[] data = tex.EncodeToPNG();
            string filename = cubemapDir + "/Cubemap0_" + face.ToString() + ".png";
            File.WriteAllBytes(filename, data);

            // Set proper settings
            AssetDatabase.ImportAsset(filename);
            TextureImporter importer = AssetImporter.GetAtPath(filename) as TextureImporter;
            importer.isReadable = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        DestroyImmediate(tex);
        DestroyImmediate(camera.gameObject);
    }
#endif

    // RotS 1:23:37
    private static void Order66(Transform anakin) {
        GameObject[] younglings = new GameObject[anakin.childCount];
        for (int i = 0; i < younglings.Length; i++)
            younglings[i] = anakin.GetChild(i).gameObject;
        foreach (GameObject youngling in younglings)
            DestroyImmediate(youngling);
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
        plane.transform.localScale = Vector3.one * _cellEdgeSize / 10;

        if (border) {
            float oldRoadWidth = _roadWidth;
            _roadWidth = 0.0f;
            GenerateBlock(cell.transform);
            _roadWidth = oldRoadWidth;
        } else {
            if (Random.Range(0.0f, 1.0f) >= _parkChance)
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
        meshRenderer.material = _material;

        // Remove Geometry
        Order66(cell.transform);

        return sceneCell;
    }

    private float GetBuildingCoord(int i) {
        if (i < 0)
            i = _buildingsPerBlock + i;
        float buildingArea = _cellEdgeSize - _roadWidth;
        float buildingWidth = buildingArea / _buildingsPerBlock;
        float coordFraction = ((float)i / _buildingsPerBlock - 0.5f);
        return buildingArea * coordFraction + buildingWidth / 2;
    }

    private void GenerateBlock(Transform parent) {
        float buildingArea = _cellEdgeSize - _roadWidth;
        float buildingWidth = buildingArea / _buildingsPerBlock;
        float blockHeight = Random.Range(_minHeight, _maxHeight);
        if (_buildingsPerBlock == 1) {
            CreateBuilding(parent, 0, 0, buildingWidth, blockHeight);
            return;
        }

        for (int i = 0; i < _buildingsPerBlock; i++) {
            float z = GetBuildingCoord(i);
            float height0 = blockHeight + Random.Range(_heightVariation, -_heightVariation);
            float height1 = blockHeight + Random.Range(_heightVariation, -_heightVariation);
            CreateBuilding(parent, GetBuildingCoord(0), z, buildingWidth, height0);
            CreateBuilding(parent, GetBuildingCoord(-1), z, buildingWidth, height1);
        }

        for (int i = 1; i < _buildingsPerBlock - 1; i++) {
            float x = GetBuildingCoord(i);
            float height0 = blockHeight + Random.Range(_heightVariation, -_heightVariation);
            float height1 = blockHeight + Random.Range(_heightVariation, -_heightVariation);
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

        if (GUILayout.Button("Generate City")) {
            myTarget.Generate();
        }
        if (GUILayout.Button("Generate Cubemaps")) {
            myTarget.GenerateCubemaps();
        }

        base.OnInspectorGUI();
    }
}
#endif
