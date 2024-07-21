using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragWorkspace : MonoBehaviour
{
    private const int MOUSE_MODE = 0;
    private const int TOUCH_MODE = 1;

    [SerializeField] private float movementTresshold;
    [SerializeField] private float workspacePositionChange;
    [SerializeField] private float workspaceRotationChange;
    [SerializeField] private int sensibility;

    //private WorkspaceConfig config;
    private Vector3 originalPosition;                                         // Saves the original position of the workspace prior to user configuration
    private Quaternion originalRotation;                                      // Saves the original rotation of the workspace prior to the user configuration
    private Vector3 originalScale;                                            // Saves the original scale of the workspace prior to the user configuration
    private Vector3 initialDragPosition;
    private Vector3 cameraForward;
    private int offCounter;

    private bool hasClicked = true;
    private bool movingX = false;
    private bool movingY = false;

    private UIDebuger uidebug;
    private int x = 0;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        offCounter = 0;

        uidebug = GameObject.Find("UI").GetComponentInChildren<UIDebuger>();
    }

    void Update()
    {
        if (Input.touchCount != 1)
        {
            hasClicked = true;
            movingX = false;
            movingY = false;
            return;
        }

        Touch touchData = Input.GetTouch(0);
        if(touchData.phase == TouchPhase.Began)
        {
            hasClicked = false;

            if (!hasClicked)
            {
                initialDragPosition = touchData.position;
                cameraForward = Camera.main.transform.forward;
            }
        }

        if (!hasClicked && touchData.phase == TouchPhase.Moved)
        {
            ManageInputDrag(touchData.position, TOUCH_MODE);
            x++;
        }

        if (!hasClicked && (touchData.phase == TouchPhase.Ended || touchData.phase == TouchPhase.Canceled))
        {
            hasClicked = true;
            movingX = false;
            movingY = false;
        }
    }

    void OnMouseDown()
    {
        hasClicked = false;

        if(!hasClicked)
        { 
            initialDragPosition = Input.mousePosition;
            cameraForward = Camera.main.transform.forward;
        }
    }

    void OnMouseUp()
    {
        hasClicked = true;
        movingX = false;
        movingY = false;
    }

    void OnMouseDrag()
    {
        ManageInputDrag(Input.mousePosition, MOUSE_MODE);
    }

    private void ManageInputDrag(Vector3 currentDragPosition, int inputMode)
    {
        if (offCounter <= 0)
        {
            float deltaX = initialDragPosition.x - currentDragPosition.x;
            float deltaY = initialDragPosition.y - currentDragPosition.y;

            float directionX = 0.0f;
            float directionY = 0.0f;

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

            if (inputMode == MOUSE_MODE)
                MouseMove(Camera.main.transform.forward, transform.forward, directionX, directionY);
            else
                TouchMove(Camera.main.transform.forward, transform.forward, directionX, directionY);

            offCounter = sensibility;
        }
        else
        {
            offCounter--;
        }
    }

    private void MouseMove(Vector3 cameraForward, Vector3 workspaceForward, float directionX, float directionY)
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
        else if (cameraWorkspaceAngle > 135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x - speedY, transform.position.y, transform.position.z + speedX);
            transform.position = newPosition;
        }
    }

    private void TouchMove(Vector3 cameraForward, Vector3 workspaceForward, float directionX, float directionY)
    {
        float cameraWorkspaceAngle = Vector3.SignedAngle(cameraForward, workspaceForward, Vector3.up);
        float speedX = workspacePositionChange * directionX * Time.deltaTime;
        float speedY = workspacePositionChange * directionY * Time.deltaTime;
        if (cameraWorkspaceAngle > -45.0 && cameraWorkspaceAngle <= 45.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x - speedX, originalPosition.y, originalPosition.z + speedY);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle <= -45.0 && cameraWorkspaceAngle >= -135.0)
        {
            Vector3 newPosition = new Vector3(originalPosition.x - speedY, originalPosition.y, transform.position.z + speedX);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle > 45.0 && cameraWorkspaceAngle <= 135.0)
        {
            Vector3 newPosition = new Vector3(originalPosition.x + speedY, originalPosition.y, transform.position.z - speedX);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle > 135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x + speedX, originalPosition.y, originalPosition.z + speedY);
            transform.position = newPosition;
        }
        uidebug.Log("X: " + transform.position.x + " Y: " + transform.position.y + " Z: " + transform.position.z);
    }

    public void Reset()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
    }
}