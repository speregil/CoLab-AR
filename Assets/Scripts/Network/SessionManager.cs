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
    private bool onRoom = false;
    private bool cameraAnchorCheck = true;

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

    void Update()
    {
        Debug.Log(onRoom && !NetworkManager.IsHost);
        if (onRoom && !NetworkManager.IsHost) 
        {
            Debug.Log("Client: Checking for anchor");
            if (cameraAnchorCheck && currentCameraAnchor != null)
            {
                Debug.Log("Client: Anchor exists");
                cameraAnchorCheck = false;
            }
            else Debug.Log("Client: Anchor does not exist");
        }
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
        {
            Debug.Log("Connecting host");
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.Log("Connecting client");
            NetworkManager.Singleton.StartClient();
        }
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

    public void InstantiateCameraAnchor(ulong clientId)
    {
        Debug.Log("instantiating camera for: " + clientId);
        NetworkObject anchornetworkObject = NetworkManager.SpawnManager.InstantiateAndSpawn(cameraAnchorPrefab.GetComponent<NetworkObject>(), clientId,false, false, false,Vector3.zero,Quaternion.identity);
        currentCameraAnchor = anchornetworkObject.gameObject;
    }
    
    /**
    * Callback thrown when a new client joins the room
    * @param clientId Unique ID create by the network manager of the client
    */
    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"User connected: {clientId}");
        if(NetworkManager.IsHost)
            InstantiateCameraAnchor(clientId);
        //uidebuger.Log("Client connected: " + clientId);
    }
}