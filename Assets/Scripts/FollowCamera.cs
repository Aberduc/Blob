using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public SwingPlayer m_player;
    public float m_maxAngleDifference;

    private Vector3 m_planeNormal;


    // Use this for initialization
    void Start()
    {
        m_planeNormal = Vector3.Cross(transform.forward, transform.up).normalized;
    }

    // Update is called once per frame
    void Update()
    {

        float angleDifference = Vector3.Angle(Vector3.ProjectOnPlane(m_player.transform.position, m_planeNormal) - transform.position, transform.forward);

        if (angleDifference > m_maxAngleDifference)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.LookRotation(Vector3.ProjectOnPlane(m_player.transform.position, m_planeNormal) - transform.position), (angleDifference - m_maxAngleDifference) / angleDifference);
        }


        Debug.DrawRay(transform.position, 10 * transform.forward, Color.yellow);
    }
}
