using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour {

    public float offset = -0.05f;
    public float thickness = 0.025f;
    public float buttonAppearDist = 0.2f;
    public OVRHand lHand;
    public OVRHand rHand;

    private bool _calibHeight = false;
    private bool _calibSize = false;
    private OVRHand _activeHand;
    private CapsuleCollider[] _activeColliders;
    private TableAnchor[] _anchors;
    private GameObject _cube;
    private GameObject _button;

    private void Start() {
        _anchors = GetComponentsInChildren<TableAnchor>();
        _button = GetComponentInChildren<CustomButton>().transform.parent.gameObject;

        _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cube.transform.SetParent(transform);
        _cube.transform.localPosition = Vector3.zero;
        _cube.transform.localRotation = Quaternion.identity;
        _cube.transform.localScale = new Vector3(0, thickness, 0);
        SetCubeSize();
    }

    private void LateUpdate() {
        if (_calibHeight && _activeHand.IsTracked)
            CalibHeight();
        else if (_calibSize)
            Rescale();
    }

    private void Rescale() {
        SetCubeSize();

        if (!_button.activeSelf &&
                (!lHand.IsTracked ||
                    Vector3.Distance(lHand.transform.position, transform.position) > buttonAppearDist) &&
                (!rHand.IsTracked ||
                    Vector3.Distance(rHand.transform.position, transform.position) > buttonAppearDist)) {
            _button.SetActive(true);
            _calibSize = false;
        }
    }

    private void SetCubeSize() {
        float minX = float.PositiveInfinity;
        float minZ = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxZ = float.NegativeInfinity;
        Vector3[] positions = new Vector3[_anchors.Length];
        for (int i = 0; i < _anchors.Length; i++) {
            positions[i] = _anchors[i].transform.position;
            minX = Mathf.Min(_anchors[i].transform.localPosition.x, minX);
            minZ = Mathf.Min(_anchors[i].transform.localPosition.z, minZ);
            maxX = Mathf.Max(_anchors[i].transform.localPosition.x, maxX);
            maxZ = Mathf.Max(_anchors[i].transform.localPosition.z, maxZ);
        }
        transform.Translate((maxX + minX) / 2, 0, (maxZ + minZ) / 2);
        _cube.transform.localScale = new Vector3(maxX - minX, thickness, maxZ - minZ);
        for (int i = 0; i < _anchors.Length; i++) {
            _anchors[i].transform.position = positions[i];
            _anchors[i].transform.localPosition = Vector3.Project(_anchors[i].transform.localPosition, _anchors[i].scaleVec);
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
            _calibSize = true;
            foreach (TableAnchor anchor in _anchors) {
                anchor.GetComponent<MeshRenderer>().enabled = true;
                anchor.enabled = true;
            }
            _button.SetActive(false);
        }
    }

    public void StartCalibHeight(OVRHand hand) {
        _calibHeight = true;
        _calibSize = false;
        _activeHand = hand;
        _activeColliders = hand.GetComponentsInChildren<CapsuleCollider>();
        foreach (TableAnchor anchor in _anchors) {
            anchor.GetComponent<MeshRenderer>().enabled = false;
            anchor.enabled = false;
        }
        _button.SetActive(false);
    }

    public void EndCalib(OVRHand hand) {
        _calibHeight = false;
        _calibSize = false;
        foreach (TableAnchor anchor in _anchors) {
            anchor.GetComponent<MeshRenderer>().enabled = false;
            anchor.enabled = false;
        }
        _button.SetActive(false);
    }
}
