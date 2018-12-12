using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinSphere : MonoBehaviour {
    public SwingPlayer m_player
    {
        private get;
        set;
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter(Collision collision)
    {
        m_player.OnSkinCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        m_player.OnSkinCollision(collision);
    }
}
