using UnityEngine;
using Unity.Netcode;


public class ModelData : NetworkBehaviour
{
   private NetworkVariable<int> modelId = new NetworkVariable<int>();
   private NetworkVariable<bool> pingOn = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        GameObject localWorkspace = GameObject.FindGameObjectWithTag("workspace");
        Vector3 spawnPosition = localWorkspace.transform.position;
        transform.position = spawnPosition;
    }

    public void UpdateModelID(int id)
    {
        if (IsServer)
        {
            modelId.Value = id;
        }
    }

    public int GetModelID()
    {
        return modelId.Value;
    }

    public bool IsPingOn()
    {
        return pingOn.Value;
    }

    public void SetPingOn(bool ping)
    {
        if (IsServer)
        {
            pingOn.Value = ping;
        }
    }

    [Rpc(SendTo.Server)]
    public void ChangePingStatusRpc(bool status)
    {    
        SetPingOn(status);
    }
}