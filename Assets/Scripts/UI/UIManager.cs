using UnityEngine;
using UnityEngine.UI;

/**
 * Behaviour that controls which UI elements of the app are shown in a given moment
 */
public class UIManager : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private GameObject introMenu;                  // Reference to the Main Intro menu canvas
    [SerializeField] private GameObject createRoomMenu;             // Reference to the Room Creation menu canvas
    [SerializeField] private GameObject joinRoomMenu;               // Reference to the Join to Room menu canvas
    [SerializeField] private GameObject profileMenu;                // Reference to the Profile Configuration menu canvas
    [SerializeField] private GameObject workspaceConfigMenu;        // Referene to the worspace config tools canvas
    [SerializeField] private GameObject mainMenu;                   // Referene to the main menu canvas
    [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private Color SelectedConfigStateColor;        // Color for the selected configuration option in the Workspace config menu

    [SerializeField] private UserConfiguration userConfiguration;   // Reference to the offline user configuration component
    private IntroManager introManager;                              // Reference to the Intro Scene actions manager
    

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        introManager = GetComponent<IntroManager>();
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Shows the Room Creation menu in the intro scene
     */
    public void CreateRoom()
    {
        introMenu.SetActive(false);
        createRoomMenu.SetActive(true);
    }

    /**
     * Shows and initilizes the Join to Room menu in the intro scene
     */
    public void JoinRoom()
    {
        introMenu.SetActive(false);
        introManager.InitializeJoinRoom();
        joinRoomMenu.SetActive(true);
    }

    /**
     * Shows and initilizes the Profile configuration menu in the intro scene
     */
    public void ProfileMenu()
    {
        introMenu.SetActive(false);
        introManager.InitializeProfileMenu(userConfiguration.GetUsername(), userConfiguration.GetColorValue());
        profileMenu.SetActive(true);
    }

    /**
     * Moves to the Main view after accepting the room creation configuration
     */
    public void AcceptCreateRoom()
    {
        createRoomMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    /**
     * Moves to the Main scene after accepting the join to room configuration
     */
    public void AcceptJoinRoom()
    {
        joinRoomMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    /**
     * Reacts to the action of saving any changes in the Profile menu
     */
    public string SaveProfile(string username, Color userColor, int colorValue)
    {
        return userConfiguration.UpdateAndSaveProfile(username, userColor, colorValue);
    }

    public string LoadProfile() 
    { 
        return userConfiguration.LoadProfile();
    }

    /**
     * Moves back to the main intro menu after canceling the room creation config
     */
    public void CancelCreateRoom()
    {
        introMenu.SetActive(true);
        createRoomMenu.SetActive(false);
    }

    /**
     * Moves back to the main intro menu after canceling the join to room config
     */
    public void CancelJoinRoom()
    {
        introMenu.SetActive(true);
        joinRoomMenu.SetActive(false);
    }

    /**
     * Moves back to the main intro menu from the profile menu
     */
    public void CancelProfileMenu()
    {
        introMenu.SetActive(true);
        profileMenu.SetActive(false);
    }

    /**
     * Shows the workspace configuration menu
     */
    public void WorkspaceConfiguration()
    {
        workspaceConfigMenu.SetActive(true);
        ActivateCrosshair(true, true, 0);
        GameObject configPanel = workspaceConfigMenu.transform.GetChild(0).gameObject;
        GameObject configButtons = configPanel.transform.GetChild(0).gameObject;
    }

    /**
     * Hides the workspace configuration menu and shows the main menu button
     */
    public void AcceptWorkspaceConfiguration()
    {
        workspaceConfigMenu.SetActive(false);
        ActivateCrosshair(false,false,1);
        mainMenu.SetActive(true);
    }

    /**
     * Returns a reference to one of the buttons of the Workspace Configuration menu
     * @param buttonName Name of the button beeing search
     * @param isConfig True if the button is part of the configuration set , False if
     *  it is from the Flow set
     * @return Button The button component associated with the name given as parameter
     */
    public Button GetWorkspaceConfigButton(string buttonName, bool isConfig)
    {
        GameObject workspaceButton = null;
        Transform panel = workspaceConfigMenu.transform.Find("Panel");
        Transform configButtons = panel.Find("ConfigButtons");
        Transform flowButtons = panel.Find("FlowButtons");

        if (isConfig)
            workspaceButton = configButtons.Find(buttonName).gameObject;
        else
            workspaceButton = flowButtons.Find(buttonName).gameObject;

        return workspaceButton.GetComponent<Button>();
    }

    public void ChangeTrackingState(int status)
    {
        mainMenuManager.SetTrackingStatus(status);
    }

    public Vector3 GetCrosshairPosition()
    {
        return mainMenuManager.GetCrosshairPosition();
    }

    public Vector3 GetCrosshairDiffPosition()
    {
        return mainMenuManager.GetCrosshairDiffPosition();
    }

    public void ActivateCrosshair(bool activate, bool bouncy, int type)
    {
        mainMenuManager.ActivateCrosshair(activate, bouncy, type);
    }

    public bool IsPositionOnButton(Vector3 position)
    {
        bool isOnButton = false;
        isOnButton = mainMenuManager.IsPositionOnButton(position);
        isOnButton = mainMenuManager.CheckCorners(workspaceConfigMenu.transform.GetChild(0).GetComponent<RectTransform>(), position);
        return isOnButton;
    }

    public void LeaveRoom()
    {
        mainMenu.SetActive(false);
        introMenu.SetActive(true);
    }
}