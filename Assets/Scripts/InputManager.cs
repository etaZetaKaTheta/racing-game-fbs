using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class InputManager : NetworkBehaviour
{
    [HideInInspector] public Controls controls;

    /*private void Awake()
    {
        if(!IsLocalPlayer)
        {
            enabled = false;
        }
    }*/

    void Awake()
    {
        controls = new Controls();
    }


    private void Start()
    {
        if (!IsLocalPlayer)
        {
            enabled = false;
        }
    }
    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
