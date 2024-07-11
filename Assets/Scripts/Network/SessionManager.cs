using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Niantic.Lightship.SharedAR.Colocalization;

public class SessionManager : MonoBehaviour
{
    private IntroManager introManager;
    private SharedSpaceManager sharedSpaceManager;

    // Start is called before the first frame update
    void Start()
    {
        introManager = GameObject.Find("UI").GetComponent<IntroManager>();
        sharedSpaceManager = GetComponent<SharedSpaceManager>();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        CreateRoom();
    }

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
        Debug.Log(introManager.IsHost());
        if (introManager.IsHost())
            NetworkManager.Singleton.StartHost();
        else
            NetworkManager.Singleton.StartClient();
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
    }
}