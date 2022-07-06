using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private UnityTransport transport;
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private TMP_InputField inputIP;
    [SerializeField] private GameObject cam;
    public void StartHost()
    {
        transport.SetConnectionData(inputIP.text, 18769);
        networkManager.StartHost();
        cam.SetActive(false);
        gameObject.SetActive(false);
    }

    public void StartClient()
    {
        transport.SetConnectionData(inputIP.text, 18769);
        networkManager.StartClient();
        cam.SetActive(false);
        gameObject.SetActive(false);
    }
}
