using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Text;
using System;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TMP_InputField inputIP;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject canvas;

    private string lobbyPassword = "";

    public void StartHost()
    {
        if (inputIP.text == "")
        {
            inputIP.text = "127.0.0.1";
        }

        transport.SetConnectionData(inputIP.text, 18769);
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        cam.SetActive(false);
        canvas.SetActive(false);
    }

    public void StartClient()
    {
        string payload = inputPassword.text;

        if (inputIP.text == "")
        {
            inputIP.text = "127.0.0.1";
        }

        transport.SetConnectionData(inputIP.text, 18769);

        byte[] payloadBytes = Encoding.ASCII.GetBytes(payload);
        
        NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

        NetworkManager.Singleton.StartClient();
        cam.SetActive(false);
        canvas.SetActive(false);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    { 
        string payload = Encoding.ASCII.GetString(request.Payload);
        
        response.Approved = payload == lobbyPassword;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;
        response.Pending = false;
    }
}
