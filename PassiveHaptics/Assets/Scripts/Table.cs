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
        Redirect
    };

    public State state = State.None;
    public float offset = -0.05f;
    public float thickness = 0.025f;
    public float buttonAppearDist = 0.2f;
    public OVRHand lHand;
    public OVRHand rHand;
    public TMPro.TextMeshPro textMesh;
    public Bug bugPrefab;
    public float bugSpawnTime = 10.0f;
    public float bugSpawnRate = 10.0f;
    public float touchCubeOffset = 0.1f;

    private OVRHand _activeHand;
    private CapsuleCollider[] _activeColliders;
    private TableAnchor[] _anchors;
    private GameObject _tableCube;
    private GameObject _button;
    private GameObject _touchCubesParent;
    private GameObject[] _touchCubes;
    private BoxCollider _collider;
    private float _bugSpawnDelay;
    private float _bugSpawnCountdown;
    public List<Bug> _bugs;
    public int _bugScore = 0;
    private float _minX;
    private float _minZ;
    private float _maxX;
    private float _maxZ;

    private void Start() {
        _anchors = GetComponentsInChildren<TableAnchor>();
        _button = GetComponentInChildren<CustomButton>().transform.parent.gameObject;
        _collider = GetComponent<BoxCollider>();
        _bugSpawnDelay = 0.0f;
        _bugSpawnCountdown = bugSpawnTime;

        _tableCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _tableCube.transform.SetParent(transform);
        _tableCube.transform.localPosition = Vector3.zero;
        _tableCube.transform.localRotation = Quaternion.identity;
        _tableCube.transform.localScale = new Vector3(0, thickness, 0);
        SetTableCubeSize();

        _touchCubesParent = new GameObject();
        _touchCubesParent.transform.SetParent(transform);
        _touchCubesParent.transform.localPosition = Vector3.zero;
        _touchCubesParent.transform.localRotation = Quaternion.identity;
        _touchCubesParent.SetActive(false);

        _touchCubes = new GameObject[3];
        for (int i = 0; i < 3; i++) {
            _touchCubes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            _touchCubes[i].transform.SetParent(_touchCubesParent.transform);
            _touchCubes[i].transform.localPosition = Vector3.forward * (i - 1) * touchCubeOffset;
            _touchCubes[i].transform.localRotation = Quaternion.identity;
            _touchCubes[i].transform.localScale = new Vector3(0, 0, 0);
        }

        textMesh.SetText("To begin, press one of the red buttons at your feet (Watch your head!)");
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
            case State.Redirect:
                Redirect();
                break;
            default:
                break;
        }
    }

    public void RemoveBug(Bug bug) {
        if (_bugs.Contains(bug)) {
            _bugs.Remove(bug);
            _bugScore++;
            textMesh.SetText("Smash all of the bugs!\nScore: " + _bugScore);

            if (_bugs.Count == 0) {
                state = State.Redirect;
                textMesh.SetText("Pinch to set the size and position of the real object");
            }
        }
    }

    private void SetTouchCubeSize(OVRHand hand) {
        if (!hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            return;
        float minY = float.PositiveInfinity;
        bool handInArea = false;
        CapsuleCollider[] colliders = hand.GetComponentsInChildren<CapsuleCollider>();
        foreach (CapsuleCollider collider in colliders) {
            if (_collider.bounds.Intersects(collider.bounds))
                handInArea = true;
            minY = Mathf.Min(minY, collider.bounds.min.y);
        }
        if (handInArea && minY != float.PositiveInfinity) {
            if (!_touchCubesParent.activeSelf)
                _touchCubesParent.SetActive(true);
            _touchCubesParent.transform.position = new Vector3(
                hand.transform.position.x,
                (minY + transform.position.y) / 2,
                hand.transform.position.z
            );
            for (int i = 0; i < 3; i++)
                _touchCubes[i].transform.localScale = Mathf.Abs(transform.position.y - minY) * Vector3.one;
        }
    }

    private void Redirect() {
        SetTouchCubeSize(lHand);
        SetTouchCubeSize(rHand);
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
                Random.Range(-0.5f, 0.5f) * _tableCube.transform.localScale.x,
                thickness / 2,
                Random.Range(-0.5f, 0.5f) * _tableCube.transform.localScale.z
            );
            bug.table = this;
            bug.xBounds = (_maxX - _minX) / 2;
            bug.zBounds = (_maxZ - _minZ) / 2;
            _bugs.Add(bug);
            _bugSpawnDelay += 1 / bugSpawnRate;
        }
    }

    private void Rescale() {
        SetTableCubeSize();

        if (!_button.activeSelf &&
                (!lHand.IsTracked ||
                    Vector3.Distance(lHand.transform.position, transform.position) > buttonAppearDist) &&
                (!rHand.IsTracked ||
                    Vector3.Distance(rHand.transform.position, transform.position) > buttonAppearDist)) {
            _button.SetActive(true);
        }
    }

    private void SetTableCubeSize() {
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
        _tableCube.transform.localScale = new Vector3(_maxX - _minX, thickness, _maxZ - _minZ);
        _collider.size = new Vector3(_maxX - _minX, 0.5f, _maxZ - _minZ);
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
            textMesh.SetText("With your wrist near one of the balls on the edge, pinch and move to adjust the size of the table\nPress the red button in the middle of the table when you're done");
        }
    }

    public void StartCalibHeight(OVRHand hand) {
        if (state != State.None)
            return;
        state = State.CalibHeight;
        _activeHand = hand;
        _activeColliders = hand.GetComponentsInChildren<CapsuleCollider>();
        foreach (TableAnchor anchor in _anchors) {
            anchor.GetComponent<MeshRenderer>().enabled = false;
            anchor.enabled = false;
        }
        _button.SetActive(false);
        textMesh.SetText("To set the virtual table height, place your hand on the real table and pinch your thumb and index finger.");
    }

    public void EndCalib(OVRHand hand) {
        state = State.BugGameSpawn;
        foreach (TableAnchor anchor in _anchors) {
            anchor.GetComponent<MeshRenderer>().enabled = false;
            anchor.enabled = false;
        }
        _button.SetActive(false);
        textMesh.SetText("Smash all of the bugs!\nScore: " + _bugScore);
    }
}
