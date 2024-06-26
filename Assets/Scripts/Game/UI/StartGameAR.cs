using Niantic.Lightship.SharedAR.Colocalization;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StartGameAR : MonoBehaviour
{
    [SerializeField] private SharedSpaceManager spaceManager;

    [SerializeField] private Texture2D targetImage;
    [SerializeField] private float targetImageSize;

    [SerializeField] private Button startGameButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    private const int MAX_CLIENTS = 2;
    private string roomName = "TestRoom";
    private bool isHost;

    public static event Action OnStartSharedSpaceHost;
    public static event Action OnJoinSharedSpaceClient;
    public static event Action OnStartSharedSpace;
    public static event Action OnStartGame;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        spaceManager.sharedSpaceManagerStateChanged += SharedSpaceManagerOnSharedSpaceManagerStateChanged;

        startGameButton.onClick.AddListener(StartGame);
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinGameClient);

        startGameButton.interactable = false;
    }

    private void SharedSpaceManagerOnSharedSpaceManagerStateChanged(SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs obj)
    {
        if (obj.Tracking)
        {
            startGameButton.interactable = true;
            createRoomButton.interactable = false;
            joinRoomButton.interactable = false;
        }
    }

    void StartGame()
    {
        OnStartGame?.Invoke();

        if(isHost)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    void CreateRoom()
    {
        isHost = true;
        OnStartSharedSpaceHost?.Invoke();
        StartSharedSpace();
    }

    void JoinGameClient()
    {
        isHost = false;
        OnJoinSharedSpaceClient?.Invoke();
        StartSharedSpace();
    }

    void StartSharedSpace()
    {
        OnStartSharedSpace?.Invoke();

        if(spaceManager.GetColocalizationType() == SharedSpaceManager.ColocalizationType.MockColocalization)
        {
            var mockTrackingArgs = ISharedSpaceTrackingOptions.CreateMockTrackingOptions();
            var roomArgs = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
                roomName,
                capacity: MAX_CLIENTS,
                description: "SharedARTestMock"
            );

            spaceManager.StartSharedSpace(mockTrackingArgs, roomArgs);
            return;
        }

        if (spaceManager.GetColocalizationType() == SharedSpaceManager.ColocalizationType.ImageTrackingColocalization)
        {
            var imageTrackingArgs = ISharedSpaceTrackingOptions.CreateImageTrackingOptions(targetImage, targetImageSize);
            var roomArgs = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
                roomName,
                capacity: MAX_CLIENTS,
                description: "SharedARTest"
            );

            spaceManager.StartSharedSpace(imageTrackingArgs, roomArgs);
            return;
        }
    }

// Start is called before the first frame update
void Start()
    {
        
    }
}
