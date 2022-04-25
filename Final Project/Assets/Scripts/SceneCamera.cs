using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour {

    public SceneCellManager _manager;
    public Material _material;
    public Cubemap _cubemap;

    private Camera _camera;
    private CommandBuffer _cb;
    private Mesh _skyMesh;
    private Vector2Int _cubemapPos;

    private const string CUBEMAP_DIR = "Assets/Resources/SceneCells/Cubemaps";

    private void Awake() {
        _camera = GetComponent<Camera>();
        _cb = new CommandBuffer();
        _material = new Material(_material);
        _cubemapPos = new Vector2Int(-1, -1);

        Vector3[] verts = new Vector3[] {
            new Vector3 (-1, -1, -1),
            new Vector3 ( 1, -1, -1),
            new Vector3 ( 1,  1, -1),
            new Vector3 (-1,  1, -1),
            new Vector3 (-1,  1,  1),
            new Vector3 ( 1,  1,  1),
            new Vector3 ( 1, -1,  1),
            new Vector3 (-1, -1,  1)
        };

        int[] tris = new int[] {
            0, 2, 1,
            0, 3, 2,
            2, 3, 4,
            2, 4, 5,
            1, 2, 5,
            1, 5, 6,
            0, 7, 4,
            0, 4, 3,
            5, 4, 7,
            5, 7, 6,
            0, 6, 7,
            0, 1, 6
        };

        _skyMesh = new Mesh();
        _skyMesh.SetVertices(verts);
        _skyMesh.SetUVs(0, verts);
        _skyMesh.SetTriangles(tris, 0);
        _skyMesh.RecalculateNormals();
        _skyMesh.RecalculateBounds();

        _cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        _cb.DrawMesh(_skyMesh, Matrix4x4.identity, _material);
        _camera.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, _cb);

        _cubemap = new Cubemap(_manager._cubemapSize, TextureFormat.RGB24, 0);
        LoadCubemapFace(CubemapFace.PositiveY, string.Format("{0}/{1}.png", CUBEMAP_DIR, CubemapFace.PositiveY.ToString()));
        LoadCubemapFace(CubemapFace.NegativeY, string.Format("{0}/{1}.png", CUBEMAP_DIR, CubemapFace.NegativeY.ToString()));
        _cubemap.Apply();
        _material.SetTexture("_Cubemap", _cubemap);
    }

    private void LoadCubemap(int i, int j) {
        // Load Cubemap Faces
        CubemapFace[] faces = new CubemapFace[] { CubemapFace.PositiveX, CubemapFace.NegativeX, CubemapFace.PositiveZ, CubemapFace.NegativeZ };
        foreach (CubemapFace face in faces)
            LoadCubemapFace(face, string.Format("{0}/{1}x{2}_{3}.png", CUBEMAP_DIR, i, j, face));
        _cubemap.Apply();

        // Set Cubemap to shader
        // _material.SetTexture("_Cubemap", _cubemap);
    }

    private void LoadCubemapFace(CubemapFace face, string filename) {
        Texture2D tex = AssetDatabase.LoadAssetAtPath(filename, typeof(Texture2D)) as Texture2D;
        Color[] colors = tex.GetPixels();
        Resources.UnloadAsset(tex);
        _cubemap.SetPixels(colors, face);
    }

    private void Update() {
        Vector2Int cubemapPos = _manager.GetCubemapIdx(transform.position);
        if (cubemapPos != _cubemapPos) {
            _cubemapPos = cubemapPos;
            LoadCubemap(_cubemapPos.x, _cubemapPos.y);
            Debug.LogFormat("At Cubemap Position: {0}x{1}", _cubemapPos.x, _cubemapPos.y);
        }
    }
}
