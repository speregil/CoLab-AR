using Unity.Netcode;
using UnityEngine;

public class RoomAnchorBehaviour : NetworkBehaviour
{
    void OnTransformParentChanged()
    {
        //transform.position = transform.parent.position;
        //Camera.main.transform.position = transform.position;
    }
}