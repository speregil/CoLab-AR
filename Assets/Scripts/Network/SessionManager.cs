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

    private Dictionary<string, Color> participants = new Dictionary<string, Color>();
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

        if (IsOwner) {
            mainMenu.SetSessionManager(this);
            gaze.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            Ray ray = new Ray();

            if(Input.GetMouseButtonDown(0))
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(index: 0);
                ray = Camera.main.ScreenPointToRay(touch.position);
            }

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                if(hit.transform.tag == "anchor")
                    mainMenu.OpenParticipantOptions();
            }
        }
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

    public void ApplyAnchorColor(Color participantColor)
    {
        Material mainMaterial = mainBody.GetComponent<Renderer>().material;
        mainMaterial.SetColor("_Color", normalizeColor(participantColor));
    }

    private void LocalUpdateAnchorColorRpc()
    {
        GameObject[] localParticipants = GameObject.FindGameObjectsWithTag("participant");

        Debug.Log(localParticipants.Length);
        foreach (GameObject local in localParticipants)
        {
            SessionManager localManager = local.GetComponent<SessionManager>();
            ulong localOwnerId = localManager.OwnerClientId;

            string username = "";
            foreach (string participant in participants.Keys)
            {
                string[] data = participant.Split(":");
                ulong key = ulong.Parse(data[1]);
                if (localOwnerId == key) username = participant;
            }

            //Debug.Log(username);
            if (username != "")
            {
                Color participantColor = participants[username];
                localManager.ApplyAnchorColor(participantColor);
            }
        }

    }

    public Color normalizeColor(Color baseColor)
    {
        Color normalizedColor = new Color();
        normalizedColor.r = baseColor.r / 255;
        normalizedColor.g = baseColor.g / 255;
        normalizedColor.b = baseColor.b / 255;
        return normalizedColor;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void RegisterNewParticipantRpc(UserConfiguration.ProfileStruct newUserProfile, ulong clientID)
    {
        string newParticipantName = newUserProfile.username.ToString() + ":" + clientID;
        Color participantColor = new Color(newUserProfile.r, newUserProfile.g, newUserProfile.b);

        if(newParticipantName != null && newParticipantName != "") {
            participants[newParticipantName] = participantColor;
        }
        else
        {
            newParticipantName = "New Participant " + defaultParticipantCounter;
            participants[newParticipantName] = participantColor;
            defaultParticipantCounter++;
        }

        if(IsOwner)
            LocalUpdateAnchorColorRpc();
    }
}