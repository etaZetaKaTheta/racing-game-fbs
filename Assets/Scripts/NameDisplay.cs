using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;

public class NameDisplay : NetworkBehaviour
{
    [SerializeField] private TMP_Text nameShield;

    private NetworkVariable<FixedString32Bytes> displayName = new NetworkVariable<FixedString32Bytes>();

    public ulong currentOwnerId;

    public override void OnNetworkSpawn()
    {
        CarUIManager.NameChanged += ChangeName;
        displayName.OnValueChanged += HandleNameChanged;
    }

    public override void OnNetworkDespawn()
    {
        CarUIManager.NameChanged -= ChangeName;
        displayName.OnValueChanged -= HandleNameChanged;
    }

    private void ChangeName(string newName, ulong ownerId)
    {
        currentOwnerId = ownerId;
        ChangeNameServerRpc(newName);
    }

    private void HandleNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        if (currentOwnerId == OwnerClientId) 
        {
            nameShield.text = newName.ToString();
            Debug.Log("NAME GESETZT");
            Debug.Log(newName.ToString());
            Debug.Log(OwnerClientId);
        }
    }

    [ServerRpc]
    private void ChangeNameServerRpc(string newName)
    {
        displayName.Value = newName;
    }
}
