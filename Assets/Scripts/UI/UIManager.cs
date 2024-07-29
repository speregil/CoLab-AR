using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * Behaviour that controls which UI elements of the app are shown in a given moment
 */
public class UIManager : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] GameObject introMenu;              // Reference to the Main Intro menu canvas
    [SerializeField] GameObject createRoomMenu;         // Reference to the Room Creation menu canvas
    [SerializeField] GameObject joinRoomMenu;           // Reference to the Join to Room menu canvas
    [SerializeField] Color SelectedConfigStateColor;    // Color for the selected configuration option in the Workspace config menu

    GameObject workspaceConfigMenu;                     // Referene to the worspace config tools canvas


    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);    // The UI is created once in the intro scene
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    public void SetupWorkspaceConfigMenu()
    {
        workspaceConfigMenu = transform.Find("WorkspaceConfigMenu").gameObject;
        workspaceConfigMenu.SetActive(false);
    }

    /**
     * Shows the Room Creation menu in the intro scene
     */
    public void CreateRoom()
    {
        introMenu.SetActive(false);
        createRoomMenu.SetActive(true);
    }

    /**
     * Shows the Join to Room menu in the intro scene
     */
    public void JoinRoom()
    {
        introMenu.SetActive(false);
        joinRoomMenu.SetActive(true);
    }

    /**
     * Moves to the Main scene after accepting the room creation configuration
     */
    public void AcceptCreateRoom()
    {
        createRoomMenu.SetActive(false);
        SceneManager.LoadScene(sceneName: "Main");
    }

    /**
     * Moves to the Main scene after accepting the join to room configuration
     */
    public void AcceptJoinRoom()
    {
        joinRoomMenu.SetActive(false);
        SceneManager.LoadScene(sceneName: "Main");
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