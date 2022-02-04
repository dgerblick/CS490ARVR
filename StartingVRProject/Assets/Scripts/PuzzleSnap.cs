using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleSnap : OVRGrabbable {

    public GameObject prize;
    public int id = 0;
    public float maxAngle = Mathf.Cos(45.0f * Mathf.Deg2Rad);
    public float puzzleSnapOffset = 0.3f;
    public float puzzleSnapMargin = 0.5f;
    public PuzzleSnap north = null;
    public PuzzleSnap south = null;
    public PuzzleSnap east = null;
    public PuzzleSnap west = null;
    private float grabbedAt;
    private float solvedCountdown;

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
        bool[] processed = { false, false, false, false };
        PuzzleSnap[] puzzle = { null, null, null, null };
        processed[id] = true;
        Stack<PuzzleSnap> stack = new Stack<PuzzleSnap>();
        stack.Push(this);
        while (stack.Count > 0) {
            PuzzleSnap current = stack.Pop();
            int[] correct;
            switch (current.id) {
                case 0:
                    correct = new int[4] { -1, 2, 3, -1 };
                    break;
                case 1:
                    correct = new int[4] { 3, -1, -1, 2 };
                    break;
                case 2:
                    correct = new int[4] { 0, -1, 1, -1 };
                    break;
                case 3:
                    correct = new int[4] { -1, 1, -1, 0 };
                    break;
                default:
                    return;
            }
            PuzzleSnap[] actual = { current.north, current.south, current.east, current.west };
            for (int i = 0; i < 4; i++) {
                int testId = actual[i] == null ? -1 : actual[i].id;
                if (correct[i] != testId) {
                    Debug.Log(current.id + ": Failed on i=" + i + ", expecting " + correct[i] + ", got " + testId);
                    return;
                }
                if (correct[i] != -1 && !processed[correct[i]]) {
                    processed[correct[i]] = true;
                    stack.Push(actual[i]);
                }
            }
            puzzle[current.id] = current;
        }
        Vector3 spawnPoint = Vector3.zero;
        for (int i = 0; i < 4; i++) {
            if (!processed[i] || puzzle[i].isGrabbed)
                return;
            spawnPoint += puzzle[i].transform.position / 4;
        }
        spawnPoint.y += 1;
        if (puzzle[0].solvedCountdown < 0.0f) {
            Instantiate(prize, spawnPoint, transform.rotation);
            puzzle[0].solvedCountdown = 1.0f;
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
        solvedCountdown -= Time.fixedDeltaTime;

        Vector3 nSnap = piece.transform.position + piece.transform.forward * puzzleSnapOffset;
        Vector3 sSnap = piece.transform.position - piece.transform.forward * puzzleSnapOffset;
        Vector3 eSnap = piece.transform.position + piece.transform.right * puzzleSnapOffset;
        Vector3 wSnap = piece.transform.position - piece.transform.right * puzzleSnapOffset;

        if (!isGrabbed && this == piece.north && Vector3.Distance(nSnap, transform.position) < puzzleSnapMargin) {
            AlignTo(piece.gameObject, nSnap);
        } else if (!isGrabbed && this == piece.east && Vector3.Distance(eSnap, transform.position) < puzzleSnapMargin) {
            AlignTo(piece.gameObject, eSnap);
        }

        if (ShouldDeselect(piece))
            return;
        if (Vector3.Dot(piece.transform.up, transform.up) < maxAngle)
            return;

        if (south == null && Vector3.Distance(nSnap, transform.position) < puzzleSnapMargin && AlignTo(piece.gameObject, nSnap)) {
            south = piece;
            piece.north = this;
            Complete();
        } else if (north == null && Vector3.Distance(sSnap, transform.position) < puzzleSnapMargin && AlignTo(piece.gameObject, sSnap)) {
            north = piece;
            piece.south = this;
            Complete();
        } else if (west == null && Vector3.Distance(eSnap, transform.position) < puzzleSnapMargin && AlignTo(piece.gameObject, eSnap)) {
            west = piece;
            piece.east = this;
            Complete();
        } else if (east == null && Vector3.Distance(wSnap, transform.position) < puzzleSnapMargin && AlignTo(piece.gameObject, wSnap)) {
            east = piece;
            piece.west = this;
            Complete();
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
