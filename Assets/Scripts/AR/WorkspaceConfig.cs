using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
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

    private GameObject currentEditableWorkspace;                // Current instance of the workspace plane
    private NetworkObject currentWorkspace;                      // Current NetworkObject of the workspace plane
    private IDragBehaviour drag;                                // Reference to the Drag Behaviour of the current workspace
    private int currentConfigState;                             // Defines the current Config State of the app

    private bool isDetectingPlanes = false;                     // Flag that determines if the script is currently tracking planes or not
    private bool isConfiguringWorkspace = false;                // Flag that determines if the user is currently configuring pos/rot of the workspace

    //------------------------------------------------------------------------------------------------------
    // Network Behaviour Functions
    //------------------------------------------------------------------------------------------------------

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        Debug.Log("Object spawned on network");
        GameObject arConfig = GameObject.Find("ARConfig");
        GameObject ui = GameObject.Find("UI");
        raycastManager = arConfig.GetComponentInChildren<ARRaycastManager>();
        trackingManager = arConfig.GetComponentInChildren<TrackingManager>();
        uiManager = ui.GetComponent<UIManager>();
        if (CheckForWorkspace())
        {
            Debug.Log("Found workspace");
            MoveParticipantsToWorkspaceAnchorRpc();
        }
        ConfigureWorspaceMenu();
        DetectingPlanes(IsServer);
    }

    void Update()
    {
        if (!IsOwner) return;

        // Input check for mobile builds
        if (isDetectingPlanes && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(index: 0);
            Vector2 touchPosition = touch.position;
            ARRaycasting(touchPosition);
        }
        // Input check for PC builds
        else if (isDetectingPlanes && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector2 mousePositionMap = new Vector2(mousePosition.x, mousePosition.y);
            ARRaycasting(mousePositionMap);
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

        if (raycastManager.Raycast(position, hits, trackableTypes: UnityEngine.XR.ARSubsystems.TrackableType.PlaneEstimated))
        {
            // Identyfies the plane hit with the raycast
            Pose pose = hits[0].pose;
            Vector2 planeSize = new Vector2(hits[0].trackable.gameObject.GetComponent<Renderer>().bounds.size.x, hits[0].trackable.gameObject.GetComponent<Renderer>().bounds.size.z);
            
            // Host call to create the workspace plane
            InstantiateWorkspace(pose.position,pose.rotation,planeSize, NetworkManager.Singleton.LocalClientId);

            // Deactivates plane detection and cleans the screen
            trackingManager.CleanTrackables();
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

    /**
     *  Creates a workspace plane based on the size of the selected detected plane
     *  @param planePosition Vector3 representing the position of the detected plane selected
     *  @param planeRotation Quaternion representing the rotation of the detected plane selected
     */
    public void InstantiateWorkspace(Vector3 planePosition, Quaternion planeRotation, Vector2 planeSize, ulong clientId)
    {
        // Instantiates the workspace and scales it
        if (IsServer) 
        {
            //NetworkObject worspaceNetworkObject = NetworkManager.SpawnManager.InstantiateAndSpawn(workspacePrefab.GetComponent<NetworkObject>(), clientId, false, false, false, planePosition, planeRotation);
            //currentWorkspaceInstance = worspaceNetworkObject.gameObject;
            currentEditableWorkspace = Instantiate(workspacePrefab, planePosition, planeRotation);
            Vector2 workspaceSize = new Vector2(currentEditableWorkspace.GetComponent<Renderer>().bounds.size.x, currentEditableWorkspace.GetComponent<Renderer>().bounds.size.z);
            Vector3 ratioSize = new Vector3(planeSize.x / workspaceSize.x, 1.0f, planeSize.y / workspaceSize.y);
            currentEditableWorkspace.transform.localScale = ratioSize;

            // Spawns the workspace in the room
            currentWorkspace = currentEditableWorkspace.GetComponent<NetworkObject>();

            currentWorkspace.SpawnWithOwnership(clientId);

            gameObject.transform.SetParent(currentWorkspace.transform);

            // Setups the configuration menu
            isConfiguringWorkspace = true;
            drag = currentEditableWorkspace.GetComponent<IDragBehaviour>();
            drag.SetOnConfig(true);
            SetConfigState(POSITIONXZ_STATE);
            uiManager.WorkspaceConfiguration();
        }
    }

    public bool CheckForWorkspace()
    {
        GameObject[] workspaceSearch = GameObject.FindGameObjectsWithTag("workspace");
        if(workspaceSearch.Length > 0)
        {
            currentWorkspace = workspaceSearch[0].GetComponent<NetworkObject>();
            return true;
        }
        return false;
    }

    [Rpc(SendTo.Server)]
    public void MoveParticipantsToWorkspaceAnchorRpc()
    {
        currentWorkspace = GameObject.FindGameObjectWithTag("workspace").GetComponent<NetworkObject>();
        if (IsHost)
        {
            Debug.Log("Is Host");
            foreach (NetworkClient participant in NetworkManager.Singleton.ConnectedClientsList)
            {
                Debug.Log("Found: " + participant.PlayerObject.gameObject.name);
                participant.PlayerObject.gameObject.transform.SetParent(currentWorkspace.transform);
            }
        }
    }
}