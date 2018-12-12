using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputerInput : InputController
{
    protected override void Init()
    {
    }

    protected override Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    protected override bool GetJump()
    {
        return Input.GetButton("Jump");
    }
}
