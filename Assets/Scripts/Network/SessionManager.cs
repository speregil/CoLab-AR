using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.VisualScripting;

/**
 * Behaviour that controls the network functions of a participant who joined a room
 */
public class SessionManager : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private GameObject PointerPrefab;

    private Dictionary<string, Color> participants = new Dictionary<string, Color>();
    private GameObject selectedParticipant;
    private UserConfiguration userConfig;
    private MainMenuManager mainMenu;

    private int defaultParticipantCounter = 1;

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    public override void OnNetworkSpawn()
    {
        userConfig = GameObject.Find("OfflineConfig").GetComponent<UserConfiguration>();
        GameObject mainMenuObject = GameObject.Find("UI").transform.Find("MainMenu").gameObject;
        mainMenu = mainMenuObject.GetComponent<MainMenuManager>();
        RegisterNewParticipantRpc(userConfig.GetProfileStruct(), NetworkManager.Singleton.LocalClientId);

        if (IsOwner) mainMenu.SetSessionManager(this);
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------
    
    public MainMenuManager GetMainMenuReference()
    {
        return mainMenu;
    }

    public Dictionary<string,Color> GetParticipantsList()
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
        if(!IsOwner) return;

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
        Vector3 position = selectedParticipant.transform.position;
        position.y = position.y + 0.5f;
        mainMenu.CloseParticipantOptions();
        SpawnPingRpc(position, NetworkManager.Singleton.LocalClientId);
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

    [Rpc(SendTo.Server)]
    private void SpawnPingRpc(Vector3 position, ulong clientID)
    {
        if (IsHost) { 
            GameObject pointerInstance = Instantiate(PointerPrefab, position, Quaternion.identity);
            NetworkObject pointerNetworkObject = pointerInstance.GetComponent<NetworkObject>();
            pointerNetworkObject.SpawnWithOwnership(clientID);
        }
    }
}