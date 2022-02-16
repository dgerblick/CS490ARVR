using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour {

    public float offset = -0.05f;

    private bool _calibHeight = false;
    private OVRHand _activeHand;
    private CapsuleCollider[] _activeColliders;

    private void Start() {
    
    }

    private void Update() {
        if (_calibHeight && _activeHand.enabled)
            CalibHeight();
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

        if (_activeHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            _calibHeight = false;
    }

    public void StartCalibHeight(OVRHand hand) {
        _calibHeight = true;
        _activeHand = hand;
        _activeColliders=hand.GetComponentsInChildren<CapsuleCollider>();
    }
}
    