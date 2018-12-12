using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccelerometorDisplay : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
#if UNITY_ANDROID
        Input.gyro.enabled = true;
#else
        this.gameObject.SetActive(false);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        string text = Input.acceleration.ToString();
        text += "\n";
        text += Input.gyro.enabled;
        text += "\n";
        text += Input.gyro.attitude.eulerAngles;
        text += "\n";
        text += Mathf.Atan2(-Input.acceleration.y, -Input.acceleration.z);
        GetComponent<Text>().text = text;
    }
}
