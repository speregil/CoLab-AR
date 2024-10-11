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

    /*private NetworkVariable<UserConfiguration.ProfileStruct> lastProfile = new NetworkVariable<UserConfiguration.ProfileStruct>(
        new UserConfiguration.ProfileStruct
        {
            username = "New Profile",
            r = 150,
            g = 150,
            b = 150
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);*/
    private Dictionary<string,float> participants = new Dictionary<string, float>();
    private UserConfiguration userConfig;

    private int defaultParticipantCounter = 1;

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    public override void OnNetworkSpawn()
    {
        userConfig = GameObject.Find("OfflineConfig").GetComponent<UserConfiguration>();
        
        if (IsOwner)
            RegisterNewParticipantRpc(userConfig.GetProfileStruct());
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    [Rpc(SendTo.Server)]
    private void RegisterNewParticipantRpc(UserConfiguration.ProfileStruct newUserProfile)
    {
        string newParticipantName = newUserProfile.username.ToString();

        if(newParticipantName != null && newParticipantName != "")
            participants[newParticipantName] = newUserProfile.r;
        else
        {
            string defaultName = "New Participant" + defaultParticipantCounter;
            participants[defaultName] = 150.0f;
            defaultParticipantCounter++;
            newUserProfile.username = defaultName;
            newUserProfile.r = 150.0f;
            newUserProfile.g = 150.0f;
            newUserProfile.b = 150.0f;
        }

        foreach(string p in participants.Keys)
        {
            Debug.Log(p + "\n");
        }

        UpdateParticipantsListRpc(newUserProfile);
    }

    [Rpc(SendTo.Server)]
    public void PetitionCurrentParticipantsListRpc(ulong clientID)
    {
        return;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateParticipantsListRpc(UserConfiguration.ProfileStruct newUserProfile)
    {
        if (IsHost) return;

        string newParticipantName = newUserProfile.username.ToString();
        participants[newParticipantName] = newUserProfile.r;
        
        foreach (string p in participants.Keys)
        {
            Debug.Log(p + "\n");
        }
    }
}