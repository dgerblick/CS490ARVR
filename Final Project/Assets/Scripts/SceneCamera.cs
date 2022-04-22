using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour {

    public Material _material;

    private Camera _camera;
    private CommandBuffer _cb;
    private Mesh _skyMesh;

    private void Awake() {
        _camera = GetComponent<Camera>();
        _cb = new CommandBuffer();
        _material = new Material(_material);

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
