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

    private UIDebuger uidebug;

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
            return;
        }

        Touch touchData = Input.GetTouch(0);
        if(touchData.phase == TouchPhase.Began)
        {
            hasClicked = false;
            initialDragPosition = touchData.position;
            cameraForward = Camera.main.transform.forward;
        }

        if (!hasClicked && touchData.phase == TouchPhase.Moved)
        {
            ManageInputDrag(touchData.position, TOUCH_MODE);
        }

        if (!hasClicked && (touchData.phase == TouchPhase.Ended || touchData.phase == TouchPhase.Canceled))
        {
            hasClicked = true;
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
            float direction = deltaX / Mathf.Abs(deltaX);

            if (Mathf.Abs(deltaX) > movementTresshold)
            {
                if (inputMode == MOUSE_MODE)
                    MouseMove(Camera.main.transform.forward, transform.forward, direction);
                else
                    TouchMove(Camera.main.transform.forward, transform.forward, direction);
                
                offCounter = sensibility;
            }
        }
        else
        {
            offCounter--;
        }
    }

    private void MouseMove(Vector3 cameraForward, Vector3 workspaceForward, float direction)
    {
        float cameraWorkspaceAngle = Vector3.SignedAngle(cameraForward, workspaceForward, Vector3.up);
        if (cameraWorkspaceAngle > -45.0 && cameraWorkspaceAngle <= 45.0 ) 
        {
            Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z - (workspacePositionChange * direction));
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle <= -45.0 && cameraWorkspaceAngle >= -135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x - (workspacePositionChange * direction), transform.position.y, transform.position.z);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle > 45.0 && cameraWorkspaceAngle <= 135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x + (workspacePositionChange * direction), transform.position.y, transform.position.z);
            transform.position = newPosition;
        }
        else if(cameraWorkspaceAngle > 135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z + (workspacePositionChange * direction));
            transform.position = newPosition;
        }
    }

    private void TouchMove(Vector3 cameraForward, Vector3 workspaceForward, float direction)
    {
        float cameraWorkspaceAngle = Vector3.SignedAngle(cameraForward, workspaceForward, Vector3.up);
        if (cameraWorkspaceAngle > -45.0 && cameraWorkspaceAngle <= 45.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x - (workspacePositionChange * direction), originalPosition.y, originalPosition.z);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle <= -45.0 && cameraWorkspaceAngle >= -135.0)
        {
            Vector3 newPosition = new Vector3(originalPosition.x, originalPosition.y, transform.position.z + (workspacePositionChange * direction));
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle > 45.0 && cameraWorkspaceAngle <= 135.0)
        {
            Vector3 newPosition = new Vector3(originalPosition.x, originalPosition.y, transform.position.z - (workspacePositionChange * direction));
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle > 135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x + (workspacePositionChange * direction), originalPosition.y, originalPosition.z);
            transform.position = newPosition;
        }
        uidebug.Log(transform.position.x + ", " + transform.position.y + ", " + transform.position.z);
    }

    public void Reset()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
    }
}