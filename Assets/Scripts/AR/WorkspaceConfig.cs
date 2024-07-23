using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

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

    [SerializeField] private GameObject workspacePrefab;                      // Prefab for the workspace plane      
    [SerializeField] private GameObject workspaceConfigUI;                    // Reference to the Room Configuration UI elements

    private ARRaycastManager raycastManager;                                  // Reference to the ARRaycastManager component in the parent object
    private ARPlaneManager planeManager;                                      // Reference to the ARPlaneManager component in the parent object
    private TrackingManager trackingManager;                                  // Reference to the TrackingManager component in the parent object
    private UIManager uiManager;                                              // Reference to the UIManager component loaded in the scene

    private GameObject currentWorkspaceInstance;                              // Current instance of the workspace plane
    private DragWorkspace drag;                                               // Reference to the Drag Behaviour of the current workspace
    private int currentConfigState;                                           // Defines the current Config State of the app

    private bool isDetectingPlanes = true;                                    // Flag that determines if the script is currently tracking planes or not
    private bool isConfiguringWorkspace = false;                              // Flag that determines if the user is currently configuring pos/rot of the workspace

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        // Setup references with components in the parent object
        raycastManager = transform.parent.gameObject.GetComponent<ARRaycastManager>();
        planeManager = transform.parent.gameObject.GetComponent<ARPlaneManager>();
        trackingManager = transform.parent.gameObject.GetComponent<TrackingManager>();

        // Setup references to the UIManager and trasfer of control of the RoomConfigMenu
        uiManager = GameObject.Find("UI").GetComponent<UIManager>();
        workspaceConfigUI.transform.SetParent(uiManager.transform,false);
        uiManager.SetupWorkspaceConfigMenu();
    }

    void Update()
    {
        // Input check for mobile builds
        if (Input.touchCount > 0 && isDetectingPlanes)
        {
            Touch touch = Input.GetTouch(index: 0);
            Vector2 touchPosition = touch.position;
            ARRaycasting(touchPosition);
        }
        // Input check for PC builds
        else if (Input.GetMouseButtonDown(0) && isDetectingPlanes)
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
            
            // Server call to create the workspace plane
            InstantiateWorkspaceServerRpc(pose.position,pose.rotation,planeSize, NetworkManager.Singleton.LocalClientId);
            planeManager.enabled = false;       // Disables the ARPlane Manager
            isDetectingPlanes = false;

            trackingManager.CleanTrackables();  // Cleans all the other detected planes
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
        drag.Reset();
    }

    //------------------------------------------------------------------------------------------------------
    // Network Functions
    //------------------------------------------------------------------------------------------------------

    /**
     *  Creates a workspace plane based on the size of the selected detected plane
     *  @param planePosition Vector3 representing the position of the detected plane selected
     *  @param planeRotation Quaternion representing the rotation of the detected plane selected
     */
    [ServerRpc(RequireOwnership = false)]
    void InstantiateWorkspaceServerRpc(Vector3 planePosition, Quaternion planeRotation, Vector2 planeSize, ulong clientId)
    {
        // Instantiates the workspace and scales it
        currentWorkspaceInstance = Instantiate(workspacePrefab, planePosition, planeRotation);
        Vector2 workspaceSize = new Vector2(currentWorkspaceInstance.GetComponent<Renderer>().bounds.size.x, currentWorkspaceInstance.GetComponent<Renderer>().bounds.size.z);
        Vector3 ratioSize = new Vector3(planeSize.x / workspaceSize.x, 1.0f, planeSize.y / workspaceSize.y);
        currentWorkspaceInstance.transform.localScale = ratioSize;

        // Spawns the workspace in the room
        NetworkObject worspaceNetworkObject = currentWorkspaceInstance.GetComponent<NetworkObject>();
        worspaceNetworkObject.SpawnWithOwnership(clientId);

        // Setups the configuration menu
        isConfiguringWorkspace = true;
        drag = currentWorkspaceInstance.GetComponent<DragWorkspace>();
        drag.SetOnConfig(true);
        SetConfigState(POSITIONXZ_STATE);
        uiManager.WorkspaceConfiguration();
    }
}