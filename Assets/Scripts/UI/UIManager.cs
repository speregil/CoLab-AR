using Niantic.Lightship.SharedAR.Rooms;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private GameObject workspaceConfigMenu;        // Referene to the worspace config tools canvas
    [SerializeField] private Color SelectedConfigStateColor;        // Color for the selected configuration option in the Workspace config menu
    
    [SerializeField] private SessionManager sessionManager;         // Reference to the SessionManager
    [SerializeField] private WorkspaceConfig workspaceConfig;       // Reference to the Worspace Config behaviour

    private IntroManager introManager;                              // Reference to the Intro Scene actions manager[SerializeField]

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
     * Moves to the Main scene after accepting the room creation configuration
     */
    public void AcceptCreateRoom()
    {
        createRoomMenu.SetActive(false);
        sessionManager.CreateRoom(introManager.GetRoomName(),true);
        workspaceConfig.DetectingPlanes(true);
    }

    /**
     * Moves to the Main scene after accepting the join to room configuration
     */
    public void AcceptJoinRoom()
    {
        joinRoomMenu.SetActive(false);
        sessionManager.CreateRoom(introManager.GetRoomName(), false);
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
     * Moves back to the main intro menu fter canceling the join to room config
     */
    public void CancelJoinRoom()
    {
        introMenu.SetActive(true);
        joinRoomMenu.SetActive(false);
    }

    /**
     * Shows the workspace configuration menu
     */
    public void WorkspaceConfiguration()
    {
        workspaceConfigMenu.SetActive(true);
        GameObject configPanel = workspaceConfigMenu.transform.GetChild(0).gameObject;
        GameObject configButtons = configPanel.transform.GetChild(0).gameObject;
        Button positionXZ = configButtons.transform.Find("PositionXZBtn").gameObject.GetComponent<Button>();
        positionXZ.onClick.Invoke();
    }

    /**
     * Hides the workspace configuration menu
     */
    public void AcceptWorkspaceConfiguration()
    {
        workspaceConfigMenu.SetActive(false); 
    }
}