using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.ARFoundation.VisualScripting;

/**
 * Behaviour that controls the network functions of a participant who joined a room
 */
public class SessionManager : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    //[SerializeField] private Dictionary<string,float> participants = new Dictionary<string, float>();
    [SerializeField] private List<string> participants = new List<string>();
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

        if(IsOwner) mainMenu.SetSessionManager(this);
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
            //participants[newParticipantName] = newUserProfile.r;
            participants.Add(newParticipantName);
        }
        else
        {
            newParticipantName = "New Participant" + defaultParticipantCounter;
            participants.Add(newParticipantName);
            /*participants[defaultName] = 150.0f;
            defaultParticipantCounter++;
            newUserProfile.username = defaultName;
            newUserProfile.r = 150.0f;
            newUserProfile.g = 150.0f;
            newUserProfile.b = 150.0f;*/
        }

        //UpdateParticipantsListRpc(newParticipantName);
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