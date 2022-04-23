using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour {

    public Material _material;
    public Cubemap _cubemap;

    private Camera _camera;
    private CommandBuffer _cb;
    private Mesh _skyMesh;

    private void Awake() {
        _camera = GetComponent<Camera>();
        _cb = new CommandBuffer();
        _material = new Material(_material);

        // Load Cubemap
        string cubemapDir = "Assets/Resources/SceneCells/Cubemaps";
        Texture2D[] faceTextures = new Texture2D[6];
        CubemapFace[] faces = new CubemapFace[] {
            CubemapFace.PositiveX, CubemapFace.NegativeX,
            CubemapFace.PositiveY, CubemapFace.NegativeY,
            CubemapFace.PositiveZ, CubemapFace.NegativeZ
        };
        for (int i = 0; i < 6; i++) {
            string filename = cubemapDir + "/Cubemap0_" + faces[i].ToString() + ".png";
            faceTextures[i] = AssetDatabase.LoadAssetAtPath(filename, typeof(Texture2D)) as Texture2D;
        }
        _cubemap = new Cubemap(faceTextures[0].width, TextureFormat.RGB24, false);
        for (int i = 0; i < 6; i++) {
            Color[] colors = faceTextures[i].GetPixels();
            _cubemap.SetPixels(colors, faces[i]);
        }
        _cubemap.Apply();

        // Set Cubemap to shader
        _material.SetTexture("_Cubemap", _cubemap);

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
    }

    private void Update() {

    }
}
