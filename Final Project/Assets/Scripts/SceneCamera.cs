using System;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class SceneCamera : MonoBehaviour {

    public SceneCellManager _manager;
    public Material _material;
    public Mesh _skyMesh;

    private Camera _camera;
    private CommandBuffer _cb;
    private Vector2Int _cubemapPos;
    private Vector2Int _cellPos;
    private Cubemap[] _cubemaps;

    private void Awake() {
        _camera = GetComponent<Camera>();
        _cb = new CommandBuffer();
        _material = new Material(_material);
        _cubemapPos = new Vector2Int(-1, -1);
        _cellPos = new Vector2Int(-1, -1);

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
        _skyMesh.SetUVs(1, verts);
        _skyMesh.SetUVs(2, verts);
        _skyMesh.SetUVs(3, verts);
        _skyMesh.SetTriangles(tris, 0);
        _skyMesh.RecalculateNormals();
        _skyMesh.RecalculateBounds();

        _cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        _cb.DrawMesh(_skyMesh, Matrix4x4.identity, _material);
        _camera.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, _cb);

        _cubemaps = new Cubemap[4];
        for (int i = 0; i < 4; i++) {
            _cubemaps[i] = new Cubemap(_manager._cubemapSize, TextureFormat.RGB24, 0);
            _manager.LoadCubemapTopBottom(_cubemaps[i]);
        }
        _material.SetTexture("_CubemapSW", _cubemaps[0]);
        _material.SetTexture("_CubemapNW", _cubemaps[1]);
        _material.SetTexture("_CubemapSE", _cubemaps[2]);
        _material.SetTexture("_CubemapNE", _cubemaps[3]);
    }

    private void Update() {
        Vector2Int cubemapPos = _manager.GetNearestCubemap(transform.position);
        if (cubemapPos != _cubemapPos) {
            _manager.ChangeCubemap(_cubemapPos, cubemapPos);
            Debug.LogFormat("At Cubemap Position: {0}x{1}", cubemapPos.x, cubemapPos.y);
            _cubemapPos = cubemapPos;
        }

        Tuple<Vector2Int, Vector2> cellPos = _manager.GetCellPos(transform.position);
        if (cellPos.Item1 != _cellPos) {
            _manager.ChangeCell(cellPos.Item1, _cubemaps, _skyMesh);
            _skyMesh.RecalculateNormals();
            _skyMesh.RecalculateBounds();
            // _camera.RemoveCommandBuffer(CameraEvent.AfterImageEffectsOpaque, _cb);
            // _camera.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, _cb);
            Debug.LogFormat("At Cell Position: {0}x{1}", cellPos.Item1.x, cellPos.Item1.y);
            _cellPos = cellPos.Item1;
        }
        _material.SetFloat("_PosX", cellPos.Item2.x);
        _material.SetFloat("_PosY", cellPos.Item2.y);
    }
}
