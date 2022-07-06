using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DisableOnConnect : NetworkBehaviour
{
    [SerializeField] private InputManager input;
    [SerializeField] private CarController controller;
    [SerializeField] private GameObject cam;


    private void Start()
    {
        if (IsLocalPlayer) { return; }
        input.enabled = false;
        controller.enabled = false;
        cam.SetActive(false);
    }
}