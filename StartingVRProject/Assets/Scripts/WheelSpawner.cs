using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSpawner : MonoBehaviour {

    public GameObject[] wheelOptions;
    public GameObject[] spawnerOptions;
    public GameObject indicator;
    public OVRInput.Controller controller;
    public float triggerThreshold = 0.75f;
    public float thumbThreshold = 0.75f;
    public float indicatorDist = 0.065f;
    public float spawnDist = 0.065f;

    private float _thumbThresholdSqr;
    private int _index = -1;

    void Start() {
        _thumbThresholdSqr = thumbThreshold * thumbThreshold;
        transform.localScale = Vector3.zero;
    }

    void Update() {
        float trigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller);
        if (trigger < triggerThreshold) {
            transform.localScale = Vector3.zero;
            if (_index != -1) {
                Vector3 pos = transform.position + transform.forward * spawnDist;
                Instantiate(spawnerOptions[_index], pos, transform.rotation);
                _index = -1;
            }
            return;
        }
        transform.localScale = Vector3.one * (trigger - triggerThreshold) / (1.0f - triggerThreshold);
        Vector2 thumb = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, controller);
        if (thumb.sqrMagnitude < _thumbThresholdSqr) {
            indicator.transform.localPosition = Vector3.zero;
            indicator.transform.localScale = Vector3.zero;
            _index = -1;
        } else {
            thumb.Normalize();
            float angle = Mathf.Atan2(thumb.x, thumb.y);
            if (angle < 0)
                angle += 2 * Mathf.PI;
            indicator.transform.localPosition = indicatorDist * new Vector3(thumb.x, thumb.y, 0.0f);
            indicator.transform.localScale = 0.02f * Vector3.one;
            _index = Mathf.RoundToInt(wheelOptions.Length * angle / (2 * Mathf.PI));
        }
        for (int i = 0; i < wheelOptions.Length; i++) {
            if (i == _index)
                wheelOptions[i].transform.localScale = 1.5f * Vector3.one;
            else
                wheelOptions[i].transform.localScale = Vector3.one;
        }
    }
}
