using UnityEngine;

public class ModelData : MonoBehaviour
{
    private string modelId;
    private ulong ownerId;

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
