using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableAnchor : MonoBehaviour {

    public enum AnchorType {
        Scale,
    };
    public AnchorType anchorType;
    public Vector3 scaleVec;
    public float pinchDist = 0.1f;
    public OVRHand lHand;
    public OVRHand rHand;

    private OVRHand _activeHand = null;

    private void Update() {
        float lDist = Vector3.Distance(lHand.transform.position, transform.position);
        float rDist = Vector3.Distance(rHand.transform.position, transform.position);
        if (_activeHand == null && lDist < pinchDist)
            _activeHand = lHand;
        else if (_activeHand == null && rDist < pinchDist)
            _activeHand = rHand;
        
        if (_activeHand != null && _activeHand.enabled && _activeHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            transform.position = _activeHand.transform.position;
        else
            _activeHand = null;
    }
}
