using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Constants
    //------------------------------------------------------------------------------------------------------

    public const int INTERACTION_STATE_POINT = 0;
    public const int INTERACTION_STATE_GRAB = 1;
    public const int INTERACTION_STATE_VOTE = 2;
    public const int TRACKING_OK_STATE = 0;                                    
    public const int TRACKING_WARNING_STATE = 1;                                     
    public const int TRACKING_NONE_STATE = 2;

    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private GameObject mainButton;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject participantsMenu;
    [SerializeField] private GameObject participantLabelPrefab;
    [SerializeField] private GameObject participantOptions;
    [SerializeField] private GameObject trackingPanel;
    [SerializeField] private TMP_Text   trackingStateLbl;
    [SerializeField] private TMP_Text   participantOptionsUsernameLabel;
    [SerializeField] private float mouseHoldTimer;

    private SessionManager sessionManager;

    private TMP_Text mainButtonLbl;
    private Button mainButtonBtn;

    private bool onMainMenu = false;
    private bool mousePressed = false;
    private float mouseHoldTime = 0.0f;
    private int currentInteractionState = INTERACTION_STATE_POINT;


    // Start is called before the first frame update
    void Start()
    {
        mainButtonLbl = mainButton.transform.GetComponentInChildren<TMP_Text>();
        mainButtonBtn = mainButton.GetComponentInChildren<Button>();
        mainButtonBtn.onClick.AddListener(OpenMainMenu);
    }

    void Update()
    {
        bool mouseClick = false;
        bool mouseHold = false;
        bool touchInput = Input.touchCount > 0;
        bool touchHold = false;

        if (Input.GetMouseButton(0))
        {
            mousePressed = true;
            mouseHoldTime += 0.5f;
            if (mouseHoldTime >= mouseHoldTimer)
            {
                Debug.Log("mouseHold");
                mouseHold = true;
                mouseHoldTime = 0.0f;
            }
        }
        else if (mousePressed)
        {
            mouseClick = true;
            mousePressed = false;
        }

        ManageInteraction(mouseClick, mouseHold, touchInput, touchHold);
    }

    public void ManageInteraction(bool mouseClick, bool mouseHold, bool touchInput, bool touchHold)
    {
        Ray ray = new Ray();

        if (mouseClick)
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (touchInput)
        {
            Touch touch = Input.GetTouch(index: 0);
            ray = Camera.main.ScreenPointToRay(touch.position);
        }

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            if (hit.transform.tag == "anchor")
            {
                GameObject selectedParticipant = hit.transform.gameObject;
                sessionManager.SelectParticipant(selectedParticipant);
                SessionManager selectedParticipantManager = selectedParticipant.GetComponentInParent<SessionManager>();
                OpenParticipantOptions(sessionManager.GetUsernameById(selectedParticipantManager.OwnerClientId));
            }
        }
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

    public void OnInteractionModeSelect(int mode)
    { 
        currentInteractionState = mode;
    }

    public void SetSessionManager(SessionManager sessionManager)
    {
        this.sessionManager = sessionManager;
    }
}