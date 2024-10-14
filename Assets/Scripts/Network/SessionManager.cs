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
    private Material mainMaterial;

    private int defaultParticipantCounter = 1;

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    public override void OnNetworkSpawn()
    {
        userConfig = GameObject.Find("OfflineConfig").GetComponent<UserConfiguration>();
        GameObject mainMenuObject = GameObject.Find("UI").transform.Find("MainMenu").gameObject;
        mainMenu = mainMenuObject.GetComponent<MainMenuManager>();
        mainMaterial = mainBody.GetComponent<Renderer>().material;
        mainMaterial.SetColor("_Color", userConfig.GetUserColor());
        //mainBody.GetComponent<Renderer>().material = mainMaterial;
        RegisterNewParticipantRpc(userConfig.GetProfileStruct());

        if(IsOwner) {
            mainMenu.SetSessionManager(this);
            gaze.SetActive(false);
        }
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    public List<string> GetParticipantsList()
    {
        return participants;
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