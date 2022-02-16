using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour {

    public float offset = -0.05f;
    public float thickness = 0.025f;

    private bool _calibHeight = false;
    private OVRHand _activeHand;
    private CapsuleCollider[] _activeColliders;
    private TableAnchor[] _anchors;
    private GameObject _cube;

    private void Start() {
        _anchors = GetComponentsInChildren<TableAnchor>();
        _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cube.transform.SetParent(transform);
        _cube.transform.localPosition = Vector3.zero;
        _cube.transform.localRotation = Quaternion.identity;
        _cube.transform.localScale = new Vector3(0, thickness, 0);
    }

    private void LateUpdate() {
        if (_calibHeight && _activeHand.enabled)
            CalibHeight();
        else
            Rescale();
    }

    private void Rescale() {
        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;
        foreach (TableAnchor anchor in _anchors) {
            minX = Mathf.Min(anchor.transform.localPosition.x, minX);
            minZ = Mathf.Min(anchor.transform.localPosition.z, minZ);
            maxX = Mathf.Max(anchor.transform.localPosition.x, maxX);
            maxZ = Mathf.Max(anchor.transform.localPosition.z, maxZ);
        }
        Vector3 delta = new Vector3((maxX + minX) / 2, 0, (maxZ + minZ) / 2);
        transform.Translate(delta);
        _cube.transform.localScale = new Vector3(maxX - minX, thickness, maxZ - minZ);
        foreach (TableAnchor anchor in _anchors) {
            anchor.transform.localPosition = Vector3.Project(anchor.transform.localPosition, anchor.scaleVec);
            anchor.transform.Translate(-delta);
        }

    }

    private void CalibHeight() {
        float minY = float.PositiveInfinity;
        foreach (CapsuleCollider collider in _activeColliders)
            minY = Mathf.Min(minY, collider.bounds.min.y);
        Vector3 pos = _activeHand.transform.position;
        pos.y = minY + offset;
        Vector3 right = _activeHand.transform.rotation * Vector3.right;
        Vector3 forward = Vector3.Cross(right, Vector3.up);
        Quaternion rot = Quaternion.LookRotation(forward, Vector3.up);
        transform.SetPositionAndRotation(pos, rot);

        if (_activeHand.GetFingerIsPinching(OVRHand.HandFinger.Index)) {
            _calibHeight = false;
            foreach (TableAnchor anchor in _anchors)
                anchor.enabled = true;
        }
    }

    public void StartCalibHeight(OVRHand hand) {
        _calibHeight = true;
        _activeHand = hand;
        _activeColliders = hand.GetComponentsInChildren<CapsuleCollider>();
        foreach (TableAnchor anchor in _anchors)
            anchor.enabled = false;
    }
}
