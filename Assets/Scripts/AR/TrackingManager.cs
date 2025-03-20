using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;

/**
 * Behaviour that manages the the plane detection functions using for the room configuration
 */
public class TrackingManager : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    private GameObject trackables;              // Parent gameobject for all the AR planes detected
    private ARPlaneManager planeManager;        // Reference to the ARPlaneManager component
    private ARAnchorManager anchorManager;      // Reference to the ARAnchorManager component
    private ARRaycastManager raycastManager;    // Reference to the ARRaycastManager component

    private bool onAddModel = false;            // Flag to know if the user is currently adding a model to the scene

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        trackables = transform.Find("Trackables").gameObject;
        planeManager = GetComponent<ARPlaneManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        anchorManager.anchorsChanged += OnAnchorsChanged;
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (onAddModel)
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            Ray ray = Camera.main.ScreenPointToRay(screenCenter);
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(ray, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                Debug.Log("Hit pose: " + hitPose.position);
            }   
        }
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Activates or deactives the ARPlaneManager behaviour. Activation can only happen if the current client
     * is the room's host
     */
    public void EnableTracking(bool isAble)
    {
        if (NetworkManager.Singleton.IsHost)
            planeManager.enabled = isAble;
    }

    /**
     * Destroys all the planes detected and tracked by the ARPlaneManager behaviour, leaving the Trakables
     * node empty
     */
    public void CleanDetectedPlanes()
    {
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
    }

    public void AddAnchor(GameObject anchorObject) 
    {
        anchorObject.AddComponent<ARAnchor>();
    }

    public void ActivateModelPositioning(bool isActive)
    {
        onAddModel = isActive;
    }

    private void OnAnchorsChanged(ARAnchorsChangedEventArgs args)
    {
        Debug.Log("Anchors changed event");
        foreach (var anchor in args.added)
        {
            Debug.Log("Anchor added: " + anchor.gameObject.name);
        }
    }
}