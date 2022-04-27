using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public float _renderHeight = 1.0f;

    private Cubemap _cubemapBuffer;
    private Texture2D _cubemapFaceTex;

    private const string RESOURCES_DIR = "Assets/Resources";
    private const string CELL_DIR_NAME = "SceneCells";
    private const string CUBEMAP_DIR_NAME = "Cubemaps";
    private const string CELL_DIR = RESOURCES_DIR + "/" + CELL_DIR_NAME;
    private const string CUBEMAP_DIR = CELL_DIR + "/" + CUBEMAP_DIR_NAME;


#if UNITY_EDITOR
    // Re-Generate the scene
    public void Generate() {
        if (EditorApplication.isPlaying)
            return;
        Order66(transform);

        // See if SceneCells Directory Exists
        if (Directory.Exists(CELL_DIR))
            AssetDatabase.DeleteAsset(CELL_DIR);
        AssetDatabase.CreateFolder(RESOURCES_DIR, CELL_DIR_NAME);

        int numCells = 2 * _cellsPerEdge;
        float sceneWidth = numCells * _cellEdgeSize;

        SceneCellManager scm = GetComponent<SceneCellManager>();

        // Create Cells
        for (int i = 0; i < numCells; i++) {
            float x = sceneWidth * ((i + 0.5f) / numCells - 0.5f);

            for (int j = 0; j < numCells; j++) {
                float z = sceneWidth * ((j + 0.5f) / numCells - 0.5f);
                bool isborder = i == 0 || i == numCells - 1 || j == 0 || j == numCells - 1;

                SceneCell sc = GenerateCell(x, z, i, j, isborder);
            }
        }
        scm.UpdateCellCount();
        GenerateMorphMesh();
        SaveCells();
        scm.ReloadCells();
        // GenerateCubemaps();
    }

    public void GenerateCubemaps() {
        // See if SceneCells/Cubemaps Directory Exists
        if (Directory.Exists(CUBEMAP_DIR))
            AssetDatabase.DeleteAsset(CUBEMAP_DIR);
        AssetDatabase.CreateFolder(CELL_DIR, CUBEMAP_DIR_NAME);
        SceneCellManager scm = GetComponent<SceneCellManager>();

        // Create Camera and Cubemap objects
        _cubemapBuffer = new Cubemap(scm._cubemapSize, TextureFormat.RGBA32, false);
        Camera camera = new GameObject().AddComponent<Camera>();
        camera.transform.parent = transform;
        camera.transform.localPosition = new Vector3(0, _renderHeight, 0);
        camera.transform.localRotation = Quaternion.identity;
        _cubemapFaceTex = new Texture2D(_cubemapBuffer.width, _cubemapBuffer.height, TextureFormat.RGB24, false);

        // Render Shared (top/bottom) faces
        scm.HideForCubemapRender(scm.GetNearestCubemap(Vector3.zero));
        camera.RenderToCubemap(_cubemapBuffer);
        SaveFace(CubemapFace.PositiveY, CubemapFace.PositiveY.ToString());
        SaveFace(CubemapFace.NegativeY, CubemapFace.NegativeY.ToString());

        // Write to Files
        CubemapFace[] faces = new CubemapFace[] {
            CubemapFace.PositiveX, CubemapFace.NegativeX,
            CubemapFace.PositiveZ, CubemapFace.NegativeZ
        };

        int numMaps = 2 * _cellsPerEdge + 1;
        for (int i = 0; i < numMaps; i++) {
            float x = (i - _cellsPerEdge) * _cellEdgeSize;
            for (int j = 0; j < numMaps; j++) {
                float z = (j - _cellsPerEdge) * _cellEdgeSize;
                scm.HideForCubemapRender(new Vector2Int(i, j));
                camera.transform.position = new Vector3(x, _renderHeight, z);
                camera.RenderToCubemap(_cubemapBuffer);
                foreach (CubemapFace face in faces)
                    SaveFace(face, string.Format("{0}x{1}_{2}", i, j, face.ToString()));
            }
        }
        scm.HideForCubemapRender(-Vector2Int.one);

        DestroyImmediate(_cubemapFaceTex);
        DestroyImmediate(camera.gameObject);
    }

    public void GenerateMorphMesh() {
        SceneCellManager scm = GetComponent<SceneCellManager>();
        int numCells = 2 * _cellsPerEdge;
        for (int i = 0; i < numCells; i++) {
            for (int j = 0; j < numCells; j++) {
                scm.GenerateMorph(new Vector2Int(i, j), 1024);
            }
        }
    }

    public void SaveCells() {
        var sceneCells = GetComponentsInChildren<SceneCell>();
        foreach (SceneCell sc in sceneCells) {
            sc.Save();
        }
    }

    private void SaveFace(CubemapFace face, string filename) {
        // Save to file
        _cubemapFaceTex.SetPixels(_cubemapBuffer.GetPixels(face));
        byte[] data = _cubemapFaceTex.EncodeToPNG();
        string path = string.Format("{0}/{1}.png", CUBEMAP_DIR, filename);
        File.WriteAllBytes(path, data);

        // Set proper settings
        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        importer.isReadable = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
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

    private SceneCell GenerateCell(float x, float z, int i, int j, bool border = false) {
        GameObject cell = new GameObject(string.Format("{0}_{1}", i, j));
        cell.transform.SetParent(transform);
        cell.transform.localPosition = new Vector3(x, 0, z);
        cell.transform.localRotation = Quaternion.identity;
        cell.transform.localScale = Vector3.one;
        MeshFilter meshFilter = cell.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cell.AddComponent<MeshRenderer>();
        SceneCell sceneCell = cell.AddComponent<SceneCell>();
        sceneCell._i = i;
        sceneCell._j = j;

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
        for (int k = 0; k < meshFilters.Length; k++) {
            combine[k].mesh = meshFilters[k].sharedMesh;
            combine[k].transform = cell.transform.worldToLocalMatrix * meshFilters[k].transform.localToWorldMatrix;
        }
        meshFilter.sharedMesh = new Mesh();
        meshFilter.sharedMesh.CombineMeshes(combine);
        meshRenderer.material = _material;

        cell.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;

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

        if (GUILayout.Button("Generate City"))
            myTarget.Generate();
        if (GUILayout.Button("Generate Cubemaps"))
            myTarget.GenerateCubemaps();
        if (GUILayout.Button("Generate Morph Mesh"))
            myTarget.GenerateMorphMesh();
        if (GUILayout.Button("Save Cells"))
            myTarget.SaveCells();

        base.OnInspectorGUI();
    }
}
#endif
