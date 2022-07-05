using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetworkManagerOverrides : NetworkManager
{
    [SerializeField] private GameObject menuUI;
    private void Start()
    {
        GameObject netMenuUI = Instantiate(menuUI);
        netMenuUI.GetComponent<NetworkObject>().Spawn();
    }
}
