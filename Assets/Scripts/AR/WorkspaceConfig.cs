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
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private GameObject workspacePrefab;                      // Prefab for the workspace plane      

    private ARRaycastManager raycastManager;                                  // Reference to the ARRaycastManager component in the parent object
    private ARPlaneManager planeManager;                                      // Reference to the ARPlaneManager component in the parent object
    private TrackingManager trackingManager;                                  // Reference to the TrackingManager component in the parent object

    private GameObject currentWorkspaceInstance;                              // Current instance of the workspace plane
    private bool isDetectingPlanes = true;                                    // Flag that determines if the script is currently tracking planes or not

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        raycastManager = transform.parent.gameObject.GetComponent<ARRaycastManager>();
        planeManager = transform.parent.gameObject.GetComponent<ARPlaneManager>();
        trackingManager = transform.parent.gameObject.GetComponent<TrackingManager>();
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
    }
}