using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class DisableOnConnect : NetworkBehaviour
{
    [SerializeField] private InputManager input;
    [SerializeField] private CarController controller;
    [SerializeField] private GameObject carCam;
    [SerializeField] private GameObject carUI;
    [SerializeField] private AudioSource[] audioSources;
    [SerializeField] private TMP_Text nameShield;

    private NetworkVariable<FixedString64Bytes> displayName = new NetworkVariable<FixedString64Bytes>();

    private Vector3 startPosition;
    private Rigidbody rb;

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer) 
        {
            TryGetComponent(out rb);
            startPosition = transform.position;
            TeleportWithRandomOffset();
            RespawnTrigger.TriggerEntered += TeleportWithRandomOffset;
            InputManager.RestartButtonPressed += TeleportWithRandomOffset;
            displayName.OnValueChanged += HandleNameChanged;
            GameObject buffer = Instantiate(carCam);
            buffer.GetComponent<CameraFollow>().target = transform;
            return; 
        }

        if (IsServer)
        {
            PlayerData? playerData = MenuUI.GetPlayerData(OwnerClientId);
            if (playerData.HasValue)
            {
                displayName.Value = playerData.Value.PlayerName;
            }
        }

        Destroy(input);
        Destroy(controller);
        Destroy(carUI);
        foreach (AudioSource src in audioSources)
        {
            Destroy(src);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsLocalPlayer)
        {
            RespawnTrigger.TriggerEntered -= TeleportWithRandomOffset;
            InputManager.RestartButtonPressed -= TeleportWithRandomOffset;
            displayName.OnValueChanged -= HandleNameChanged;
        }
    }

    private void TeleportWithRandomOffset()
    {
        rb.velocity = Vector3.zero;
        Vector3 ranPos = new Vector3(startPosition.x + Random.Range(1, 10), startPosition.y, startPosition.z + Random.Range(1, 10));
        transform.position = ranPos;
    }

    private void HandleNameChanged(FixedString64Bytes oldName, FixedString64Bytes newName)
    {
        nameShield.text = newName.ToString();
    }
}
