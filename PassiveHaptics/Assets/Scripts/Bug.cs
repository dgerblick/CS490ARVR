using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bug : MonoBehaviour {

    public Table table;
    public float speed = 1.0f;
    public float shakeRate = 2.0f;
    public float shakeAmount = 15.0f;
    public float xBounds;
    public float zBounds;

    private AudioSource _audioSource;

    private void Awake() {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update() {
        if (transform.localPosition.x >= xBounds
            || transform.localPosition.x <= -xBounds
            || transform.localPosition.z >= zBounds
            || transform.localPosition.z <= -zBounds) {
            Vector2 heading = new Vector2();
            float angle = Mathf.Deg2Rad * transform.localEulerAngles.y;
            heading.x = Mathf.Cos(angle);
            heading.y = Mathf.Sin(angle);
            if (transform.localPosition.x >= xBounds || transform.localPosition.x <= -xBounds)
                heading.y *= -1;
            if (transform.localPosition.z >= zBounds || transform.localPosition.z <= -zBounds)
                heading.x *= -1;
            transform.Translate(Vector3.back * Time.deltaTime * speed, Space.Self);
            transform.localEulerAngles = Vector3.up * Mathf.Rad2Deg * Mathf.Atan2(heading.y, heading.x);
        }
        transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
    }

    private void OnTriggerEnter(Collider other) {
        table.RemoveBug(this);
        _audioSource.PlayOneShot(_audioSource.clip);
        transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y / 4, transform.localScale.z);
        enabled = false;
        Destroy(gameObject, _audioSource.clip.length);
    }
}
