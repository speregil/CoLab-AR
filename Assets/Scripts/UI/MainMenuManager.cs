using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public const int TRACKING_OK_STATE = 0;                                    
    public const int TRACKING_WARNING_STATE = 1;                                     
    public const int TRACKING_NONE_STATE = 2;

    [SerializeField] private GameObject mainButton;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject participantsMenu;
    [SerializeField] private GameObject participantLabelPrefab;
    [SerializeField] private GameObject participantOptions;
    [SerializeField] private GameObject trackingPanel;
    [SerializeField] private TMP_Text   trackingStateLbl;
    [SerializeField] private TMP_Text   participantOptionsUsernameLabel;

    private SessionManager sessionManager;

    private TMP_Text mainButtonLbl;
    private Button mainButtonBtn;

    private bool onMainMenu = false;
    

    // Start is called before the first frame update
    void Start()
    {
        mainButtonLbl = mainButton.transform.GetComponentInChildren<TMP_Text>();
        mainButtonBtn = mainButton.GetComponentInChildren<Button>();
        mainButtonBtn.onClick.AddListener(OpenMainMenu);
    }

    public void SetTrackingStatus(string status, int code)
    {
        trackingStateLbl.text = status;
        switch (code)
        {
            case TRACKING_OK_STATE:
                trackingStateLbl.color = Color.green;
                break;
            case TRACKING_WARNING_STATE:
                trackingStateLbl.color = Color.yellow;
                break;
            case TRACKING_NONE_STATE:
                trackingStateLbl.color = Color.red;
                break;
        }
    }

    public void ActivateMainMenu()
    {
        trackingPanel.SetActive(false);
        mainButton.SetActive(true);
    }

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        mainButtonLbl.text = "Close";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseMainMenu);
        onMainMenu = true;
    }

    public void OpenParticipantsMenu()
    {
        mainMenu.SetActive(false);
        participantsMenu.SetActive(true);
        mainButtonLbl.text = "Back";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseParticipantsMenu);

        Dictionary<string, Color> participants = sessionManager.GetParticipantsList();
        if (participants != null)
        {
            foreach (string participant in participants.Keys)
            {
                AddParticipantToList(participant);
            }
        }
        else
            Debug.Log("Invalid list");

    }

    private void AddParticipantToList(string username)
    {
        GameObject labelInstance = GameObject.Instantiate(participantLabelPrefab);
        TMP_Text usernameLbl = labelInstance.GetComponentInChildren<TMP_Text>();
        usernameLbl.text = username;
        labelInstance.transform.SetParent(participantsMenu.transform, false);
    }

    public void OpenParticipantOptions(string username)
    {
        if (!onMainMenu)
        {
            participantOptions.SetActive(true);
            participantOptionsUsernameLabel.text = username;
            mainButtonLbl.text = "Close";
            mainButtonBtn.onClick.RemoveAllListeners();
            mainButtonBtn.onClick.AddListener(CloseParticipantOptions);
        }
    }

    public void CloseMainMenu()
    {
        mainMenu.SetActive(false);
        mainButtonLbl.text = "Open";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(OpenMainMenu);
        onMainMenu = false;
    }

    public void CloseParticipantsMenu()
    {
        foreach (Transform child in participantsMenu.transform)
        {
            Destroy(child.gameObject);
        }

        participantOptionsUsernameLabel.text = "username";
        mainMenu.SetActive(true);
        participantsMenu.SetActive(false);
        mainButtonLbl.text = "Close";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseMainMenu);
    }

    public void CloseParticipantOptions()
    {
        participantOptions.SetActive(false);
        mainButtonLbl.text = "Open";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(OpenMainMenu);
        sessionManager.UnselectParticipant();
    }

    public void OnGazeToggle(bool toggle)
    {
        sessionManager.SetGazeActive(toggle);
    }

    public void OnNameplateToggle(bool toggle)
    {
        sessionManager.SetNameplateActive(toggle);
    }

    public void OnPingParticipant()
    {
        sessionManager.PingParticipant();
    }

    public void SetSessionManager(SessionManager sessionManager)
    {
        this.sessionManager = sessionManager;
    }
}