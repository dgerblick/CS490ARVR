using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer), typeof(LaserPointer))]
public class LaserTeleporter : MonoBehaviour {

    public GameObject cameraRigObject;
    public OVRInput.RawButton teleportButtonLeft = OVRInput.RawButton.X;
    public OVRInput.RawButton teleportButtonRight = OVRInput.RawButton.A;
    public float laserMaxLength = 100f;
    public float laserTimeToOpen = 4f;
    public Material validTeleport;
    public Material invalidTeleport;
    public float backupDistance = 0.1f;
    public float maxInclineDeg = 45f;

    private LineRenderer _lineRenderer;
    private LaserPointer _laserPointer;
    private Transform _activeController;
    private float _laserOpenTime;
    private float _laserOpenRate;
    private OVRCameraRig _cameraRig;
    private float _maxIncline;

    private void Awake() {
        _lineRenderer = GetComponent<LineRenderer>();
        _laserPointer = GetComponent<LaserPointer>();
        _cameraRig = cameraRigObject.GetComponent<OVRCameraRig>();
    }

    private void Start() {
        _laserOpenRate = laserMaxLength / laserTimeToOpen;
        _lineRenderer.material = validTeleport;
        _activeController = _cameraRig.leftHandAnchor;
        _maxIncline = Mathf.Cos(Mathf.Deg2Rad * maxInclineDeg);
    }

    private void Update() {
        if (_lineRenderer.enabled) {
            // Update Laser position
            _laserPointer.SetCursorRay(_activeController);

            // Laser Length animation
            if (_laserOpenTime <= laserTimeToOpen) {
                _laserOpenTime += Time.deltaTime;

                _laserPointer.maxLength = _laserOpenRate * _laserOpenTime;
            } else {
                _laserPointer.maxLength = laserMaxLength;
            }

            // Laser hit detection / Set laser color
            RaycastHit hit;
            bool didHit = Physics.Raycast(_activeController.position, _activeController.transform.forward, out hit, laserMaxLength);
            _lineRenderer.material = didHit ? validTeleport : invalidTeleport;
            if (didHit)
                _laserPointer.maxLength = Mathf.Min(_laserPointer.maxLength, hit.distance);

            if (OVRInput.GetDown(teleportButtonLeft) || OVRInput.GetDown(teleportButtonRight)) {
                // Turn off laser
                _lineRenderer.enabled = false;
                _laserOpenTime = 0f;
                _laserPointer.maxLength = 0f;
            } else if (OVRInput.GetUp(teleportButtonLeft | teleportButtonRight)) {
                // Turn off laser
                _lineRenderer.enabled = false;
                _laserOpenTime = 0f;
                _laserPointer.maxLength = 0f;

                // Teleport
                if (didHit) {
                    Vector3 delta = hit.point - _cameraRig.transform.position;
                    if (hit.normal.y < _maxIncline) {
                        // Back up and go to ground if wall is hit
                        RaycastHit downHit;
                        if (Physics.Raycast(hit.point + backupDistance * hit.normal, Vector3.down, out downHit, laserMaxLength))
                            delta = downHit.point - _cameraRig.transform.position;
                    }

                    // Correct for transform in play area
                    delta.x -= _cameraRig.centerEyeAnchor.localPosition.x;
                    delta.z -= _cameraRig.centerEyeAnchor.localPosition.z;

                    // Actually teleport
                    _cameraRig.transform.Translate(delta);
                }
            }
        } else if (OVRInput.GetDown(teleportButtonRight)) {
            // Start laser from right controller
            _activeController = _cameraRig.rightControllerAnchor;
            _lineRenderer.enabled = true;
        } else if (OVRInput.GetDown(teleportButtonLeft)) {
            // Start laser from left controller
            _activeController = _cameraRig.leftControllerAnchor;
            _lineRenderer.enabled = true;
        }
    }
}
