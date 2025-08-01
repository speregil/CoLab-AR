using System.Collections.Generic;
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

    [SerializeField] private GameObject PointerPrefab;                                  // Prefab for the pointer object
    [SerializeField] private GameObject annotationPrefab;                               // Prefab for the annotation object  
    [SerializeField] private GameObject roomAnchorPrefab;                               // Prefab for the room anchor object

    private Dictionary<string, Color> participants = new Dictionary<string, Color>();   // Dictionary with the information of the current connected participants
    private GameObject selectedParticipant;                                             // Reference to the current selected participant for interactions
    private UserConfiguration userConfig;                                               // Reference of the current user's configuration data
    private MainMenuManager mainMenu;                                                   // Reference to the main menu manager
    private WorkspaceConfig workspaceConfig;                                            // Reference to the workspace configuration manager

    private int defaultParticipantCounter = 1;                                          // Counter for the default participant names in case no username is provided

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        userConfig = GameObject.Find("OfflineConfig").GetComponent<UserConfiguration>();
        GameObject mainMenuObject = GameObject.Find("UI").transform.Find("MainMenu").gameObject;
        mainMenu = mainMenuObject.GetComponent<MainMenuManager>();
        workspaceConfig = GetComponent<WorkspaceConfig>();
        RegisterNewParticipantRpc(userConfig.GetProfileStruct(), NetworkManager.Singleton.LocalClientId);

        if (IsOwner)
        {
            mainMenu.SetSessionManager(this);
            Instantiate(roomAnchorPrefab);
        }
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Returns the reference to the main menu manager
     * @return MainMenuManager The main menu manager component of the app
    */
    public MainMenuManager GetMainMenuReference()
    {
        return mainMenu;
    }

    /**
     * Returns the list of participants currently connected to the room
     * @return Dictionary<string, Color> A dictionary with the usernames as keys and the participant colors as values
     */
    public Dictionary<string, Color> GetParticipantsList()
    {
        return participants;
    }

    /**
     * Updates the username, ID and anchor color of all the local instances of the user anchors
    */
    private void LocalUpdateAnchors()
    {
        GameObject[] localParticipants = GameObject.FindGameObjectsWithTag("participant");

        foreach (GameObject local in localParticipants)
        {
            SessionManager localManager = local.GetComponent<SessionManager>();
            CameraAnchor localAnchor = local.GetComponent<CameraAnchor>();
            ulong localOwnerId = localManager.OwnerClientId;

            string username = "";
            foreach (string participant in participants.Keys)
            {
                string[] data = participant.Split(":");
                ulong key = ulong.Parse(data[1]);
                if (localOwnerId == key) username = participant;
            }

            if (username != "")
            {
                Color participantColor = participants[username];
                localAnchor.ApplyAnchorColor(participantColor);
                localAnchor.UpdateAnchorNameplate(username);
            }
        }
    }

    /**
     * Activates the local anchor's gaze
     * @param active True if the gaze should be activated, false otherwise
     */
    public void SetGazeActive(bool active)
    {
        if (!IsOwner) return;

        if (selectedParticipant != null)
        {
            GameObject gaze = selectedParticipant.transform.Find("Gaze").gameObject;
            gaze.SetActive(active);
        }
    }

    /**
     * Activates the local anchor's nameplate
     * @param active True if the nameplate should be activated, false otherwise
     */
    public void SetNameplateActive(bool active)
    {
        if (!IsOwner) return;

        if (selectedParticipant != null)
        {
            GameObject gaze = selectedParticipant.transform.Find("Nameplate").gameObject;
            gaze.SetActive(active);
        }
    }

    /**
     * Locally shows the ping visualization of the selected participant
     */
    public void PingParticipant()
    {
        SessionManager selectedParticipantManager = selectedParticipant.GetComponentInParent<SessionManager>();
        string participantUsername = GetUsernameById(selectedParticipantManager.OwnerClientId);
        mainMenu.CloseParticipantOptions();
        ClientSpawnParticipantPingRpc(participantUsername);
    }

    /**
     * Updates the local anchor that is currently selected
     * @param participant The GameObject of the participant to be selected
     */
    public void SelectParticipant(GameObject participant)
    {
        if (!IsOwner) return;

        selectedParticipant = participant;
    }

    /**
     * Cleans the participant selection
     */
    public void UnselectParticipant()
    {
        if (!IsOwner) return;

        selectedParticipant = null;
    }

    /**
     * Returns the user configuration of the local user
     * @return UserConfiguration The user configuration behaviour of the local user
     */
    public UserConfiguration GetParticipantConfiguration()
    {
        return userConfig;
    }

    /**
     * Returns the username of a participant given its ID
     * @param id The session ID of the participant assigned at the moment of joining the room
     * @return string The username of the participant
     */
    public string GetUsernameById(ulong id)
    {
        string username = "";

        foreach (string participant in participants.Keys)
        {
            string[] data = participant.Split(":");
            ulong key = ulong.Parse(data[1]);
            if (id == key) username = data[0];
        }

        return username;
    }

    /**
     * Signals to the network that the local user is leaving the room
     */
    public void LeaveRoom()
    {
        Debug.Log("Leaving room");
        ParticipantLeavesRoomRpc(userConfig.GetProfileStruct(), NetworkManager.Singleton.LocalClientId);
    }

    //------------------------------------------------------------------------------------------------------
    // Network Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Remote procedure that signals the registration of a new participant in the room and the update og the
     * local participant list
     * @param newUserProfile The profile data of the new participant
     * @param clientID The session ID of the new participant assigned at the moment of joining the room
     */
    [Rpc(SendTo.ClientsAndHost)]
    private void RegisterNewParticipantRpc(UserConfiguration.ProfileStruct newUserProfile, ulong clientID)
    {
        string newParticipantName = newUserProfile.username.ToString() + ":" + clientID;
        Color participantColor = new Color(newUserProfile.r, newUserProfile.g, newUserProfile.b);

        if (newParticipantName != null && newParticipantName != "") {
            participants[newParticipantName] = participantColor;
        }
        else
        {
            newParticipantName = "New Participant " + defaultParticipantCounter;
            participants[newParticipantName] = participantColor;
            defaultParticipantCounter++;
        }

        if (IsOwner)
            LocalUpdateAnchors();
    }

    /**
     * Remote procedure that signals the removal of a participant from the room and the update of the local participant list
     * @param userProfile The profile data of the participant that is leaving
     * @param clientID The session ID of the participant that is leaving
     */
    [Rpc(SendTo.Everyone)]
    private void ParticipantLeavesRoomRpc(UserConfiguration.ProfileStruct userProfile, ulong clientID)
    {
        string participantName = userProfile.username.ToString() + ":" + clientID;
        string removalKey = "";
        foreach (string name in participants.Keys)
        {
            if (name == participantName)
            {
                removalKey = name;
                break;
            }
        }
        participants.Remove(removalKey);

        if (IsOwner)
            Debug.Log("Participant Shutdown");
    }

    /**
     * Remote procedure that spawns a ping pointer object for a local user anchor
     * @param username The username of the participant to spawn the ping pointer for
     */
    [Rpc(SendTo.Everyone)]
    private void ClientSpawnParticipantPingRpc(string username)
    {
        GameObject[] participants = GameObject.FindGameObjectsWithTag("anchor");
        foreach (GameObject participant in participants)
        {
            SessionManager participantManager = participant.GetComponentInParent<SessionManager>();
            string participantUsername = GetUsernameById(participantManager.OwnerClientId);
            if (participantUsername == username)
            {
                GameObject pointerInstance = Instantiate(PointerPrefab, participant.transform.position, Quaternion.identity);
                pointerInstance.transform.SetParent(participant.transform);
                pointerInstance.transform.position = new Vector3(participant.transform.position.x, participant.transform.position.y + 0.5f, participant.transform.position.z);
            }
        }
    }

    /**
     * Remote procedure to test if it is possible to ping a model in the room
     * @param modelID The unique ID of the model to ping
     */
    [Rpc(SendTo.Server)]
    public void AskModelPingRpc(int modelID)
    {
        GameObject[] models = GameObject.FindGameObjectsWithTag("model");
        foreach (GameObject model in models)
        {
            ModelData data = model.GetComponent<ModelData>();
            int dataID = data.GetModelID();
            if (dataID == modelID)
            {
                data.SetPingOn(true);
                ClientSpawnModelPingRpc(modelID);
            }
        }
    }

    /**
     * Remote procedure that spawns a ping pointer object for a local model
     * @param modelID The unique ID of the model to ping
     */
    [Rpc(SendTo.Everyone)]
    public void ClientSpawnModelPingRpc(int modelID)
    {
        GameObject[] models = GameObject.FindGameObjectsWithTag("model");
        foreach (GameObject model in models)
        {
            ModelData data = model.GetComponent<ModelData>();
            int dataID = data.GetModelID();
            if (dataID == modelID)
            {
                GameObject pointerInstance = Instantiate(PointerPrefab, model.transform.position, Quaternion.identity);
                pointerInstance.GetComponent<PointerBehaviour>().SetPointedModel(data);
                pointerInstance.transform.SetParent(model.transform);
                pointerInstance.transform.position = new Vector3(model.transform.position.x, model.transform.position.y + 0.25f, model.transform.position.z);
            }
        }
    }

    /**
     * Remote procedure to add a local instance of a new model in the room
     * @param modelType The type of the model to be added, defined in the MainMenuManager
     * @param position The position where the model should be instantiated, in relation to the shared origin point
     * @param ownerId The session ID of the participant that is adding the model, used to assign ownership
     */
    [Rpc(SendTo.Server)]
    public void AddModelRpc(int modelType, Vector3 position, ulong ownerId)
    {
        GameObject modelPrefab = mainMenu.GetToAddModel(modelType);
        GameObject localWorkspace = workspaceConfig.GetCurrentWorkspace();

        if (modelPrefab != null)
        {
            GameObject model = Instantiate(modelPrefab);
            int modelId = model.GetInstanceID();
            NetworkObject networkModel = model.GetComponent<NetworkObject>();
            //networkModel.SpawnWithOwnership(ownerId);
            networkModel.Spawn();
            model.name = "UserModel" + modelId;
            model.GetComponent<ModelData>().UpdateModelID(modelId);
        }
    }

    /**
     * Remote procedure to delete all local instances of a model in the room
     * @param modelID The unique ID of the model to be deleted
     */
    [Rpc(SendTo.Server)]
    public void DeleteModelRpc(int modelID)
    {
        GameObject model = GameObject.Find("UserModel" + modelID);
        if (model != null)
        {
            Debug.Log("Deleting model: " + modelID);
            ModelData data = model.GetComponent<ModelData>();
            NetworkObject networkModel = model.GetComponent<NetworkObject>();
            networkModel.Despawn(true);
        }
        else
        {
            Debug.Log("Model not found: " + modelID);
        }
    }

    /**
     * Remote procedure to add a local instance of an annotation in the room
     * @param annotation The text of the annotation to be added
     * @param position The position where the annotation should be instantiated, in relation to the shared origin point
     */
    [Rpc(SendTo.Server)]
    public void AddAnnotationRpc(string annotation, Vector3 position)
    {
        GameObject instance = Instantiate(annotationPrefab);
        instance.transform.position = position;
        NetworkObject no = instance.GetComponent<NetworkObject>();
        no.Spawn();
        AnnotationData data = instance.GetComponent<AnnotationData>();
        data.UpdateAnnotation(annotation);
    }
}