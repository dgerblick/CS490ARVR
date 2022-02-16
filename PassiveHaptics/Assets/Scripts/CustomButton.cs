using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomButton : MonoBehaviour {
    
    public UnityEvent<OVRHand> onButtonPress;
    public UnityEvent<OVRHand> onButtonRelease;
    public OVRHand lHand;
    public OVRHand rHand;

    public float pressThreshold = 0.8f;
    public float releaseThreshold = 0.7f;

    private bool _isPressed = false;
    private float _startPos;
    private ConfigurableJoint _joint;

    private void Start() {
        _isPressed = false;
        _startPos = transform.position.y;
        _joint = GetComponent<ConfigurableJoint>();
    }

    private void Update() {
        float amount = Mathf.Abs(_startPos - transform.position.y) / _joint.linearLimit.limit;
        
        if (!_isPressed && amount >= pressThreshold)
            ButtonPress();
        else if (_isPressed && amount <= releaseThreshold)
            ButtonRelease();
    }

    private void ButtonPress() {
        _isPressed = true;
        float lDist = Vector3.Distance(transform.position, lHand.transform.position);
        float rDist = Vector3.Distance(transform.position, rHand.transform.position);
        onButtonPress.Invoke(rDist < lDist ? rHand : lHand);
        Debug.Log("Pressed");
    }

    private void ButtonRelease() {
        _isPressed = false;
        float lDist = Vector3.Distance(transform.position, lHand.transform.position);
        float rDist = Vector3.Distance(transform.position, rHand.transform.position);
        onButtonRelease.Invoke(rDist < lDist ? rHand : lHand);
        Debug.Log("Released");
    }
}
