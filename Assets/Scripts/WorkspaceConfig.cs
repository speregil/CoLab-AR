using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/**
 * Behaviour for creating the workspace plane of a room and configure it
 */
public class WorkspaceConfig : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private GameObject workspacePrefab;                      // Prefab for the workspace plane      

    private ARRaycastManager raycastManager;                                  // Reference to the ARRaycastManager component
    private ARPlaneManager planeManager;                                      // Reference to the ARPlaneManager component

    private GameObject currentWorkspaceInstance;                              // Current instance of the workspace plane
    private bool isDetectingPlanes = true;                                    // Flag that determines if the script is currently tracking planes or not

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
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
    void ARRaycasting(Vector2 position)
    {
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        if (raycastManager.Raycast(position, hits, trackableTypes: UnityEngine.XR.ARSubsystems.TrackableType.PlaneEstimated))
        {
            InstantiateWorkspace(hits[0]);      // Creates the workspace plane
            planeManager.enabled = false;       // Disables the ARPlane Manager
            isDetectingPlanes = false;
            CleanDetectedPlanes();              // Cleans all the detected planes
        }
    }

   /**
    *  Creates a workspace plane based on the size of the selected detected plane
    *  @param planeHit ARRaycastHit hit to a detected plane registered by the RaycastManager 
    */
   void InstantiateWorkspace(ARRaycastHit planeHit)
    {
        Pose pose = planeHit.pose;
        currentWorkspaceInstance = Instantiate(workspacePrefab, pose.position, pose.rotation);

        // Calculates the size ratio between the workspace prefab and the detected plane
        Vector2 planeSize = new Vector2(planeHit.trackable.gameObject.GetComponent<Renderer>().bounds.size.x, planeHit.trackable.gameObject.GetComponent<Renderer>().bounds.size.z);
        Vector2 workspaceSize = new Vector2(currentWorkspaceInstance.GetComponent<Renderer>().bounds.size.x, currentWorkspaceInstance.GetComponent<Renderer>().bounds.size.z);
        Vector3 ratioSize = new Vector3(planeSize.x/workspaceSize.x,1.0f, planeSize.y/workspaceSize.y);

        // Scales the instance of the workspace plane
        currentWorkspaceInstance.transform.localScale = ratioSize;
    }

    /**
     * Destroy all the detected planes in the trackables object
     */
    void CleanDetectedPlanes()
    {
        Transform trackables = transform.Find("Trackables");
        foreach (Transform child in trackables)
        {
            Destroy(child.gameObject);
        }
    }
}