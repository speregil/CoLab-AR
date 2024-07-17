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

    public const int POSITIONXZ_STATE = 0;
    public const int POSITIONY_STATE = 1;
    public const int ROTATION_STATE = 2;
    public const int SCALE_STATE = 3;

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
    private Vector3 originalPosition;                                         // Saves the original position of the workspace prior to user configuration
    private Quaternion originalRotation;                                      // Saves the original rotation of the workspace prior to the user configuration
    private Vector3 originalScale;                                            // Saves the original scale of the workspace prior to the user configuration
    private int currentConfigState;

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

    public bool IsConfiguringWorkspace()
    {
        return isDetectingPlanes;
    }

    public int GetCurrentConfigState()
    {
        return currentConfigState;
    }

    public void SetConfigState(int state)
    {
        currentConfigState = state;
    }

    /**
     * Signals the end of the workspace configurations and asks the UIManager to hide the menu
     */
    public void FinishConfiguration()
    {
        isConfiguringWorkspace = false;
        uiManager.AcceptWorkspaceConfiguration();
    }

    public void ResetConfiguration()
    {
        currentWorkspaceInstance.transform.position = originalPosition;
        currentWorkspaceInstance.transform.rotation = originalRotation;
        currentWorkspaceInstance.transform.localScale = originalScale;
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
        originalPosition = currentWorkspaceInstance.transform.localPosition;
        originalRotation = currentWorkspaceInstance.transform.localRotation;
        originalScale = currentWorkspaceInstance.transform.localScale;
        isConfiguringWorkspace = true;
        uiManager.WorkspaceConfiguration();
    }
}