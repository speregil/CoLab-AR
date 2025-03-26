using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using Unity.VisualScripting;

/**
 * Behaviour that manages the the plane detection functions using for the room configuration
 */
public class TrackingManager : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Constants
    //------------------------------------------------------------------------------------------------------

    // Constants for the model types to add to the scene
    public const int ADD_CUBE_MODEL = 0;
    public const int ADD_BOX_MODEL = 1;
    public const int ADD_PYRAMID_MODEL = 2;
    public const int ADD_SPHERE_MODEL = 3;
    public const int ADD_CYLINDER_MODEL = 4;

    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private GameObject cubeModelPrefab;
    [SerializeField] private GameObject cubeModelPrefabPrev;
    [SerializeField] private GameObject boxModelPrefab;
    [SerializeField] private GameObject boxModelPrefabPrev;
    [SerializeField] private GameObject pyramidModelPrefab;
    [SerializeField] private GameObject pyramidModelPrefabPrev;
    [SerializeField] private GameObject sphereModelPrefab;
    [SerializeField] private GameObject sphereModelPrefabPrev;
    [SerializeField] private GameObject cylinderModelPrefab;
    [SerializeField] private GameObject cylinderModelPrefabPrev;

    private GameObject trackables;              // Parent gameobject for all the AR planes detected
    private ARPlaneManager planeManager;        // Reference to the ARPlaneManager component
    private ARAnchorManager anchorManager;      // Reference to the ARAnchorManager component
    private ARRaycastManager raycastManager;    // Reference to the ARRaycastManager component

    private Pose hitPose;                       // Pose of the hit point of the raycast
    private bool onAddModel = false;            // Flag to know if the user is currently adding a model to the scene
    private GameObject toAddModelPrev;          // Reference to the model being added to the scene
    private GameObject toAddModel;
    private GameObject currentPreview = null;
    

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
                hitPose = hits[0].pose;
                Debug.Log(hits[0].trackable.gameObject.tag);
                if (hits[0].trackable.gameObject.tag.Equals("selected"))
                {
                    if (currentPreview == null)
                    {
                        currentPreview = Instantiate(toAddModelPrev, hitPose.position, Quaternion.identity);
                        float height = currentPreview.GetComponent<MeshRenderer>().bounds.size.y;
                        currentPreview.transform.position = new Vector3(hitPose.position.x, hitPose.position.y + height / 2, hitPose.position.z);
                    }
                    else
                        currentPreview.transform.position = hitPose.position;
                }
            }
            else
            {
                Destroy(currentPreview);
                currentPreview = null;
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

    public void ChangeToAddModel(int modelType)
    {
        Destroy(currentPreview);
        currentPreview = null;

        switch (modelType)
        {
            case ADD_CUBE_MODEL:
                toAddModelPrev = cubeModelPrefabPrev;
                toAddModel = cubeModelPrefab;
                break;
            case ADD_BOX_MODEL:
                toAddModelPrev = boxModelPrefabPrev;
                toAddModel = boxModelPrefab;
                break;
            case ADD_PYRAMID_MODEL:
                toAddModelPrev = pyramidModelPrefabPrev;
                toAddModel = pyramidModelPrefab;
                break;
            case ADD_SPHERE_MODEL:
                toAddModelPrev = sphereModelPrefabPrev;
                toAddModel = sphereModelPrefab;
                break;
            case ADD_CYLINDER_MODEL:
                toAddModelPrev = cylinderModelPrefabPrev;
                toAddModel = cylinderModelPrefab;
                break;
        }
    }

    public void ActivateModelPositioning(bool isActive)
    {
        onAddModel = isActive;
    }

    public void AddModel()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
            currentPreview = null;

            GameObject model = Instantiate(toAddModel, hitPose.position, Quaternion.identity);
            float height = model.GetComponent<MeshRenderer>().bounds.size.y;
            model.transform.position = new Vector3(hitPose.position.x, hitPose.position.y + height / 2, hitPose.position.z);
            NetworkObject networkModel = model.GetComponent<NetworkObject>();
            networkModel.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);
        }
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