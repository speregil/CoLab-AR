using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using Niantic.Lightship.SharedAR.Colocalization;

/**
 * Behaviour that controls the process of room creation and joining of participants
 */
public class SessionManager : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] GameObject cameraAnchorPrefab;
    [SerializeField] private SharedSpaceManager sharedSpaceManager;          // References to Lightship AR Shared Space API

    private UserProfile userProfile;                                        // Profile info and functions of the current user
    private GameObject currentCameraAnchor;

    private UIDebuger uidebuger;

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        uidebuger = GetComponent<UIDebuger>();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        userProfile = new UserProfile();
        LoadProfile();
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Creates a references to a room given the room name configured in the intro scene
     */
    public void CreateRoom(string roomName, bool isHost)
    {
        var mockTrackingArgs = ISharedSpaceTrackingOptions.CreateMockTrackingOptions();
        var roomArgs = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
            roomName,
            32,
            "Room created by user as: " + roomName
        );
        sharedSpaceManager.StartSharedSpace(mockTrackingArgs, roomArgs);
        JoinRoom(isHost);
    }

    /**
     * Joins to a room as a host or a client given the status of the user identified in the intro scene
     */
    private void JoinRoom(bool isHost)
    {
        if (isHost)
            NetworkManager.Singleton.StartHost();
        else
            NetworkManager.Singleton.StartClient();
    }

    public bool SaveProfile(string username, Color userColor)
    {
        userProfile.SetUsername(username);
        userProfile.SetUserColor(userColor);
        return userProfile.SaveProfile();
    }

    public bool LoadProfile()
    {
        return userProfile.LoadProfile();
    }

    public string GetUsername()
    {
        return userProfile.GetUsername();
    }

    public Color GetUserColor()
    {
        return userProfile.GetUserColor();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void InstantiateCameraAnchorRpc(ulong clientId)
    {
        Debug.Log(clientId + ":" + IsHost);
        currentCameraAnchor = Instantiate(cameraAnchorPrefab,Vector3.zero,Quaternion.identity);
        NetworkObject anchorNetworkObject = currentCameraAnchor.GetComponent<NetworkObject>();
        anchorNetworkObject.SpawnWithOwnership(clientId);
    }
    
    /**
    * Callback thrown when a new client joins the room
    * @param clientId Unique ID create by the network manager of the client
    */
    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        //uidebuger.Log("Client connected: " + clientId);
    }
}