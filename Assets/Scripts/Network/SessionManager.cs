using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using Niantic.Lightship.SharedAR.Colocalization;
using static UnityEngine.CullingGroup;

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
    private GameObject currentCameraAnchor = null;
    public NetworkVariable<string> lastParticipant = new NetworkVariable<string>();

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
     * Joins a room as a host or a client given the status of the user identified in the intro scene
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
        RegisterParticipantRpc(userProfile.GetUsername());
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

    public override void OnNetworkSpawn()
    {
        lastParticipant.OnValueChanged += OnLastParticipantChanged;
    }

    public void OnLastParticipantChanged(string previous, string current)
    {
        Debug.Log(current);
    }

    [Rpc(SendTo.Server), GenerateSerializationForTypeAttribute(typeof(System.String))]

    public void RegisterParticipantRpc(string username)
    {
        lastParticipant.Value = userProfile.GetUsername();
    }

    /**
    * Callback thrown when a new client joins the room
    * @param clientId Unique ID create by the network manager of the client
    */
    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"User ID: {clientId}, username: {userProfile.GetUsername()}");
        if(NetworkManager.IsHost)
            InstantiateCameraAnchor(clientId);
        //uidebuger.Log("Client connected: " + clientId);
    }
}