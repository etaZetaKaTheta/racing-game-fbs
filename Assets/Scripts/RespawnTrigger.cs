using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RespawnTrigger : MonoBehaviour
{
    public static event Action TriggerEntered;

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            TriggerEntered?.Invoke();
        }
    }
}
