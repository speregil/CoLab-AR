using UnityEngine;

/**
 * Behaviour for managing the workspace configuration when the host is creating a room
 */
public class DragBehaviour : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private float workspacePositionChange;                   // Determines how much the workspace position changes when the crosshair moves
    [SerializeField] private float workspaceRotationChange;                   // Determines how much the workspace rotation changes when the crosshair moves
    [SerializeField] private float workspaceScaleChange;                      // Determines how much the workspace scale changes when the crosshair moves
    [SerializeField] private float minimumScale;                              // Minimum value the scale transformation decreses to

    private Vector3 originalPosition;                                         // Saves the original position of the workspace prior to user configuration
    private Quaternion originalRotation;                                      // Saves the original rotation of the workspace prior to the user configuration
    private Vector3 originalScale;                                            // Saves the original scale of the workspace prior to the user configuration
    private Vector3 previousPosition;

    private int configState;                                                  // Current configuration state
    private bool onConfig = false;                                            // Flag that determines if the workspace is in configuration mode or not

    private UIManager uiManager;                                              // Reference to the UIManager script to access the UI elements

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        // Saves the original state of the workspace
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
    }

    void Update()
    {
        if (onConfig)
        {
            switch (configState)
            {
                case WorkspaceConfig.POSITIONXZ_STATE:
                    previousPosition = transform.position;
                    MoveXZ();
                    break;
                case WorkspaceConfig.POSITIONY_STATE:
                    MoveY();
                    break;
                case WorkspaceConfig.ROTATION_STATE:
                    Rotate();
                    break;
                case WorkspaceConfig.SCALE_STATE:
                    Scale();
                    break;
            }
        }
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Moves the workspace in the XZ plane in reference to the parameters calculated in ManageInputDrag
     * and the forward direction of the camera and the workspace
     */
    public void MoveXZ()
    { 
        if(uiManager.GetCrosshairDiffPosition() != Vector3.zero)
        { 
            Ray ray = Camera.main.ScreenPointToRay(uiManager.GetCrosshairPosition());
            Vector3 rayPoint = ray.GetPoint(2);
            Vector3 diffPosition = rayPoint - previousPosition;
            transform.position = new Vector3(transform.position.x + (diffPosition.x * workspacePositionChange * Time.deltaTime), transform.position.y, transform.position.z + (diffPosition.z * workspacePositionChange * Time.deltaTime));
        }
    }

    /**
     * Moves the workspace up and down along the Y axis
     */
    public void MoveY()
    {
        Vector3 diffPosition = uiManager.GetCrosshairDiffPosition();
        transform.position = new Vector3(transform.position.x, transform.position.y + (diffPosition.y * workspacePositionChange * Time.deltaTime), transform.position.z);
    }

    /**
     * Rotates the workspace around the Y axis
     */
    public void Rotate()
    {
        Vector3 diffPosition = uiManager.GetCrosshairDiffPosition();
        float rotationY = transform.eulerAngles.y + (diffPosition.x * workspaceRotationChange * Time.deltaTime);
        Vector3 newRotation = new Vector3(transform.eulerAngles.x, rotationY, transform.eulerAngles.z);
        transform.eulerAngles = newRotation;
    }

    /**
     * Scales the workspace along the XZ plane and in reference to the forward direction of the camera and the workspace.
     */
    public void Scale()
    {   
        Vector3 diffPosition = uiManager.GetCrosshairDiffPosition();
        float scaleX = transform.localScale.x + (diffPosition.x * workspaceScaleChange * Time.deltaTime);
        float scaleZ = transform.localScale.z + (diffPosition.y * workspaceScaleChange * Time.deltaTime);
        scaleX = scaleX < minimumScale ? minimumScale : scaleX;
        scaleZ = scaleZ < minimumScale ? minimumScale : scaleZ;
        transform.localScale = new Vector3(scaleX, transform.localScale.y, scaleZ);
    }

    /**
     * Resets all changes to the workspace to the initial one detected when the prefab was first instantieted
     */
    public void ResetToInitial()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
        previousPosition = transform.position;
    }

    /**
     * Turns the configuration behaviour of the workspace on and off
     * @param onConfig bool that runs the configuration behaviour on or off
     */
    public void SetOnConfig(bool onConfig)
    {
        this.onConfig = onConfig;
    }

    /**
     * Sets the current configuration state of the workspace
     * @param state int that represents the current configuration state of the workspace
     */
    public void SetConfigState(int state)
    {
        configState = state;
    }

    /**
     * Sets the UIManager reference to access the UI elements
     * @param uiManager UIManager reference to access the UI elements
     */
    public void SetUIManager(UIManager uiManager)
    {
        this.uiManager = uiManager;
    }
}