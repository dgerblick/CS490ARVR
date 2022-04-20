using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class SceneGenerator : MonoBehaviour {
    public int cellsPerEdge;
    public float cellEdgeSize;
    public Material material;

    // Re-Generate the scene
    public void Generate() {
        Order66(transform);

        float sceneWidth = cellsPerEdge * cellEdgeSize;
        for (int i = 0; i < cellsPerEdge; i++) {
            float x = sceneWidth * ((float)i / cellsPerEdge - 0.5f) + cellEdgeSize / 2;
            for (int j = 0; j < cellsPerEdge; j++) {
                float z = sceneWidth * ((float)j / cellsPerEdge - 0.5f) + cellEdgeSize / 2;
                GenerateCell(x, z);
            }
        }

    }

    // RotS 1:23:37
    private static void Order66(Transform anakin) {
        GameObject[] younglings = new GameObject[anakin.childCount];
        for (int i = 0; i < younglings.Length; i++)
            younglings[i] = anakin.GetChild(i).gameObject;
        foreach (GameObject youngling in younglings)
            GameObject.DestroyImmediate(youngling);
    }

    private GameObject GenerateCell(float x, float z) {
        GameObject cell = new GameObject("Cell");
        cell.transform.SetParent(transform);
        cell.transform.localPosition = new Vector3(x, 0, z);
        cell.transform.localRotation = Quaternion.identity;
        cell.transform.localScale = Vector3.one;
        MeshFilter meshFilter = cell.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = cell.AddComponent<MeshRenderer>();

        // Generate Plane
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.SetParent(cell.transform);
        plane.transform.localPosition = Vector3.zero;
        plane.transform.localRotation = Quaternion.identity;
        plane.transform.localScale = Vector3.one;

        // Placeholder Cube
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(cell.transform);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localRotation = Quaternion.identity;
        cube.transform.localScale = Vector3.one;

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

        return cell;
    }
}
