using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bug : MonoBehaviour {

    public Table table;

    private void OnTriggerEnter(Collider other) {
        Destroy(gameObject);
        table.bugs.Remove(this);
    }
}
