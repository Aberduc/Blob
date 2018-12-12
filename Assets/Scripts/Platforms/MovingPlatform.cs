using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public Vector3[] m_positions;
    public float m_speed;
    public float m_minMoveDistance = 0.01f;

    private int m_currentObjective = 0;
    private Rigidbody m_rigidbody;

    private void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Vector3 move = m_positions[m_currentObjective] - transform.position;
        if (move.magnitude < m_minMoveDistance)
        {
            m_rigidbody.MovePosition(m_positions[m_currentObjective]);
            m_currentObjective = m_currentObjective == m_positions.Length - 1 ? 0 : m_currentObjective + 1;
        }
        else
        {
            move = move.normalized * Mathf.Min(m_speed * Time.fixedDeltaTime, move.magnitude);
            m_rigidbody.MovePosition(transform.position + move);
        }
    }
}