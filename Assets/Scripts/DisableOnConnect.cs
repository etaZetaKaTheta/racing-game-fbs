using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DisableOnConnect : NetworkBehaviour
{
    [SerializeField] private InputManager input;
    [SerializeField] private CarController controller;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject carUI;
    [SerializeField] private AudioSource[] audioSources;


    private void Start()
    {
        if (IsLocalPlayer) { return; }
        input.enabled = false;
        controller.enabled = false;
        cam.SetActive(false);
        carUI.SetActive(false);
        foreach (AudioSource src in audioSources)
        {
            src.enabled = false;
        }
    }
}
