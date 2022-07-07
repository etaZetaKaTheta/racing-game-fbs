using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using TMPro;
using System.Text;

public class MenuUI : MonoBehaviour
{
    [SerializeField] private UnityTransport transport;
    [SerializeField] private TMP_InputField inputIP;
    [SerializeField] private TMP_InputField inputName;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject canvas;

    private static Dictionary<ulong, PlayerData> clientData;

    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;
    }

    private void OnDisable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
    }

    public void StartHost()
    {
        if (inputName.text == "")
        {
            inputName.text = "UwU";
        }

        clientData = new Dictionary<ulong, PlayerData>();
        clientData[NetworkManager.Singleton.LocalClientId] = new PlayerData(inputIP.text);

        if (inputIP.text == "")
        {
            inputIP.text = "127.0.0.1";
        }

        transport.SetConnectionData(inputIP.text, 18769);
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();
        cam.SetActive(false);
        canvas.SetActive(false);
    }

    public void StartClient()
    {
        if (inputName.text == "")
        {
            inputName.text = "UwU";
        }

        string payload = JsonUtility.ToJson(new ConnectionPayload()
        {
            playerName = inputName.text
        });

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

    private void HandleClientDisconnect(ulong id)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            clientData.Remove(id);
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        string payload = Encoding.ASCII.GetString(request.Payload);
        var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

        clientData[request.ClientNetworkId] = new PlayerData(connectionPayload.playerName);

        response.Approved = true;
        response.CreatePlayerObject = true;
        response.PlayerPrefabHash = null;
        response.Pending = false;
    }

    public static PlayerData? GetPlayerData(ulong clientId)
    {
        if (clientData.TryGetValue(clientId, out PlayerData playerData))
        {
            return playerData;
        }

        return null;
    }
}
