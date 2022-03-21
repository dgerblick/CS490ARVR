using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARRaycastManager = UnityEngine.XR.ARFoundation.ARRaycastManager;
using PlaceMultipleObjectsOnPlane = UnityEngine.XR.ARFoundation.Samples.PlaceMultipleObjectsOnPlane;

public class MyPlaneObjectManager : MonoBehaviour {

    public GameObject[] prefabs;
    public int index { get; private set; }

    private PlaceMultipleObjectsOnPlane _placer;
    private ARRaycastManager _arRaycastManager;
    private Stack<GameObject> _placedStack;

    public void NextObject() {
        index = (index + 1) % prefabs.Length;
        _placer.placedPrefab = prefabs[index];
    }

    public void Undo() {
        if (_placedStack.Count > 0)
            Destroy(_placedStack.Pop());
    }

    public void OnPlace() {
        _placedStack.Push(_placer.spawnedObject);
    }

    private void Start() {
        _placer = GetComponent<PlaceMultipleObjectsOnPlane>();
        _arRaycastManager = GetComponent<ARRaycastManager>();
        index = 0;
        _placer.placedPrefab = prefabs[index];
        PlaceMultipleObjectsOnPlane.onPlacedObject += OnPlace;
        _placedStack = new Stack<GameObject>();
    }
}
