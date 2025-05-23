using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/**
 * Network Behaviour for creating the workspace plane of a room and configure it
 */
public class WorkspaceConfig : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Constants
    //------------------------------------------------------------------------------------------------------

    public const int POSITIONXZ_STATE = 0;                                    // Defines the Config State to moving the workspace in the XZ plane
    public const int POSITIONY_STATE = 1;                                     // Defines the Config State to moving the workspace in the Y plane
    public const int ROTATION_STATE = 2;                                      // Defines the Config State to rotating the workspace around the Y axis
    public const int SCALE_STATE = 3;                                         // Defines the Config State to scaling the workspace along the X and Z axis

    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private GameObject workspacePrefab;        // Prefab for the workspace plane      
    
    private ARRaycastManager raycastManager;                    // Reference to the ARRaycastManager component in the parent object
    private TrackingManager trackingManager;                    // Reference to the TrackingManager component in the parent object
    private UIManager uiManager;                                // Reference to the UIManager component loaded in the scene
    private PlayerInput playerInput;

    private GameObject currentEditableWorkspace;                // Current instance of the workspace plane
    private NetworkObject currentWorkspace;                      // Current NetworkObject of the workspace plane
    private DragBehaviour drag;                                // Reference to the Drag Behaviour of the current workspace
    private int currentConfigState;                             // Defines the current Config State of the app

    private InputAction touchPress;
    private InputAction touchPosition;

    private bool isDetectingPlanes = false;                     // Flag that determines if the script is currently tracking planes or not
    private bool isConfiguringWorkspace = false;                // Flag that determines if the user is currently configuring pos/rot of the workspace

    //------------------------------------------------------------------------------------------------------
    // Network Behaviour Functions
    //------------------------------------------------------------------------------------------------------

    public override void OnNetworkSpawn()
    {
        GameObject arConfig = GameObject.Find("XR Origin");
        GameObject ui = GameObject.Find("UI");
        raycastManager = arConfig.GetComponent<ARRaycastManager>();
        trackingManager = arConfig.GetComponent<TrackingManager>();
        uiManager = ui.GetComponent<UIManager>();
        playerInput = ui.GetComponent<PlayerInput>();
        touchPress = playerInput.actions["TouchPress"];
        touchPosition = playerInput.actions["TouchPosition"];
        ConfigureWorspaceMenu();
        FindLocalWorkspace();

        if (IsOwner && IsServer) DetectingPlanes(true);
    }

    void Update()
    {
        if (!IsOwner) return;

        if (isDetectingPlanes && touchPress.WasPressedThisFrame())
        {
            Vector2 position = touchPosition.ReadValue<Vector2>();
            ARRaycasting(position);
        }
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Searches and adds the proper listeners to the buttons in the workspace configuration menu
     */
    private void ConfigureWorspaceMenu()
    {
        Button configBtn = uiManager.GetWorkspaceConfigButton("AcceptBtn", false);
        configBtn.onClick.AddListener(() => FinishConfiguration());

        configBtn = uiManager.GetWorkspaceConfigButton("ResetBtn", false);
        configBtn.onClick.AddListener(() => ResetConfiguration());

        configBtn = uiManager.GetWorkspaceConfigButton("PositionXZBtn", true);
        configBtn.onClick.AddListener(() => SetConfigState(POSITIONXZ_STATE));

        configBtn = uiManager.GetWorkspaceConfigButton("PositionYBtn", true);
        configBtn.onClick.AddListener(() => SetConfigState(POSITIONY_STATE));

        configBtn = uiManager.GetWorkspaceConfigButton("RotationBtn", true);
        configBtn.onClick.AddListener(() => SetConfigState(ROTATION_STATE));

        configBtn = uiManager.GetWorkspaceConfigButton("ScaleBtn", true);
        configBtn.onClick.AddListener(() => SetConfigState(SCALE_STATE));
    }

    /**
     * Controls the raycasting process and the creation of the workspace plane when selecting a detected plane
     * @param position Vector2 representing the raycat origin position
     */
    private void ARRaycasting(Vector2 position)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (raycastManager.Raycast(position, hits, TrackableType.PlaneEstimated))
        {
            // Identyfies the plane hit with the raycast
            Pose pose = hits[0].pose;
            hits[0].trackable.gameObject.tag = "selected";
            Vector2 planeSize = new Vector2(hits[0].trackable.gameObject.GetComponent<Renderer>().bounds.size.x, hits[0].trackable.gameObject.GetComponent<Renderer>().bounds.size.z);
            
            // Host call to create the workspace plane
            InstantiateWorkspaceRpc(pose.position,pose.rotation,planeSize, NetworkManager.Singleton.LocalClientId);

            // Deactivates plane detection and cleans the screen
            trackingManager.CleanDetectedPlanes();
            DetectingPlanes(false);
        }
    }

    /**
     * Getter to determine if the beheviour already creted a workspace and is in configuration mode or not
     * @return bool If the behaviour is in the configuration state or not
     */
    public bool IsConfiguringWorkspace()
    {
        return isConfiguringWorkspace;
    }

    /**
     * Getter to determine if the beheviouris detectig planes to configure
     * @return bool If the behaviour is in the detection phase or not
     */
    public bool IsDetectingPlanes()
    {
        return isDetectingPlanes;
    }

    /**
     * Setter that determines if the behaviours is currently detecting pplanes in the environment or not
     * @param isDetecting If the behaviour should start or stop detecting planes
     */
    public void DetectingPlanes(bool isDetecting)
    {
        isDetectingPlanes = isDetecting;
        trackingManager.EnableTracking(isDetecting);  
    }

    /**
     * Getter to determine the current configuration state
     * @return int Current configuration state
     * POSITIONXZ_STATE Moving the workspace in the XZ plane
     * POSITIONY_STATE  Moving the workspace in the Y plane
     * ROTATION_STATE Rotating the workspace around the Y axis
     * SCALE_STATE Scaling the workspace along the X and Z axis
     */
    public int GetCurrentConfigState()
    {
        return currentConfigState;
    }

    /**
     * Setter for the current configuration state
     * @param state Constant determining the state
     * POSITIONXZ_STATE Moving the workspace in the XZ plane
     * POSITIONY_STATE  Moving the workspace in the Y plane
     * ROTATION_STATE Rotating the workspace around the Y axis
     * SCALE_STATE Scaling the workspace along the X and Z axis
     */
    public void SetConfigState(int state)
    {
        drag.SetOnConfig(true);
        currentConfigState = state;
        drag.SetConfigState(currentConfigState);
    }

    /**
     * Signals the end of the workspace configurations and asks the UIManager to hide the menu
     */
    public void FinishConfiguration()
    {
        
        isConfiguringWorkspace = false;
        drag.SetOnConfig(false);
        uiManager.AcceptWorkspaceConfiguration();
    }

    /**
     * Asks the current workspace to reset its transform to the original position, scale and rotation detected
     */
    public void ResetConfiguration()
    {
        drag.ResetToInitial();
    }

    public GameObject GetCurrentWorkspace()
    {
        return currentWorkspace.gameObject;
    }

    public void FindLocalWorkspace()
    {
        GameObject[] results = GameObject.FindGameObjectsWithTag("workspace");
        if (results.Length > 0)
        {
            currentEditableWorkspace = results[0];
            currentWorkspace = currentEditableWorkspace.GetComponent<NetworkObject>();
        }
    }

    /**
     *  Creates a workspace plane based on the size of the selected detected plane
     *  @param planePosition Vector3 representing the position of the detected plane selected
     *  @param planeRotation Quaternion representing the rotation of the detected plane selected
     */
    [Rpc(SendTo.Server)]
    public void InstantiateWorkspaceRpc(Vector3 planePosition, Quaternion planeRotation, Vector2 planeSize, ulong clientId)
    {
        // Spawns the workspace in the room
        currentEditableWorkspace = Instantiate(workspacePrefab);
        currentWorkspace = currentEditableWorkspace.GetComponent<NetworkObject>();
        currentWorkspace.SpawnWithOwnership(clientId);

        // Instantiates the workspace and scales it
        Vector2 workspaceSize = new Vector2(currentEditableWorkspace.GetComponent<Renderer>().bounds.size.x, currentEditableWorkspace.GetComponent<Renderer>().bounds.size.z);
        Vector3 ratioSize = new Vector3(planeSize.x / workspaceSize.x, 1.0f, planeSize.y / workspaceSize.y);
        currentWorkspace.gameObject.transform.localScale = ratioSize;
        currentWorkspace.gameObject.transform.position = planePosition;
        currentWorkspace.gameObject.transform.rotation = planeRotation;

        // Setups the configuration menu
        isConfiguringWorkspace = true;
        drag = currentWorkspace.gameObject.GetComponent<DragBehaviour>();
        drag.SetUIManager(uiManager);   
        uiManager.WorkspaceConfiguration();
    }
}