using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
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

    [Header("Managers")]

    [SerializeField] private UIManager uiManager;
    [SerializeField] private TrackingManager trackingManager;

    private SessionManager sessionManager;                                      // Reference to the SessionManager component
    private CrosshairBehaviour crosshairBehaviour;

    [Header("Top Panel")]

    [SerializeField] private GameObject mainButton;                             // Reference to the main button that opens the main menu
    [SerializeField] private Sprite mainButtonTexture;
    [SerializeField] private Sprite closeButtonTexture;
    [SerializeField] private Sprite backButtonTexture;
    [SerializeField] private GameObject trackingButton;                         // Reference to the tracking button
    [SerializeField] private Sprite trackingOffButtonTexture;
    [SerializeField] private Sprite trackingOnButtonTexture;
    [SerializeField] private Sprite trackingWarningButtonTexture;

    private Button mainButtonBtn;
    private Button trackingButtonBtn;

    [Header("Main Menu")]

    [SerializeField] private GameObject mainMenu;                               // Reference to the main menu panel
    [SerializeField] private GameObject participantsMenu;                       // Reference to the participants menu panel
    [SerializeField] private GameObject participantLabelPrefab;                 // Prefab for the labels of each entry in the participants menu
    [SerializeField] private GameObject sessionPanel;
    [SerializeField] private GameObject objectsPanel;

    [Header("Model Options")]

    [SerializeField] private GameObject modelsOptions;
    [SerializeField] private GameObject addModelsPanel;
    [SerializeField] private GameObject moveModelsPanel;
    [SerializeField] private GameObject moveModelsSelectButton;
    [SerializeField] private GameObject deleteModelsPanel;
    [SerializeField] private GameObject deleteModelsBtn;
    private Button selectButtonBtn;

    [Header("Participant Options")]

    [SerializeField] private GameObject participantOptions;                     // Reference to the participant options panel
    [SerializeField] private TMP_Text participantOptionsUsernameLabel;          // Reference to the username label in the participant options panel

    [Header("UI Objects")]

    [SerializeField] private GameObject sessionCamera;
    [SerializeField] private GameObject crosshairImage;

    private bool onMainMenu = false;                                            // Flag to know if the main menu is open
    private int currentInteractionState = INTERACTION_STATE_POINT;              // Current interaction state with digital objects


    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        mainButtonBtn = mainButton.GetComponent<Button>();
        trackingButtonBtn = trackingButton.GetComponent<Button>();
        selectButtonBtn = moveModelsSelectButton.GetComponent<Button>();
        mainButtonBtn.onClick.AddListener(OpenMainMenu);
        crosshairBehaviour = crosshairImage.GetComponent<CrosshairBehaviour>();
        SetTrackingStatus(TRACKING_NONE_STATE);
    }

    void Update()
    {
        bool mouseClick = Input.GetMouseButtonDown(0);
        bool touchInput = Input.touchCount > 0;

        ManageInteraction(mouseClick, touchInput);
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
    public void ManageInteraction(bool mouseClick, bool touchInput)
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
        if (trackingButtonBtn != null)
        {
            switch (code)
            {
                case TRACKING_OK_STATE:
                    Debug.Log("Tracking OK");
                    trackingButtonBtn.image.sprite = trackingOnButtonTexture;
                    break;
                case TRACKING_WARNING_STATE:
                    Debug.Log("Tracking Warning");
                    trackingButtonBtn.image.sprite = trackingWarningButtonTexture;
                    break;
                case TRACKING_NONE_STATE:
                    Debug.Log("Tracking None");
                    trackingButtonBtn.image.sprite = trackingOffButtonTexture;
                    break;
            }
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

    public GameObject GetToAddModel(int modelType)
    {
        return trackingManager.GetToAddModel(modelType);
    }

    public Vector3 GetCrosshairPosition()
    {
        return crosshairBehaviour.GetCurrentPosition();
    }

    public bool IsPositionOnButton(Vector3 position)
    {
        bool onPosition = false;
        onPosition = CheckCorners(mainButton.GetComponent<RectTransform>(), position);
        onPosition = onPosition || CheckCorners(trackingButton.GetComponent<RectTransform>(), position);
        onPosition = onPosition || CheckCorners(addModelsPanel.GetComponent<RectTransform>(), position);
        onPosition = onPosition || CheckCorners(deleteModelsBtn.GetComponent<RectTransform>(), position);

        return onPosition;
    }

    private bool CheckCorners(RectTransform uiRect, Vector3 position)
    {
        Vector3[] corners = new Vector3[4];
        uiRect.GetWorldCorners(corners);

        if (position.x >= corners[0].x && position.x <= corners[2].x &&
            position.y >= corners[0].y && position.y <= corners[2].y)
            return true;

        return false;
    }

    //------------------------------------------------------------------------------------------------------
    // UI Open Callbacks
    //------------------------------------------------------------------------------------------------------

    /**
     * Opens the main menu
     */
    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        mainButtonBtn.image.sprite = closeButtonTexture;
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
        mainButtonBtn.image.sprite = backButtonTexture;
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
     * Opens the participant options menu
     * @param username Username of the participant being interacted with
     */
    public void OpenParticipantOptions(string username)
    {
        if (!onMainMenu)
        {
            participantOptions.SetActive(true);
            participantOptionsUsernameLabel.text = username;
            mainButtonBtn.image.sprite = closeButtonTexture;
            mainButtonBtn.onClick.RemoveAllListeners();
            mainButtonBtn.onClick.AddListener(CloseParticipantOptions);
        }
    }

    /**
     * Opens the models options menu
     */
    public void OpenModelsOptions()
    {
        modelsOptions.SetActive(true);
        objectsPanel.SetActive(false);
        mainButtonBtn.image.sprite = backButtonTexture;
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseModelsOptions);
    }

    /**
     * Opens the selection panel to add a model to the scene
     */
    public void OpenAddModelsPanel()
    {
        modelsOptions.SetActive(false);
        addModelsPanel.SetActive(true);
        crosshairImage.SetActive(true);
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseAddModelsPanel);
    }

    public void OpenMoveModelsPanel()
    {
        modelsOptions.SetActive(false);
        moveModelsPanel.SetActive(true);
        crosshairImage.SetActive(true);
        trackingManager.ActivateModelMovement(true);
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseMoveModelsPanel);
    }

    public void OpenDeleteModelPanel() 
    {
        modelsOptions.SetActive(false);
        deleteModelsPanel.SetActive(true);
        trackingManager.ActivateModelDeletion(true);
        crosshairImage.SetActive(true);
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseDeleteModelsPanel);
    }

    public void OpenSessionPanel()
    {
        mainMenu.SetActive(false);
        sessionPanel.SetActive(true);
        mainButtonBtn.image.sprite = backButtonTexture;
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseSessionPanel);
    }

    public void OpenObjectsPanel()
    {
        mainMenu.SetActive(false);
        objectsPanel.SetActive(true);
        mainButtonBtn.image.sprite = backButtonTexture;
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseObjectsPanel);
    }

    //-------------------------------------------------------------------------------------------------------
    // UI Close Callbacks
    //-------------------------------------------------------------------------------------------------------

    /**
     * Closes the main menu
     */
    public void CloseMainMenu()
    {
        mainMenu.SetActive(false);
        crosshairImage.SetActive(false);
        mainButtonBtn.image.sprite = mainButtonTexture;
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
        participantsMenu.SetActive(false);
        mainButtonBtn.image.sprite = closeButtonTexture;
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseMainMenu);
        OpenMainMenu();
    }

    /**
     * Closes the participant options menu
     */
    public void CloseParticipantOptions()
    {
        participantOptions.SetActive(false);
        mainButtonBtn.image.sprite = mainButtonTexture;
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(OpenMainMenu);
        sessionManager.UnselectParticipant();
    }

    /**
     * Closes the models options menu
     */
    public void CloseModelsOptions()
    {
        modelsOptions.SetActive(false);
        OpenObjectsPanel();
    }

    /**
     * Closes the model selection panel
     */
    public void CloseAddModelsPanel()
    {
        trackingManager.ActivateModelPositioning(false);
        addModelsPanel.SetActive(false);
        crosshairBehaviour.ResetCrosshair();
        crosshairImage.SetActive(false);
        OpenModelsOptions();
    }

    private void CloseMoveModelsPanel()
    {
        moveModelsPanel.SetActive(false);
        crosshairBehaviour.ResetCrosshair();
        crosshairImage.SetActive(false);
        trackingManager.SelectCurrentModel(false);
        trackingManager.ActivateModelMovement(false);
        OpenModelsOptions();
    }

    public void CloseDeleteModelsPanel()
    {
        trackingManager.ActivateModelDeletion(false);
        deleteModelsPanel.SetActive(false);
        crosshairBehaviour.ResetCrosshair();
        crosshairImage.SetActive(false);
        OpenModelsOptions();
    }

    public void CloseSessionPanel()
    {
        sessionPanel.SetActive(false);
        mainButtonBtn.image.sprite = closeButtonTexture;
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseMainMenu);
        OpenMainMenu();
    }

    public void CloseObjectsPanel()
    {
        objectsPanel.SetActive(false);
        mainButtonBtn.image.sprite = closeButtonTexture;
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseMainMenu);
        OpenMainMenu();
    }

    //------------------------------------------------------------------------------------------------------
    // Callbacks for UI Buttons
    //------------------------------------------------------------------------------------------------------

    /**
     * Callback for the button that changes the type of model to add to the scene
     * @param modelType Type of the model to add
     */
    public void ActivateModelPositioning(int modelType)
    {
        trackingManager.ChangeToAddModel(modelType);
        trackingManager.ActivateModelPositioning(true);
    }

    public void OnPlaceModel()
    {
        int modelType = trackingManager.GetToAddModelType();
        if (modelType >= 0)
        {
            Vector3 hitPosition = trackingManager.GetHitPosition();
            ulong clientId = NetworkManager.Singleton.LocalClientId;
            sessionManager.AddModelRpc(modelType, hitPosition, clientId);
        }
    }

    public void OnDeleteModel()
    {
        GameObject modelToDelete = trackingManager.GetCurrentSelection();
        if(modelToDelete != null) 
        { 
            Debug.Log("You are going to delete: " + modelToDelete.name);
            sessionManager.DeleteModelRpc(modelToDelete.GetComponent<ModelData>().ModelId);
        }
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

    public void OnMoveModelSelect()
    {
        if (trackingManager.SelectCurrentModel(true))
        {
            moveModelsSelectButton.GetComponentInChildren<TMP_Text>().text = "Drop";
            selectButtonBtn.onClick.RemoveAllListeners();
            selectButtonBtn.onClick.AddListener(OnDropModelSelect);
        }
    }

    public void OnDropModelSelect()
    {
        trackingManager.SelectCurrentModel(false);
        moveModelsSelectButton.GetComponentInChildren<TMP_Text>().text = "Select";
        selectButtonBtn.onClick.RemoveAllListeners();
        selectButtonBtn.onClick.AddListener(OnMoveModelSelect);
    }

    public void OnChangeModelMovement(bool active)
    {
        trackingManager.SetXZModelMovement(active);
    }

    public void OnLeaveRoom()
    {
        CloseSessionPanel();
        CloseMainMenu();
        sessionManager.LeaveRoom();
        uiManager.LeaveRoom();
    }
}