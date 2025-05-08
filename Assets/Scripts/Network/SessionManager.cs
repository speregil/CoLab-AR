using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Niantic.Lightship.SharedAR.Colocalization;

/**
 * Behaviour that controls the network functions of a participant who joined a room
 */
public class SessionManager : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private GameObject PointerPrefab;
    [SerializeField] GameObject roomAnchorPrefab;
    // Prefab for the room anchor object

    private Dictionary<string, Color> participants = new Dictionary<string, Color>();
    private GameObject selectedParticipant;
    private UserConfiguration userConfig;
    private MainMenuManager mainMenu;
    private WorkspaceConfig workspaceConfig;

    private int defaultParticipantCounter = 1;

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

    public MainMenuManager GetMainMenuReference()
    {
        return mainMenu;
    }

    public Dictionary<string, Color> GetParticipantsList()
    {
        return participants;
    }

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

    public void SetGazeActive(bool active)
    {
        if (!IsOwner) return;

        if (selectedParticipant != null)
        {
            GameObject gaze = selectedParticipant.transform.Find("Gaze").gameObject;
            gaze.SetActive(active);
        }
    }

    public void SetNameplateActive(bool active)
    {
        if (!IsOwner) return;

        if (selectedParticipant != null)
        {
            GameObject gaze = selectedParticipant.transform.Find("Nameplate").gameObject;
            gaze.SetActive(active);
        }
    }

    public void PingParticipant()
    {
        SessionManager selectedParticipantManager = selectedParticipant.GetComponentInParent<SessionManager>();
        string participantUsername = GetUsernameById(selectedParticipantManager.OwnerClientId);
        mainMenu.CloseParticipantOptions();
        ClientSpawnParticipantPingRpc(participantUsername);
    }

    public void SelectParticipant(GameObject participant)
    {
        if (!IsOwner) return;

        selectedParticipant = participant;
    }

    public void UnselectParticipant()
    {
        if (!IsOwner) return;

        selectedParticipant = null;
    }

    public UserConfiguration GetParticipantConfiguration()
    {
        return userConfig;
    }

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

    public void LeaveRoom()
    {
        Debug.Log("Leaving room");
        ParticipantLeavesRoomRpc(userConfig.GetProfileStruct(), NetworkManager.Singleton.LocalClientId);
    }

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
                pointerInstance.transform.position = new Vector3(model.transform.position.x, model.transform.position.y + 0.5f, model.transform.position.z);
            }
        }
    }

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
            networkModel.SpawnWithOwnership(ownerId);
            model.name = "UserModel" + modelId;
            model.GetComponent<ModelData>().UpdateModelID(modelId);
        }
    }

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
}