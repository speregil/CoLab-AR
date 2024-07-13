using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Niantic.Lightship.SharedAR.Colocalization;

/**
 * Behaviour that controls the process of room creation and joining of participants
 */
public class SessionManager : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    private IntroManager introManager;                      // Reference to the Intro Scene actions manager
    private SharedSpaceManager sharedSpaceManager;          // References to Lightship AR Shared Space API

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        introManager = GameObject.Find("UI").GetComponent<IntroManager>();
        sharedSpaceManager = GetComponent<SharedSpaceManager>();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        CreateRoom();
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Creates a references to a room given the room name configured in the intro scene
     */
    private void CreateRoom()
    {
        string roomName = introManager.GetRoomName();
        var mockTrackingArgs = ISharedSpaceTrackingOptions.CreateMockTrackingOptions();
        var roomArgs = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
            roomName,
            32,
            "Room created by user as: " + roomName
        );
        sharedSpaceManager.StartSharedSpace(mockTrackingArgs, roomArgs);
        JoinRoom();
    }

    /**
     * Joins to a room as a host or a client given the status of the user identified in the intro scene
     */
    private void JoinRoom()
    {
        if (introManager.IsHost())
            NetworkManager.Singleton.StartHost();
        else
            NetworkManager.Singleton.StartClient();
    }

    /**
     * Callback thrown when a new client joins the room
     * @param clientId Unique ID create by the network manager of the client
     */
    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
    }
}