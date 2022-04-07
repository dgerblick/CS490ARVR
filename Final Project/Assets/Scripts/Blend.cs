using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blend : MonoBehaviour {
    private Material _material;
    private Camera _camera;
    
    private void Awake() {
        _material = new Material(Shader.Find("Hidden/Blend"));
        _camera = GetComponent<Camera>();
        _camera.depthTextureMode = DepthTextureMode.Depth;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination) {
        Graphics.Blit(source, destination, _material);
    }
}
