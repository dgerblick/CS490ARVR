using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSnap : OVRGrabbable {

    public int id = 0;
    public float maxAngle = Mathf.Cos(10.0f * Mathf.Deg2Rad);
    public float puzzleSnapOffset = 0.3f;
    public float puzzleSnapMargin = 0.005f;
    public PuzzleSnap north = null;
    public PuzzleSnap south = null;
    public PuzzleSnap east = null;
    public PuzzleSnap west = null;
    private float grabbedAt;

    override public void GrabBegin(OVRGrabber hand, Collider grabPoint) {
        grabbedAt = Time.time;
        base.GrabBegin(hand, grabPoint);
    }

    private bool ShouldDeselect(PuzzleSnap piece) {
        if (isGrabbed && piece.isGrabbed)
            return piece.grabbedAt > grabbedAt;
        if (!isGrabbed && !piece.isGrabbed)
            return piece.id > id;
        return isGrabbed && !piece.isGrabbed;
    }

    private void AlignTo(GameObject go, Vector3 snap) {
        transform.up = go.transform.up;
        Vector3 closestAxis = go.transform.forward;
        Vector3[] axes = { go.transform.right, -go.transform.right, -go.transform.forward };
        foreach (Vector3 axis in axes) {
            if (Vector3.Dot(axis, transform.forward) > Vector3.Dot(closestAxis, transform.forward))
                closestAxis = axis;
        }
        transform.forward = closestAxis;
        Vector3 delta = snap - transform.position;
        transform.position = snap;
    }

    private void OnTriggerStay(Collider other) {
        if (other.tag != "PuzzlePiece")
            return;
        PuzzleSnap piece = other.GetComponent<PuzzleSnap>();

        Vector3 nSnap = piece.transform.position + piece.transform.forward * puzzleSnapOffset;
        Vector3 sSnap = piece.transform.position - piece.transform.forward * puzzleSnapOffset;
        Vector3 eSnap = piece.transform.position + piece.transform.right * puzzleSnapOffset;
        Vector3 wSnap = piece.transform.position - piece.transform.right * puzzleSnapOffset;

        if (piece == north) {
            AlignTo(piece.gameObject, sSnap);
        //} else if (piece == south) {
        //    AlignTo(piece.gameObject, nSnap);
        } else if (piece == east) {
            AlignTo(piece.gameObject, wSnap);
        //} else if (piece == west) {
        //    AlignTo(piece.gameObject, eSnap);
        }

        if (ShouldDeselect(piece))
            return;
        if (Vector3.Dot(piece.transform.up, transform.up) < maxAngle)
            return;
        if (isGrabbed)
            GrabEnd(Vector3.zero, Vector3.zero);

        if (south == null && Vector3.Distance(nSnap, transform.position) < puzzleSnapMargin) {
            south = piece;
            piece.north = this;
        } else if (north == null && Vector3.Distance(sSnap, transform.position) < puzzleSnapMargin) {
            north = piece;
            piece.south = this;
        } else if (west == null && Vector3.Distance(eSnap, transform.position) < puzzleSnapMargin) {
            west = piece;
            piece.east = this;
        } else if (east == null && Vector3.Distance(wSnap, transform.position) < puzzleSnapMargin) {
            east = piece;
            piece.west = this;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.tag != "PuzzlePiece")
            return;
        PuzzleSnap piece = other.GetComponent<PuzzleSnap>();
        if (piece == north) {
            north = null;
        } else if (piece == south) {
            south = null;
        } else if (piece == east) {
            east = null;
        } else if (piece == west) {
            west = null;
        }
    }
}
