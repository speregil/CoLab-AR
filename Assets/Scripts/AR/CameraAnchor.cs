using UnityEngine;
using Unity.Netcode;
using TMPro;

/**
 * Network Behaviour that controls the camera anchor and each of its components 
 * for each participant in the room
 */
public class CameraAnchor : NetworkBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private GameObject mainBody;           // Reference to the main body that represents the a participant in the augmented space
    [SerializeField] private GameObject gaze;               // Reference to the gaze object that shows the point of view of the participant
    [SerializeField] private GameObject nameplate;          // Reference to the nameplate with the username of the participant
    
    private SessionManager sessionManager;                  // Reference to the SessionManager component
    private GameObject roomCamera;                          // Reference to the global camera in each participant's instance of the room
    private GameObject sessionCamera;                       // Reference to the own ARCamera component

    //------------------------------------------------------------------------------------------------------
    // Network Behaviour Functions
    //------------------------------------------------------------------------------------------------------

    override public void OnNetworkSpawn()
    {
        sessionManager = GetComponent<SessionManager>();
        nameplate.GetComponent<Canvas>().worldCamera = Camera.main;
        roomCamera = Camera.main.gameObject;

        if (IsOwner)
        {
            sessionCamera = sessionManager.GetMainMenuReference().GetSessionCamera();
            gaze.SetActive(false);
            transform.position = sessionCamera.transform.position;
            transform.rotation = sessionCamera.transform.rotation;
        }
    }

    void Update()
    {
        if(nameplate.activeInHierarchy)
            nameplate.transform.LookAt(roomCamera.transform);

        if (IsOwner)
        {
            transform.position = sessionCamera.transform.position;
            transform.rotation = sessionCamera.transform.rotation;
        }
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Changes the color of the main body to the one received by paramenter
     * @param particpantColor Color to apply to the main body
     */
    public void ApplyAnchorColor(Color participantColor)
    {
        Material mainMaterial = mainBody.GetComponent<Renderer>().material;
        mainMaterial.SetColor("_Color", normalizeColor(participantColor));
    }

    /**
     * Updates the username depicted in the nameplate with the one receved by parameter
     * @param username string of the username to display in the nameplate
     */
    public void UpdateAnchorNameplate(string username)
    {
        TMP_Text label = nameplate.transform.Find("Panel").gameObject.GetComponentInChildren<TMP_Text>();
        label.text = username;
    }

    /** 
     * Normalizes the color received by parameter to the Unity color space
     * @param baseColor Color to normalize
     * @return Color Normalized color
     */
    private Color normalizeColor(Color baseColor)
    {
        Color normalizedColor = new Color();
        normalizedColor.r = baseColor.r / 255;
        normalizedColor.g = baseColor.g / 255;
        normalizedColor.b = baseColor.b / 255;
        return normalizedColor;
    }
}