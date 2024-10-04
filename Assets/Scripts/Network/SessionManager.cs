using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;

/**
 * Behaviour that controls the network functions of a participant who joined a room
 */
public class SessionManager : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] GameObject cameraAnchorPrefab;

    private GameObject currentCameraAnchor = null;

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        InstantiateCameraAnchorRPC(NetworkManager.Singleton.LocalClientId);   
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    [Rpc(SendTo.Server)]
    public void InstantiateCameraAnchorRPC(ulong clientId)
    {
        if (IsServer)
        {
            Debug.Log("instantiating camera for: " + clientId);
            NetworkObject anchornetworkObject = NetworkManager.SpawnManager.InstantiateAndSpawn(cameraAnchorPrefab.GetComponent<NetworkObject>(), clientId, false, false, false, Vector3.zero, Quaternion.identity);
            currentCameraAnchor = anchornetworkObject.gameObject;
        }
    }
}