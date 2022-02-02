using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSnap : OVRGrabbable {

    public GameObject prize;
    public PuzzleSnap piece0;
    public int id = 0;
    public float maxAngle = Mathf.Cos(45.0f * Mathf.Deg2Rad);
    public float puzzleSnapOffset = 0.3f;
    public float puzzleSnapMargin = 0.5f;
    public PuzzleSnap north = null;
    public PuzzleSnap south = null;
    public PuzzleSnap east = null;
    public PuzzleSnap west = null;
    private float grabbedAt;

    override public void GrabBegin(OVRGrabber hand, Collider grabPoint) {
        grabbedAt = Time.time;
        if (north != null) {
            north.south = null;
            north = null;
        }
        if (south != null) {
            south.north = null;
            south = null;
        }
        if (east != null) {
            east.west = null;
            east = null;
        }
        if (west != null) {
            west.east = null;
            west = null;
        }
        base.GrabBegin(hand, grabPoint);
    }

    private void Complete() {
        bool complete = id == 0 &&
                        east != null && east.id == 3 &&
                        south != null && south.id == 2 &&
                        east.south != null && east.south.id == 1 &&
                        south.east != null && south.east.id == 1;
        if (complete) {
            Vector3 midpoint = (transform.position +
                                east.transform.position +
                                south.transform.position +
                                south.east.transform.position) / 4;
            midpoint.y = (midpoint.x + midpoint.z) / 2;
            Instantiate(prize, midpoint, transform.rotation);
        }
    }

    private bool ShouldDeselect(PuzzleSnap piece) {
        if (isGrabbed && piece.isGrabbed)
            return piece.grabbedAt > grabbedAt;
        if (!isGrabbed && !piece.isGrabbed)
            return piece.id > id;
        return isGrabbed && !piece.isGrabbed;
    }

    private bool AlignTo(GameObject go, Vector3 snap) {
        Vector3 closestAxis = go.transform.forward;
        Vector3[] axes = { go.transform.right, -go.transform.right, -go.transform.forward };
        foreach (Vector3 axis in axes) {
            if (Vector3.Dot(axis, transform.forward) > Vector3.Dot(closestAxis, transform.forward))
                closestAxis = axis;
        }
        if (Vector3.Dot(closestAxis, transform.forward) < maxAngle)
            return false;
        transform.up = go.transform.up;
        transform.forward = closestAxis;
        transform.position = snap;
        return true;
    }

    private void OnTriggerStay(Collider other) {
        if (other.tag != "PuzzlePiece")
            return;
        PuzzleSnap piece = other.GetComponent<PuzzleSnap>();

        Vector3 nSnap = piece.transform.position + piece.transform.forward * puzzleSnapOffset;
        Vector3 sSnap = piece.transform.position - piece.transform.forward * puzzleSnapOffset;
        Vector3 eSnap = piece.transform.position + piece.transform.right * puzzleSnapOffset;
        Vector3 wSnap = piece.transform.position - piece.transform.right * puzzleSnapOffset;

        if (this == piece.north && Vector3.Distance(nSnap, transform.position) < puzzleSnapMargin) {
            AlignTo(piece.gameObject, nSnap);
        } else if (this == piece.east && Vector3.Distance(eSnap, transform.position) < puzzleSnapMargin) {
            AlignTo(piece.gameObject, eSnap);
        }

        if (ShouldDeselect(piece))
            return;
        if (Vector3.Dot(piece.transform.up, transform.up) < maxAngle)
            return;

        if (south == null && Vector3.Distance(nSnap, transform.position) < puzzleSnapMargin && AlignTo(piece.gameObject, nSnap)) {
            south = piece;
            piece.north = this;
            piece0.Complete();
        } else if (north == null && Vector3.Distance(sSnap, transform.position) < puzzleSnapMargin && AlignTo(piece.gameObject, sSnap)) {
            north = piece;
            piece.south = this;
            piece0.Complete();
        } else if (west == null && Vector3.Distance(eSnap, transform.position) < puzzleSnapMargin && AlignTo(piece.gameObject, eSnap)) {
            west = piece;
            piece.east = this;
            piece0.Complete();
        } else if (east == null && Vector3.Distance(wSnap, transform.position) < puzzleSnapMargin && AlignTo(piece.gameObject, wSnap)) {
            east = piece;
            piece.west = this;
            piece0.Complete();
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
