using Niantic.Lightship.SharedAR.Colocalization;
using Niantic.Lightship.SharedAR.Rooms;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/**
 * Behaviour that controls the configuration actions for creating or joining a room
 */
public class IntroManager : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] TMP_InputField createNameInputField;               // Reference to the input field for the name of a room to create
    [SerializeField] TMP_InputField usernameInputField;                 // Reference to the input field for the username of the current user's profile
    [SerializeField] TMP_Text responseMessageTxt;                       // Reference to the response message text in the profile menu 
    [SerializeField] TMP_Dropdown joinNameDropdown;                     // Reference to the input field for the name of a room to join to
    [SerializeField] TMP_Dropdown colorPickDropdown;                    // Reference to the dropdown field for the color of the user
    [SerializeField] GameObject roomAnchorPrefab;                       // Prefab for the room anchor object

    [SerializeField] private SharedSpaceManager sharedSpaceManager;     // References to Lightship AR Shared Space API
    [SerializeField] private Texture2D trackingImage;                   // Reference to the image used for tracking
    UIManager uiManager;                                                // Reference to the UIManager component

    private string roomName;                                            // Name of the room to create or join to
    private string username;                                            // Name of the user's profile
    private Color userColor;                                            // Color of the user's profile
    private bool isHost;                                                // Flag that determines if the user is creating or joining a room

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    private void Start()
    {
        uiManager = GetComponent<UIManager>();
        sharedSpaceManager.sharedSpaceManagerStateChanged += OnColocalizationTrackingStateChanged;
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Determines if the user is a host or not
     * @return True if the user is a host, False otherwise
     * */
    public bool IsHost()
    {
        return isHost;
    }

    /**
     * Returns the current name that is going to be used to create or join a room
     * @returns string The name of the current room
     */
    public string GetRoomName()
    {
        return roomName;
    }

    /**
     * Changes the current name that is going to be used to create or join a room
     * @param roomName New current name of the room
     */
    public void RoomNameEdit(string roomName)
    {
        this.roomName = roomName;
    }

    /**
     * Changes the current username used by the user and that is going to be saved in the local profile
     * @param username Current usernme provided by the user
     */
    public void usernameEdit(string username)
    {
        this.username = username;
    }

    /**
     * Callback launched when the dropdown field of the Join Room menu changes value
     * @param value index of the new value
     */
    public void OnDropdownChange(int value)
    {
        roomName = joinNameDropdown.options[value].text;
    }

    /**
     * Callback launched when the dropdown field of the color pick menu changes value
     * @param value index of the new value
     */
    public void OnColorPick(int value)
    {
        switch (value)
        {
            case 0: // Red
                userColor = new Color(255, 0, 0);
                break;
            case 1: // Blue
                userColor = new Color(0, 0, 255);
                break;
            case 2: // Lime
                userColor = new Color(0, 255, 0);
                break;
            case 3: // Yellow
                userColor = new Color(255, 255, 0);
                break;
            case 4: // Purple
                userColor = new Color(128, 0, 128);
                break;
            case 5: // Green
                userColor = new Color(0, 128, 0);
                break;
            case 6: // Brown
                userColor = new Color(165, 42, 42);
                break;
            case 7: // Dark Blue
                userColor = new Color(0, 255, 255);
                break;
            case 8: // Orange
                userColor = new Color(255, 165, 0);
                break;
            case 9: // Pink
                userColor = new Color(255, 0, 255);
                break;
            case 10: // Black
                userColor = new Color(0, 0, 0);
                break;
            default:
                userColor = new Color(128, 128, 128);
                break;
        }
    }

    /**
     * Cleans the UI for room creation, stablishes the user as a host and asks the UIManager to proceed with the
     * creation of a room
     */
    public void AcceptCreateRoom()
    {
        createNameInputField.text = "";
        isHost = true;
        uiManager.AcceptCreateRoom();
        ConfigureSharedSpace();
    }

    /**
     * Initialize the room list everytime the Join Room menu is opened
     */
    public void InitializeJoinRoom()
    {
        joinNameDropdown.ClearOptions();
        List<IRoom> roomList = new List<IRoom>();
        List<string> roomNamesList = new List<string>();
        RoomManagementService.GetAllRooms(out roomList);
        foreach (IRoom room in roomList)
        {
            roomNamesList.Add(room.RoomParams.Name);
        }
        joinNameDropdown.AddOptions(roomNamesList);
        roomName = joinNameDropdown.options[joinNameDropdown.value].text;
    }

    /**
     * Initialize the profile menu with the username given by parameter as default option in the interface
     * @param currentUsername Default username option to show in the interface
     */
    public void InitializeProfileMenu(string currentUsername, int currentUserColor)
    {
        usernameInputField.text = currentUsername;
        colorPickDropdown.value = currentUserColor;
        responseMessageTxt.text = "";
    }

    /**
     * Cleans the UI for joining a room, stablishes the user as a client and ask the UIManager to proceed with the
     * room join
     */
    public void AcceptJoinRoom()
    {
        isHost = false;
        uiManager.AcceptJoinRoom();
        ConfigureSharedSpace();
    }

    /**
     * Cleans the UI for room creation and asks the UIManager to move back to the intro menu
     */
    public void CancelCreateRoom()
    {
        roomName = "";
        createNameInputField.text = "";
        uiManager.CancelCreateRoom();
    }

    /**
     * Cleans the UI for joining a room and asks the UIManager to move back to the intro menu
     */
    public void CancelJoinRoom()
    {
        uiManager.CancelJoinRoom();
    }

    /**
     * Prompts the interface manager to save the user prfile in the local machine, and displays the
     * result response of the operation
     */
    public void SaveProfile()
    {
        responseMessageTxt.text = "";
        string msg = uiManager.SaveProfile(username, userColor, colorPickDropdown.value);
        responseMessageTxt.text = msg;
    }

    /**
     * Creates a references to a room given the room name configured in the intro scene
     */
    private void ConfigureSharedSpace()
    {
        Debug.Log("Configuring Shared Space");
        var imageTrackingArgs = ISharedSpaceTrackingOptions.CreateImageTrackingOptions(trackingImage,0.09f);
        var roomArgs = ISharedSpaceRoomOptions.CreateLightshipRoomOptions(
            roomName,
            32,
            "Room created by user as: " + roomName
        );
        sharedSpaceManager.StartSharedSpace(imageTrackingArgs, roomArgs);
    }

    /**
     * Joins a room as a host or a client given the status of the user identified in the intro scene
     */
    private void StartSharedSpace()
    {
        if (isHost)
        {
            Debug.Log("Connecting host");
            if(NetworkManager.Singleton.StartHost())
            {
                GameObject roomAnchorInstance = Instantiate(roomAnchorPrefab);
                roomAnchorInstance.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.ServerClientId);
            }
        }
        else
        {
            Debug.Log("Connecting client");
            NetworkManager.Singleton.StartClient();
        }
    }

    /**
     * Callback function that is launched when the tracking state of the colocalization system changes
     * @param args Event arguments that contain the new tracking state
     */
    private void OnColocalizationTrackingStateChanged(SharedSpaceManager.SharedSpaceManagerStateChangeEventArgs args)
    {
        Debug.Log("Tracking State changed");
        if (args.Tracking)
        {
            Debug.Log("Tracking Image");
            uiManager.ChangeTrackingState(MainMenuManager.TRACKING_OK_STATE);
            StartSharedSpace();
        }
    }

    /**
     * Asks the RoomManagementService to delete all the rooms storaged in the SharedAR service
     */
    public void PurgeRooms()
    {
        List<IRoom> roomList = new List<IRoom>();
        RoomManagementService.GetAllRooms(out roomList);
        foreach (IRoom room in roomList)
        {
            RoomManagementService.DeleteRoom(room.RoomParams.RoomID);
        }
        Debug.Log("Rooms Purged, my Lord.");
    }
}