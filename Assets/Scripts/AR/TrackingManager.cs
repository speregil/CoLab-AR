using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

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
    [SerializeField] private Material normalModelMaterial;
    [SerializeField] private Material selectedMaterial;

    [SerializeField] private UIManager uiManager;
    private GameObject trackables;              // Parent gameobject for all the AR planes detected
    private ARPlaneManager planeManager;        // Reference to the ARPlaneManager component
    private ARAnchorManager anchorManager;      // Reference to the ARAnchorManager component
    private ARRaycastManager raycastManager;    // Reference to the ARRaycastManager component

    private Vector3 hitPosition;                // Pose of the hit point of the raycast
    private bool onAddModel = false;            // Flag to know if the user is currently adding a model to the scene
    private bool onDeleteModel = false;         // Flag to know if the user is currently deleting a model from the scene
    private bool onMoveModel = false;
    private GameObject toAddModelPrev;          // Reference to the model being added to the scene
    private int toAddModel;
    private GameObject currentPreview = null;
    private GameObject currentSelection = null;
    private Vector3 rayOrigin = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    


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
        if(onAddModel || onDeleteModel || onMoveModel) { 
            rayOrigin = uiManager.GetCrosshairPosition();
            TrackingModelsRaycast();
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
                toAddModel = modelType;
                break;
            case ADD_BOX_MODEL:
                toAddModelPrev = boxModelPrefabPrev;
                toAddModel = modelType;
                break;
            case ADD_PYRAMID_MODEL:
                toAddModelPrev = pyramidModelPrefabPrev;
                toAddModel = modelType;
                break;
            case ADD_SPHERE_MODEL:
                toAddModelPrev = sphereModelPrefabPrev;
                toAddModel = modelType;
                break;
            case ADD_CYLINDER_MODEL:
                toAddModelPrev = cylinderModelPrefabPrev;
                toAddModel = modelType;
                break;
        }
    }

    public GameObject GetToAddModel(int modelType)
    {
        switch (modelType)
        {
            case ADD_CUBE_MODEL:
                return cubeModelPrefab;
            case ADD_BOX_MODEL:
                return boxModelPrefab;
            case ADD_PYRAMID_MODEL:
                return pyramidModelPrefab;
            case ADD_SPHERE_MODEL:
                return sphereModelPrefab;
            case ADD_CYLINDER_MODEL:
                return cylinderModelPrefab;
            default:
                return null;
        }
    }

    public void ActivateModelPositioning(bool isActive)
    {
        onAddModel = isActive;

        if (!isActive) resetModelsRaycast();
    }

    public void ActivateModelDeletion(bool isActive)
    {
        onDeleteModel = isActive;
        if (!isActive) resetModelsRaycast();
    }

    private void TrackingModelsRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(rayOrigin);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.gameObject.tag.Equals("workspace") || hit.transform.gameObject.tag.Equals("model"))
            {
                hitPosition = hit.point;
                if (onAddModel) { 
                    if (currentPreview == null)
                    {
                        currentPreview = Instantiate(toAddModelPrev, hitPosition, Quaternion.identity);
                        float height = currentPreview.GetComponent<MeshRenderer>().bounds.size.y;
                        currentPreview.transform.position = new Vector3(hitPosition.x, hitPosition.y + height / 2, hitPosition.z);
                    }
                    else
                        currentPreview.transform.position = hitPosition;
                }
                else if(hit.transform.gameObject.tag.Equals("model")) 
                {
                    currentSelection = hit.transform.gameObject;
                    currentSelection.GetComponent<MeshRenderer>().material = selectedMaterial;
                }
                else {
                    if(currentSelection != null)
                    {
                        currentSelection.GetComponent<MeshRenderer>().material = normalModelMaterial;
                        currentSelection = null;
                    }
                }
            }
        }
        else
        {
            Destroy(currentPreview);
            currentPreview = null;
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

    public int GetToAddModelType()
    {
        return currentPreview != null ? toAddModel : -1;
    }

    public Vector3 GetHitPosition()
    {
        return hitPosition;
    }

    public GameObject GetCurrentSelection()
    {
        return currentSelection;
    }

    private void resetModelsRaycast()
    {
        rayOrigin = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        if (currentPreview != null) Destroy(currentPreview);
        currentPreview = null;
        toAddModelPrev = null;
        currentSelection = null;
    }
}