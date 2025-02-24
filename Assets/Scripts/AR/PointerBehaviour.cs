using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PointerBehaviour : NetworkBehaviour
{
    [SerializeField] private float lifespan;
    private NetworkObject no;
    private float aliveFor = 0.0f;

    public override void OnNetworkSpawn()
    {
        no = GetComponent<NetworkObject>();
        aliveFor = 0.0f;
    }

    private void Update()
    {
        if (IsOwner) { 
            aliveFor += Time.deltaTime;
            Debug.Log(aliveFor);
            if (aliveFor >= lifespan)
            {
                DespawnPointerRpc();
            }
        }
    }

    [Rpc(SendTo.Server)]
    private void DespawnPointerRpc()
    {
        no.Despawn(true);
    }
}