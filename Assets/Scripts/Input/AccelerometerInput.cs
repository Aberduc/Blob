using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AccelerometerInput : InputController
{
    private float m_originalAngle;

    AccelerometerInput() : base()
    {
        InitOriginalAngle();
    }

    protected override Vector2 GetInput()
    {
        return new Vector2(Mathf.Clamp(2f * Input.acceleration.x, -1f, 1f), Mathf.Clamp(-2f * Mathf.Sin(Mathf.Atan2(-Input.acceleration.y, -Input.acceleration.z) - m_originalAngle), -1f, 1f));
    }

    protected override bool GetJump()
    {
        return Input.touchCount > 0;
    }

    public void InitOriginalAngle()
    {
        m_originalAngle = Mathf.Atan2(-Input.acceleration.y, -Input.acceleration.z);
    }

    protected override void Init()
    {
        throw new System.NotImplementedException();
    }
}