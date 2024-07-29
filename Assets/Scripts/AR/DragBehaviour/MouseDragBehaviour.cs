using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Behaviour for detecting and managing the workspace configuration when interacting with a mouse
 */
public class MouseDragBehaviour : MonoBehaviour, IDragBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    [SerializeField] private float movementTresshold;                         // Determines the tresshold in wich mouse drag cahnge moves the workspace
    [SerializeField] private float workspacePositionChange;                   // Determines how much the workspace position changes when draged
    [SerializeField] private float workspaceRotationChange;                   // Determines how much the workspace rotation changes when draged 
    [SerializeField] private float workspaceScaleChange;                      // Determines how much the workspace scale changes when draged
    [SerializeField] private float minimumScale;
    [SerializeField] private int sensibility;                                 // Stablished how much sensible the movement is to the position change of the mouse while dragging

    private Vector3 originalPosition;                                         // Saves the original position of the workspace prior to user configuration
    private Quaternion originalRotation;                                      // Saves the original rotation of the workspace prior to the user configuration
    private Vector3 originalScale;                                            // Saves the original scale of the workspace prior to the user configuration

    private Vector3 initialDragPosition;                                      // Initial position of the mouse in screen when the beginin of a drag is detected
    private float directionX;                                                 // Direction in the X axis of the screen in which the drag was detected
    private float directionY;                                                 // Direction in the Y axis of the screen in which the drag was detected
    private Vector3 cameraForward;                                            // Represents the forwards direction of the camera
    private Vector3 workspaceForward;                                         // Represents de forwards direction of the workspace
    private int offCounter;                                                   // Counter that controls the sensibility of the mouse

    private int configState;                                                  // Current configuration state
    private bool onConfig = false;                                            // Flag that determines if the workspace is in configuration mode or not
    private bool hasClicked = true;                                           // Flag that determines if the mouse has completed a full click or not
    private bool movingX = false;                                             // Flag that determines if the mouse is being dragged across the X axis of the screen or not
    private bool movingY = false;                                             // Flag that determines if the mouse is being dragged across the Y axis of the screen or not

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        // Saves the originsl state of the workspace
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;

        // Detects the forward vectors of the camera and the workspace
        cameraForward = Camera.main.transform.forward;
        workspaceForward = transform.forward;

        offCounter = 0; // Initialize the sensibility control counter
    }

    void OnMouseDown()
    {
        // Saves the initial position of the mouse in the screen when a drag starts
        if (onConfig)
        {
            hasClicked = false;

            if (!hasClicked)
            {
                initialDragPosition = Input.mousePosition;
                cameraForward = Camera.main.transform.forward;
            }
        }
    }    

    void OnMouseUp()
    {
        // A drag finished and everything restarts
        if (onConfig)
        {
            hasClicked = true;
            movingX = false;
            movingY = false;
        }
    }

    void OnMouseDrag()
    {
        if (onConfig)
        {
            ManageInputDrag(Input.mousePosition);
        }
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Manage the movement of the workspace depending on the input detected from the mose and the current Configuration State
     * @param currentDragPosition Vector3 representing the current position of the mouse in screen during a drag and relative
     *  to the initial position detected.
     */
    private void ManageInputDrag(Vector3 currentDragPosition)
    {
        if (offCounter <= 0)    // Move only if not in off time due to sensibility configuration
        {
            float deltaX = initialDragPosition.x - currentDragPosition.x;
            float deltaY = initialDragPosition.y - currentDragPosition.y;

            directionX = 0.0f;
            directionY = 0.0f;

            // Calculates the direction of the movement in relation to the initial detected drag point
            if(Mathf.Abs(deltaX) > movementTresshold && !movingY)
            {
                directionX = deltaX / Mathf.Abs(deltaX);
                movingX = true;
            }
            
            if (Mathf.Abs(deltaY) > movementTresshold && !movingX)
            {
                directionY = deltaY / Mathf.Abs(deltaY);
                movingY = true;
            }

            // Moves the workspace depending on the current configuration state
            switch (configState)
            {
                case WorkspaceConfig.POSITIONXZ_STATE:
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

            offCounter = sensibility;   // Ininiates the sensibility counter
        }
        else
        {
            offCounter--;   // Moves the sensibility counter if in off time
        }
    }

    /**
     * Moves the workspace in the XZ plane in reference to the parameters calculated in ManageInputDrag
     * and the forward direction of the camera and the workspace
     */
    public void MoveXZ()
    {
        float cameraWorkspaceAngle = Vector3.SignedAngle(cameraForward, workspaceForward, Vector3.up);
        float speedX = workspacePositionChange * directionX * Time.deltaTime;
        float speedY = workspacePositionChange * directionY * Time.deltaTime;

        if (cameraWorkspaceAngle > -45.0 && cameraWorkspaceAngle <= 45.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x + speedY, transform.position.y, transform.position.z - speedX);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle <= -45.0 && cameraWorkspaceAngle >= -135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x - speedX, transform.position.y, transform.position.z - speedY);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle > 45.0 && cameraWorkspaceAngle <= 135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x + speedX, transform.position.y, transform.position.z + speedY);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle < -135.0 || cameraWorkspaceAngle > 135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x - speedY, transform.position.y, transform.position.z + speedX);
            transform.position = newPosition;
        }
    }

    /**
     * Moves the workspace up and down along the Y axis
     */
    public void MoveY()
    {
        float speedY = workspacePositionChange * directionY * Time.deltaTime;
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y - speedY, transform.position.z);
        transform.position = newPosition;
    }

    /**
     * Rotates the workspace around the Y axis
     */
    public void Rotate()
    {
        float rotationSpeed = workspaceRotationChange * directionX * Time.deltaTime;
        float rotationY = transform.eulerAngles.y - rotationSpeed;

        if (rotationY > 360.0f || rotationY < -360.0f)
            rotationY = 0.0f;

        Vector3 newRotation = new Vector3(transform.eulerAngles.x, rotationY, transform.eulerAngles.z);
        transform.eulerAngles = newRotation;
    }

    /**
     * Scales the workspace along the XZ plane and in reference to the forward direction of the camera and the workspace.
     */
    public void Scale()
    {
        float cameraWorkspaceAngle = Vector3.SignedAngle(cameraForward, workspaceForward, Vector3.up);
        float speedX = workspacePositionChange * directionX * Time.deltaTime;
        float speedY = workspacePositionChange * directionY * Time.deltaTime;
        float scaleX = transform.localScale.x - speedX;
        float scaleY = transform.localScale.y - speedY;

        if(scaleX < minimumScale)
            scaleX = minimumScale;
        if(scaleY < minimumScale)
            scaleY = minimumScale;

        if (cameraWorkspaceAngle > -45.0 && cameraWorkspaceAngle <= 45.0)
        {
            Vector3 newScale = new Vector3(scaleX, transform.localScale.y, scaleY);
            transform.localScale = newScale;
        }
        else if (cameraWorkspaceAngle <= -45.0 && cameraWorkspaceAngle >= -135.0)
        {
            Vector3 newScale = new Vector3(scaleY, transform.localScale.y, scaleX);
            transform.localScale = newScale;
        }
        else if (cameraWorkspaceAngle > 45.0 && cameraWorkspaceAngle <= 135.0)
        {
            Vector3 newScale = new Vector3(scaleY, transform.localScale.y, scaleX);
            transform.localScale = newScale;
        }
        else if (cameraWorkspaceAngle < -135.0 || cameraWorkspaceAngle > 135.0)
        {
            Vector3 newScale = new Vector3(scaleX, transform.localScale.y, scaleY);
            transform.localScale = newScale;
        }
    }

    /**
     * Resets all changes to the workspace to the initial one detected when the prefab was first instantieted
     */
    public void ResetToInitial()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
    }

    /**
     * Turns the configuration behaviour of the workspace on and off
     * @param onConfig bool that runs the configuration behaviour on or off
     */
    public void SetOnConfig(bool onConfig)
    {
        this.onConfig = onConfig;
    }

    public void SetConfigState(int state)
    {
        this.configState = state;
    }
}