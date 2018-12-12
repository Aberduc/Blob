using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputController : MonoBehaviour
{
    public SwingPlayer m_player
    {
        protected get;
        set;
    }


    private void Start()
    {
        Init();
    }

    private void Update()
    {
        m_player.SetInput(GetInput(), GetJump());
    }

    protected abstract void Init();
    /// <summary>
    /// return a Vector2 that contains the traditional horizontal and vertical input, as respectively its x and y components.
    /// </summary>
    protected abstract Vector2 GetInput();
    /// <summary>
    /// return true if the jump button is currently pressed
    /// </summary>
    protected abstract bool GetJump();
}
