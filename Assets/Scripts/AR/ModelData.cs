using UnityEngine;
using Unity.Netcode;


public class ModelData : NetworkBehaviour
{
   private NetworkVariable<int> modelId = new NetworkVariable<int>();
    
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
}