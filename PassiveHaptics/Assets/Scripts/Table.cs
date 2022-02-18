using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour {

    public enum State {
        None,
        CalibHeight,
        CalibSize,
        BugGameSpawn,
        BugGamePlay,
    };

    public State state = State.None;
    public float offset = -0.05f;
    public float thickness = 0.025f;
    public float buttonAppearDist = 0.2f;
    public OVRHand lHand;
    public OVRHand rHand;
    public Bug bugPrefab;
    public float bugSpawnTime = 10.0f;
    public float bugSpawnRate = 10.0f;
    public List<Bug> bugs;

    private OVRHand _activeHand;
    private CapsuleCollider[] _activeColliders;
    private TableAnchor[] _anchors;
    private GameObject _cube;
    private GameObject _button;
    private float _bugSpawnDelay;
    private float _bugSpawnCountdown;
    private float _minX;
    private float _minZ;
    private float _maxX;
    private float _maxZ;

    private void Start() {
        _anchors = GetComponentsInChildren<TableAnchor>();
        _button = GetComponentInChildren<CustomButton>().transform.parent.gameObject;
        _bugSpawnDelay = 0.0f;
        _bugSpawnCountdown = bugSpawnTime;

        _cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cube.transform.SetParent(transform);
        _cube.transform.localPosition = Vector3.zero;
        _cube.transform.localRotation = Quaternion.identity;
        _cube.transform.localScale = new Vector3(0, thickness, 0);
        SetCubeSize();
    }

    private void LateUpdate() {
        switch (state) {
            case State.CalibHeight:
                if (_activeHand.IsTracked)
                    CalibHeight();
                break;
            case State.CalibSize:
                Rescale();
                break;
            case State.BugGameSpawn:
                BugGameSpawn();
                break;
            default:
                break;
        }
    }

    private void BugGameSpawn() {
        if (_bugSpawnCountdown <= 0) {
            state = State.BugGamePlay;
            _bugSpawnCountdown = bugSpawnTime;
            return;
        }
        _bugSpawnCountdown -= Time.deltaTime;
        _bugSpawnDelay -= Time.deltaTime;
        while (_bugSpawnDelay <= 0) {
            Bug bug = Instantiate<Bug>(bugPrefab, transform);
            bug.transform.Rotate(0, Random.Range(0f, 360f), 0);
            bug.transform.localPosition = new Vector3(
                Random.Range(-0.5f, 0.5f) * _cube.transform.localScale.x,
                thickness / 2,
                Random.Range(-0.5f, 0.5f) * _cube.transform.localScale.z
            );                        
            bug.table = this;
            bug.xBounds = (_maxX - _minX) / 2;
            bug.zBounds = (_maxZ - _minZ) / 2;
            bugs.Add(bug);
            _bugSpawnDelay += 1 / bugSpawnRate;
        }
    }

    private void Rescale() {
        SetCubeSize();

        if (!_button.activeSelf &&
                (!lHand.IsTracked ||
                    Vector3.Distance(lHand.transform.position, transform.position) > buttonAppearDist) &&
                (!rHand.IsTracked ||
                    Vector3.Distance(rHand.transform.position, transform.position) > buttonAppearDist)) {
            _button.SetActive(true);
        }
    }

    private void SetCubeSize() {
        _minX = float.PositiveInfinity;
        _minZ = float.PositiveInfinity;
        _maxX = float.NegativeInfinity;
        _maxZ = float.NegativeInfinity;
        Vector3[] positions = new Vector3[_anchors.Length];
        for (int i = 0; i < _anchors.Length; i++) {
            positions[i] = _anchors[i].transform.position;
            _minX = Mathf.Min(_anchors[i].transform.localPosition.x, _minX);
            _minZ = Mathf.Min(_anchors[i].transform.localPosition.z, _minZ);
            _maxX = Mathf.Max(_anchors[i].transform.localPosition.x, _maxX);
            _maxZ = Mathf.Max(_anchors[i].transform.localPosition.z, _maxZ);
        }
        transform.Translate((_maxX + _minX) / 2, 0, (_maxZ + _minZ) / 2);
        _cube.transform.localScale = new Vector3(_maxX - _minX, thickness, _maxZ - _minZ);
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
            state = State.CalibSize;
            foreach (TableAnchor anchor in _anchors) {
                anchor.GetComponent<MeshRenderer>().enabled = true;
                anchor.enabled = true;
            }
            _button.SetActive(false);
        }
    }

    public void StartCalibHeight(OVRHand hand) {
        state = State.CalibHeight;
        _activeHand = hand;
        _activeColliders = hand.GetComponentsInChildren<CapsuleCollider>();
        foreach (TableAnchor anchor in _anchors) {
            anchor.GetComponent<MeshRenderer>().enabled = false;
            anchor.enabled = false;
        }
        _button.SetActive(false);
    }

    public void EndCalib(OVRHand hand) {
        state = State.BugGameSpawn;
        foreach (TableAnchor anchor in _anchors) {
            anchor.GetComponent<MeshRenderer>().enabled = false;
            anchor.enabled = false;
        }
        _button.SetActive(false);
    }
}
