using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxSpawner : MonoBehaviour {
    public GameObject boxPrefab;
    public float loadDist = float.PositiveInfinity;
    public float renderHeight = 1.0f;
    public bool refresh = false;
    public Material skybox;
    public Material background;
    public int cubemapSize = 1024;

    public int numBoxes = 11;
    public float spacing = 1f;
    public float minScale = 0.2f;
    public float maxScale = 0.5f;
    public float minHeight = 0.1f;
    public float maxHeightClose = 0.5f;
    public float maxHeightFar = 2.0f;

    private MeshRenderer[] _boxRenderers;
    private float _loadDist = float.PositiveInfinity;
    private float _loadDistSqr = float.PositiveInfinity;
    private Cubemap _cubemap;

    private void Start() {
        float maxDist = Mathf.Sqrt(2.0f * spacing * spacing * (numBoxes + 0.5f) * (numBoxes + 0.5f));
        _boxRenderers = new MeshRenderer[(2 * numBoxes + 1) * (2 * numBoxes + 1)];
        int idx = 0;

        for (int i = -numBoxes; i <= numBoxes; i++) {
            float x = (i + 0.5f) * spacing;
            for (int j = -numBoxes; j <= numBoxes; j++) {
                GameObject cube = Instantiate(boxPrefab);

                float z = (j + 0.5f) * spacing;
                float scale = Random.Range(minScale, maxScale);
                float maxHeight = Mathf.Lerp(maxHeightClose, maxHeightFar, Mathf.Sqrt(i * i + j * j) / maxDist);
                float height = Random.Range(minHeight, maxHeight);
                cube.transform.localScale = new Vector3(scale, height, scale);
                cube.transform.position = new Vector3(x, 0.0f, z);

                cube.transform.parent = transform;
                _boxRenderers[idx] = cube.GetComponentInChildren<MeshRenderer>();
                idx++;
            }
        }

        _cubemap = new Cubemap(cubemapSize, TextureFormat.RGBA32, false);
        RefreshCubemap();
    }

    private void Update() {
        if (refresh) {
            refresh = false;
            RefreshCubemap();
        }

        foreach (MeshRenderer boxRenderer in _boxRenderers) {
            boxRenderer.enabled = Vector3.Distance(Camera.main.transform.position, boxRenderer.transform.position) < loadDist;
        }
    }

    private void RefreshCubemap() {
        RenderSettings.skybox = background;

        GameObject go = new GameObject("CubemapCamera");
        Camera camera = go.AddComponent<Camera>();

        foreach (MeshRenderer boxRenderer in _boxRenderers) {
            boxRenderer.enabled = Vector3.Distance(Camera.main.transform.position, boxRenderer.transform.position) >= loadDist;
        }

        go.transform.position = Vector3.up * renderHeight;
        go.transform.rotation = Quaternion.identity;
        camera.RenderToCubemap(_cubemap);

        skybox.SetTexture("_Tex", _cubemap);
        RenderSettings.skybox = skybox;
        DestroyImmediate(go);
    }
}
