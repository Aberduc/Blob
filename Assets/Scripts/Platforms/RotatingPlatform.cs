using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour {

    public float m_angularSpeed;

    private Rigidbody m_rigidbody;

    private void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Quaternion q = transform.localRotation * Quaternion.Euler(0, m_angularSpeed * Time.fixedDeltaTime, 0);

        m_rigidbody.MoveRotation(q);
    }
}
