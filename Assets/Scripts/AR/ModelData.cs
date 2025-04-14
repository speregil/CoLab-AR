using UnityEngine;
using Unity.Netcode;

public class ModelData : NetworkBehaviour
{
    private string modelId;
    private ulong ownerId;

    private GameObject localWorkspace;

    public override void OnNetworkSpawn()
    {
        localWorkspace = GameObject.FindWithTag("workspace");
        transform.position = localWorkspace.transform.position;
    }

    public string ModelId
    {
        get { return modelId; }
        set { modelId = value; }
    }

    public ulong OwnerId
    {
        get { return ownerId; }
        set { ownerId = value; }
    }
}
