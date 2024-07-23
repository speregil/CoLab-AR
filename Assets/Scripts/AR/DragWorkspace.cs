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
    [SerializeField] private float workspaceScaleChange;
    [SerializeField] private int sensibility;

    private Vector3 originalPosition;                                         // Saves the original position of the workspace prior to user configuration
    private Quaternion originalRotation;                                      // Saves the original rotation of the workspace prior to the user configuration
    private Vector3 originalScale;                                            // Saves the original scale of the workspace prior to the user configuration
    private Vector3 initialDragPosition;
    private Vector3 cameraForward;
    private int offCounter;

    private int configState;
    private bool onConfig = false;
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
        if (onConfig && Input.touchCount > 0)
        {
            Touch touchData = Input.GetTouch(0);
            if (touchData.phase == TouchPhase.Began)
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
    }

    void OnMouseDown()
    {
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
            ManageInputDrag(Input.mousePosition, MOUSE_MODE);
        }
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

            switch (configState)
            {
                case WorkspaceConfig.POSITIONXZ_STATE:
                    if (inputMode == MOUSE_MODE)
                        MouseMoveXZ(Camera.main.transform.forward, transform.forward, directionX, directionY);
                    else
                        TouchMoveXZ(Camera.main.transform.forward, transform.forward, directionX, directionY);
                    break;
                case WorkspaceConfig.POSITIONY_STATE:
                    MoveY(directionY);
                    break;
                case WorkspaceConfig.ROTATION_STATE:
                    Rotate(directionX);
                    break;
                case WorkspaceConfig.SCALE_STATE:
                    if (inputMode == MOUSE_MODE)
                        MouseScale(Camera.main.transform.forward, transform.forward, directionX, directionY);
                    else
                        TouchScale(directionX, directionY);
                    break;
            }

            offCounter = sensibility;
        }
        else
        {
            offCounter--;
        }
    }

    private void MouseMoveXZ(Vector3 cameraForward, Vector3 workspaceForward, float directionX, float directionY)
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

    private void TouchMoveXZ(Vector3 cameraForward, Vector3 workspaceForward, float directionX, float directionY)
    {
        float cameraWorkspaceAngle = Vector3.SignedAngle(cameraForward, workspaceForward, Vector3.up);
        float speedX = workspacePositionChange * directionX * Time.deltaTime;
        float speedY = workspacePositionChange * directionY * Time.deltaTime;
        if (cameraWorkspaceAngle > -45.0 && cameraWorkspaceAngle <= 45.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x - speedX, transform.position.y, transform.position.z + speedY);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle <= -45.0 && cameraWorkspaceAngle >= -135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x - speedY, transform.position.y, transform.position.z + speedX);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle > 45.0 && cameraWorkspaceAngle <= 135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x + speedY, transform.position.y, transform.position.z - speedX);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle < -135.0 || cameraWorkspaceAngle > 135.0)
        {
            Vector3 newPosition = new Vector3(transform.position.x + speedX, transform.position.y, transform.position.z + speedY);
            transform.position = newPosition;
        }
        uidebug.Log("X: " + transform.position.x + " Y: " + transform.position.y + " Z: " + transform.position.z);
    }

    private void MoveY(float directionY)
    {
        float speedY = workspacePositionChange * directionY * Time.deltaTime;
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y - speedY, transform.position.z);
        transform.position = newPosition;
    }

    private void Rotate(float directionX)
    {
        float rotationSpeed = workspaceRotationChange * directionX * Time.deltaTime;
        Vector3 newRotation = new Vector3(transform.eulerAngles.x,transform.eulerAngles.y - rotationSpeed, transform.eulerAngles.z);
        transform.eulerAngles = newRotation;
    }

    private void TouchRotate(float directionX)
    {
        Debug.Log("Rotate");
    }

    private void MouseScale(Vector3 cameraForward, Vector3 workspaceForward, float directionX, float directionY)
    {
        float cameraWorkspaceAngle = Vector3.SignedAngle(cameraForward, workspaceForward, Vector3.up);
        float speedX = workspacePositionChange * directionX * Time.deltaTime;
        float speedY = workspacePositionChange * directionY * Time.deltaTime;

        if (cameraWorkspaceAngle > -45.0 && cameraWorkspaceAngle <= 45.0)
        {
            Vector3 newScale = new Vector3(transform.localScale.x - speedX, transform.localScale.y, transform.localScale.z - speedY);
            transform.localScale = newScale;
        }
        else if (cameraWorkspaceAngle <= -45.0 && cameraWorkspaceAngle >= -135.0)
        {
            Vector3 newScale = new Vector3(transform.localScale.x - speedY, transform.localScale.y, transform.localScale.z - speedX);
            transform.localScale = newScale;
        }
        else if (cameraWorkspaceAngle > 45.0 && cameraWorkspaceAngle <= 135.0)
        {
            Vector3 newScale = new Vector3(transform.localScale.x - speedY, transform.localScale.y, transform.localScale.z - speedX);
            transform.localScale = newScale;
        }
        else if (cameraWorkspaceAngle < -135.0 || cameraWorkspaceAngle > 135.0)
        {
            Vector3 newScale = new Vector3(transform.localScale.x - speedX, transform.localScale.y, transform.localScale.z - speedY);
            transform.localScale = newScale;
        }
    }

    private void TouchScale(float directionX, float directionY)
    {
        Debug.Log("Scale");
    }

    public void Reset()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
    }

    public void SetOnConfig(bool onConfig)
    {
        this.onConfig = onConfig;
    }

    public void SetConfigState(int state)
    {
        this.configState = state;
    }
}