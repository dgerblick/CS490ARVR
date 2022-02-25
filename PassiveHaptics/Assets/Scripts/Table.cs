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
    public OVRHand lHand;
    public OVRHand rHand;
    public GameObject lButton;
    public GameObject rButton;
    public TMPro.TextMeshPro textMesh;
    public Bug bugPrefab;
    public float bugSpawnTime = 10.0f;
    public float bugSpawnRate = 10.0f;
    public float touchCubeOffset = 0.1f;
    public Material deselected;
    public Material selected;
    public List<Bug> bugs;

    private OVRHand _activeHand;
    private bool _leftIn = false;
    private bool _rightIn = false;
    public Vector3 _redirectStart;
    private CapsuleCollider[] _activeColliders;
    private TableAnchor[] _anchors;
    private GameObject _tableCube;
    private GameObject _button;
    private GameObject _touchCubesParent;
    private GameObject[] _touchCubes;
    private float _bugSpawnDelay;
    private float _bugSpawnCountdown;
    private int _bugScore = 0;
    private float _minX;
    private float _minZ;
    private float _maxX;
    private float _maxZ;
    private float _detectRadius;
    private int _selectedCube = -1;

    private void Start() {
        _anchors = GetComponentsInChildren<TableAnchor>();
        _button = GetComponentInChildren<CustomButton>().transform.parent.gameObject;
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
            _touchCubes[i].GetComponent<BoxCollider>().enabled = false;
            _touchCubes[i].GetComponent<MeshRenderer>().material = deselected;
        }

        textMesh.SetText("To begin, press one of the red buttons at your feet (Watch your head!)");
    }

    private void LateUpdate() {
        _leftIn = lHand.IsTracked
                  && Mathf.Pow(lHand.transform.parent.position.x - transform.position.x, 2)
                     + Mathf.Pow(lHand.transform.parent.position.z - transform.position.z, 2) <= Mathf.Pow(_detectRadius, 2)
                  && Vector3.Distance(lHand.transform.parent.position, transform.position) <= _detectRadius
                  && lHand.transform.parent.position.y > transform.position.y;
        _rightIn = rHand.IsTracked
                   && Mathf.Pow(rHand.transform.parent.position.x - transform.position.x, 2)
                     + Mathf.Pow(rHand.transform.parent.position.z - transform.position.z, 2) <= Mathf.Pow(_detectRadius, 2)
                   && rHand.transform.parent.position.y > transform.position.y;
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
        if (bugs.Contains(bug)) {
            bugs.Remove(bug);
            _bugScore++;
            textMesh.SetText("Smash all of the bugs!\nScore: " + _bugScore);

            if (bugs.Count == 0) {
                state = State.Redirect;
                lButton.transform.position = transform.TransformPoint(_maxX - lButton.transform.localScale.x, 0, _minZ + lButton.transform.localScale.z);
                rButton.transform.position = transform.TransformPoint(_maxX - rButton.transform.localScale.x, 0, _maxZ - rButton.transform.localScale.z);
                lButton.SetActive(true);
                rButton.SetActive(true);
                textMesh.SetText("Pinch to set the size and position of the real object");
            }
        }
    }

    private bool SetTouchCubeSize(OVRHand hand) {
        if (!hand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            return false;
        float minY = float.PositiveInfinity;
        CapsuleCollider[] colliders = hand.GetComponentsInChildren<CapsuleCollider>();
        foreach (CapsuleCollider collider in colliders)
            minY = Mathf.Min(minY, collider.bounds.min.y);
        if (minY != float.PositiveInfinity) {
            Vector3 newPos = new Vector3(
                hand.transform.position.x,
                (minY + transform.position.y) / 2,
                hand.transform.position.z
            );
            if (!_touchCubesParent.activeSelf)
                _touchCubesParent.SetActive(true);
            _touchCubesParent.transform.position = newPos;
            _touchCubesParent.transform.localScale = Mathf.Abs(transform.position.y - minY) * Vector3.one;
            _selectedCube = -1;
            return true;
        }
        return false;
    }

    private void RedirectHand(Transform hand, Vector3 warpStart, Vector3 warpEnd, Vector3 handEnd) {
        Vector3 warp = warpEnd - warpStart;
        float a = Mathf.Clamp(Vector3.Dot(warp, hand.parent.transform.position - warpStart) / warp.sqrMagnitude, 0, 1);
        hand.position = hand.parent.transform.position + a * (handEnd - warpEnd);
    }

    private void Redirect() {
        bool isMoving = false;
        if (_rightIn)
            isMoving = SetTouchCubeSize(rHand);
        else if (_leftIn)
            isMoving = SetTouchCubeSize(lHand);

        if (_selectedCube == -1) {
            lHand.transform.localPosition = Vector3.zero;
            rHand.transform.localPosition = Vector3.zero;
        } else {
            Vector3 center = _touchCubesParent.transform.TransformPoint(_touchCubes[1].transform.localPosition + Vector3.up);
            Vector3 target = _touchCubesParent.transform.TransformPoint(_touchCubes[_selectedCube].transform.localPosition + Vector3.up);
            RedirectHand(rHand.transform, _redirectStart, center, target);
            RedirectHand(lHand.transform, _redirectStart, center, target);
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
                Random.Range(-0.5f, 0.5f) * _tableCube.transform.localScale.x,
                thickness / 2,
                Random.Range(-0.5f, 0.5f) * _tableCube.transform.localScale.z
            );
            bug.table = this;
            bug.xBounds = (_maxX - _minX) / 2;
            bug.zBounds = (_maxZ - _minZ) / 2;
            bugs.Add(bug);
            _bugSpawnDelay += 1 / bugSpawnRate;
        }
    }

    private void Rescale() {
        SetTableCubeSize();

        if (!_button.activeSelf && !_leftIn && !_rightIn) {
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
        _detectRadius = Mathf.Min((_maxX - _minX) / 2, (_maxZ - _minZ) / 2);
        transform.Translate((_maxX + _minX) / 2, 0, (_maxZ + _minZ) / 2);
        _tableCube.transform.localScale = new Vector3(_maxX - _minX, thickness, _maxZ - _minZ);
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
        lButton.SetActive(false);
        rButton.SetActive(false);
        textMesh.SetText("To set the virtual table height, place your hand on the real table and pinch your thumb and index finger.");
    }

    public void StartRedirect(OVRHand hand) {
        if (state != State.Redirect)
            return;
        _redirectStart = hand.transform.parent.position;
    }

    public void NewRedirectTarget(OVRHand hand) {
        if (state != State.Redirect)
            return;
        foreach (GameObject cube in _touchCubes)
            cube.GetComponent<MeshRenderer>().material = deselected;
        _selectedCube = Random.Range(0, 3);
        _touchCubes[_selectedCube].GetComponent<MeshRenderer>().material = selected;
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
