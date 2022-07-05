using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class MenuUI : NetworkBehaviour
{
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TMP_InputField inputIP;
    [SerializeField] private Camera cam;
    public void StartHost()
    {
        transport.SetConnectionData(inputIP.text, 18769);
        NetworkManager.Singleton.StartHost();
        cam.enabled = false;
        gameObject.SetActive(false);
    }

    public void StartClient()
    {
        transport.SetConnectionData(inputIP.text, 18769);
        NetworkManager.Singleton.StartClient();
        cam.enabled = false;
        gameObject.SetActive(false);
    }
}
