using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class Baton : OVRGrabbable {

    public Vector3 offsetPos;
    public Vector3 offsetRot;

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint) {
        base.GrabBegin(hand, grabPoint);
        transform.position = hand.transform.position + offsetPos;
        transform.rotation = hand.transform.rotation * Quaternion.Euler(offsetRot);
        GetComponent<Animator>().Play("Extend");
    }

    public override void GrabEnd(Vector3 linearVelocity, Vector3 angularVelocity) {
        base.GrabEnd(linearVelocity, angularVelocity);

        GetComponent<Animator>().Play("Retract");
    }
}
