using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/**
 * Behaviour that controls the main menu of the application
 */
public class MainMenuManager : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Constants
    //------------------------------------------------------------------------------------------------------

    // Constants for the selected interaction type with digital objects
    public const int INTERACTION_STATE_POINT = 0;
    public const int INTERACTION_STATE_GRAB = 1;
    public const int INTERACTION_STATE_VOTE = 2;

    // Constants for the tracking status of the Image Tracking
    public const int TRACKING_OK_STATE = 0;                                    
    public const int TRACKING_WARNING_STATE = 1;                                     
    public const int TRACKING_NONE_STATE = 2;

    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private GameObject mainButton;                             // Reference to the main button that opens the main menu
    [SerializeField] private GameObject mainMenu;                               // Reference to the main menu panel
    [SerializeField] private GameObject participantsMenu;                       // Reference to the participants menu panel
    [SerializeField] private GameObject participantLabelPrefab;                 // Prefab for the labels of each entry in the participants menu
    [SerializeField] private GameObject participantOptions;                     // Reference to the participant options panel
    [SerializeField] private TMP_Text participantOptionsUsernameLabel;          // Reference to the username label in the participant options panel 
    [SerializeField] private GameObject trackingButton;                         // Reference to the tracking button
    [SerializeField] private TMP_Text   trackingStateLbl;                       // Reference to the label in the tracking button
    [SerializeField] private GameObject modelsOptions;
    [SerializeField] private GameObject addModelsPanel;
    [SerializeField] private float mouseHoldTimer;                              // Time to consider a mouse hold
    [SerializeField] private GameObject sessionCamera;                          // Reference to the ARCamera component of each participant

    private SessionManager sessionManager;                                      // Reference to the SessionManager component

    private TMP_Text mainButtonLbl;                                             // Reference to the text label of the main button
    private Button mainButtonBtn;                                               // Reference to the button component of the main button

    private bool onMainMenu = false;                                            // Flag to know if the main menu is open
    private bool mousePressed = false;                                          // Flag to know if the mouse is pressed
    private float mouseHoldTime = 0.0f;                                         // Time since the last frame the mouse was pressed
    private int currentInteractionState = INTERACTION_STATE_POINT;              // Current interaction state with digital objects


    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        mainButtonLbl = mainButton.transform.GetComponentInChildren<TMP_Text>();
        mainButtonBtn = mainButton.GetComponentInChildren<Button>();
        mainButtonBtn.onClick.AddListener(OpenMainMenu);
        SetTrackingStatus(TRACKING_NONE_STATE);
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

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Manages the interaction with digital objects in the scene
     * @param mouseClick Flag to know if the mouse was clicked
     * @param mouseHold Flag to know if the mouse is being held
     * @param touchInput Flag to know if there is touch input
     * @param touchHold Flag to know if the touch is being held
     */
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

    /**
     * Sets the tracking status of the Image Tracking
     * @param code Code of the status of the tracking
     */
    public void SetTrackingStatus(int code)
    {
        switch (code)
        {
            case TRACKING_OK_STATE:
                Debug.Log("Tracking OK");
                trackingStateLbl.color = Color.green;
                break;
            case TRACKING_WARNING_STATE:
                Debug.Log("Tracking Warning");
                trackingStateLbl.color = Color.yellow;
                break;
            case TRACKING_NONE_STATE:
                Debug.Log("Tracking None");
                trackingStateLbl.color = Color.red;
                break;
        }
    }

    /**
     * Activates the main menu button after the intro settings
     */
    public void ActivateMainMenu()
    {
        trackingButton.SetActive(false);
        mainButton.SetActive(true);
    }

    /**
     * Opens the main menu
     */
    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        mainButtonLbl.text = "Close";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseMainMenu);
        onMainMenu = true;
    }

    /**
     * Opens the participants menu
     */
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

     /**
     * Adds a participant to the participants menu list
     * @param username Username of the participant to add
     */
    private void AddParticipantToList(string username)
    {
        GameObject labelInstance = GameObject.Instantiate(participantLabelPrefab);
        TMP_Text usernameLbl = labelInstance.GetComponentInChildren<TMP_Text>();
        usernameLbl.text = username;
        labelInstance.transform.SetParent(participantsMenu.transform, false);
    }

    /**
     * Opens the participant options menu
     * @param username Username of the participant being interacted with
     */
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

    public void OpenModelsOptions()
    {
        modelsOptions.SetActive(true);
        mainMenu.SetActive(false);
        mainButtonLbl.text = "Back";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseModelsOptions);
    }

    public void OpenAddModelsPanel()
    {
        modelsOptions.SetActive(false);
        addModelsPanel.SetActive(true);
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseAddModelsPanel);
    }

    /**
     * Closes the main menu
     */
    public void CloseMainMenu()
    {
        mainMenu.SetActive(false);
        mainButtonLbl.text = "Open";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(OpenMainMenu);
        onMainMenu = false;
    }

    /**
     * Closes the participants menu
     */
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

    /**
     * Closes the participant options menu
     */
    public void CloseParticipantOptions()
    {
        participantOptions.SetActive(false);
        mainButtonLbl.text = "Open";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(OpenMainMenu);
        sessionManager.UnselectParticipant();
    }

    public void CloseModelsOptions()
    {
        modelsOptions.SetActive(false);
        OpenMainMenu();
    }

    public void CloseAddModelsPanel()
    {
        addModelsPanel.SetActive(false);
        OpenModelsOptions();
    }

    /**
     * Callback for the gaze toggle button to turn the gaze of a selected participant on or off
     * @param toggle Flag to know if the gaze is active or not
     */
    public void OnGazeToggle(bool toggle)
    {
        sessionManager.SetGazeActive(toggle);
    }

    /**
     * Callback for the nameplate toggle button to turn the nameplate of a selected participant on or off
     * @param toggle Flag to know if the nameplate is active or not
     */
    public void OnNameplateToggle(bool toggle)
    {
        sessionManager.SetNameplateActive(toggle);
    }

    /**
     * Callback for the ping button to trigger the ping animation on a selected participant
     */
    public void OnPingParticipant()
    {
        sessionManager.PingParticipant();
    }

    /**
     * Callback for the interaction mode buttons to change the interaction mode with digital objects
     * @param mode Interaction mode to set
     */
    public void OnInteractionModeSelect(int mode)
    { 
        currentInteractionState = mode;
    }

    /**
     * Assignis the session manager of the session participant to the main menu manager
     * @param sessionManager Reference to the SessionManager component of the session participant
     */
    public void SetSessionManager(SessionManager sessionManager)
    {
        this.sessionManager = sessionManager;
    }

    /**
     * Return the ARCamera component of the session participant
     * @return GameObject Reference to the ARCamera component of the session participant
     */
    public GameObject GetSessionCamera()
    {
        return sessionCamera;
    }
}