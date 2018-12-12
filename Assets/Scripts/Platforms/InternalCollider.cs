using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InternalCollider : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.tag == "Core")
        {
            Debug.Log("CRUSHED ? ");
        }
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.gameObject.tag == "Core")
        {
            Debug.Log("CRUSHED");
        }
    }
}
