using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndZone : MonoBehaviour
{
    private float m_timeEnter;
    private bool m_wasInTriggerLastFrame;
    public float m_timeBeforeWinning = 1f;

    public LayerMask m_checkingLayer;
    public float m_radius;

    private void FixedUpdate()
    {
        // check
        if (Physics.OverlapSphere(transform.position, m_radius, m_checkingLayer).Length > 0)
        {
            if (!m_wasInTriggerLastFrame)
            {
                m_timeEnter = Time.time;
            }

            if (Time.time - m_timeEnter > m_timeBeforeWinning)
            {
                if (!GetComponent<ParticleSystem>().isPlaying)
                    GetComponent<ParticleSystem>().Play();
            }

            m_wasInTriggerLastFrame = true;
        }
        else
        {
            GetComponent<ParticleSystem>().Stop();
            m_wasInTriggerLastFrame = false;
        }
    }
}
