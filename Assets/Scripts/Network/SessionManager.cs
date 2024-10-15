using System.Collections;
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

    [SerializeField] private GameObject mainBody;
    [SerializeField] private GameObject gaze;

    private List<string> participants = new List<string>();
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
        RegisterNewParticipantRpc(userConfig.GetProfileStruct());

        if(IsOwner) {
            mainMenu.SetSessionManager(this);
            gaze.SetActive(false);
            ApplyAnchorColor();
        }
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    public List<string> GetParticipantsList()
    {
        return participants;
    }

    public void ApplyAnchorColor()
    {
        Material mainMaterial = mainBody.GetComponent<Renderer>().material;
        Color participantColor = userConfig.GetUserColor();
        Debug.Log("r: " + participantColor.r + " g: " + participantColor.g + " b: " + participantColor.b);
        mainMaterial.SetColor("_Color", participantColor);
        TrasmitAnchorColorChangeRpc(userConfig.GetProfileStruct(), OwnerClientId);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RegisterNewParticipantRpc(UserConfiguration.ProfileStruct newUserProfile)
    {
        string newParticipantName = newUserProfile.username.ToString();

        if(newParticipantName != null && newParticipantName != "") { 
            participants.Add(newParticipantName);
        }
        else
        {
            newParticipantName = "New Participant" + defaultParticipantCounter;
            participants.Add(newParticipantName);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TrasmitAnchorColorChangeRpc(UserConfiguration.ProfileStruct profile, ulong ownerID)
    {
        Debug.Log("Rpc: " + profile.r +":" + profile.g + ":" + profile.b);
        Material mainMaterial = mainBody.GetComponent<Renderer>().material;
        Color participantColor = new Color(profile.r, profile.g, profile.b);
        mainMaterial.SetColor("_Color", participantColor);
    }

    [Rpc(SendTo.Server)]
    public void PetitionCurrentParticipantsListRpc(ulong clientID)
    {
        string msg = "" + participants.Count + " ";
        foreach(string participant in participants) {
            msg += participant + " ";
        }

        UpdateParticipantsListRpc(msg.TrimEnd(), clientID);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateParticipantsListRpc(string currentList, ulong clientID)
    {
        if (IsHost) return;

        string[] listParams = currentList.Split(" ");
        int count = int.Parse(listParams[0]);
        int i = 1;
        while (i < count) {
            participants.Add(listParams[i]);
            count++;
        }
    }
}