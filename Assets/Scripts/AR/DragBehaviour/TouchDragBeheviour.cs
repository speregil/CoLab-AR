using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchDragBeheviour : MonoBehaviour, IDragBehaviour
{
    [SerializeField] private float movementTresshold;
    [SerializeField] private float workspacePositionChange;
    [SerializeField] private float workspaceRotationChange;
    [SerializeField] private float workspaceScaleChange;
    [SerializeField] private float minimumScale;
    [SerializeField] private int sensibility;
    
    private Vector3 originalPosition;                                         // Saves the original position of the workspace prior to user configuration
    private Quaternion originalRotation;                                      // Saves the original rotation of the workspace prior to the user configuration
    private Vector3 originalScale;                                            // Saves the original scale of the workspace prior to the user configuration
    private Vector3 initialDragPosition;
    private Vector3 cameraForward;
    private Vector3 workspaceForward;
    private float directionX;
    private float directionY;
    private int offCounter;

    private int configState;
    private bool onConfig = false;
    private bool hasClicked = true;
    private bool movingX = false;
    private bool movingY = false;

    // Start is called before the first frame update
    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        cameraForward = Camera.main.transform.forward;
        workspaceForward = transform.forward;
        offCounter = 0;
    }

    // Update is called once per frame
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
                ManageInputDrag(touchData.position);
            }

            if (!hasClicked && (touchData.phase == TouchPhase.Ended || touchData.phase == TouchPhase.Canceled))
            {
                hasClicked = true;
                movingX = false;
                movingY = false;
            }
        }
    }

    private void ManageInputDrag(Vector3 currentDragPosition)
    {
        if (offCounter <= 0)
        {
            float deltaX = initialDragPosition.x - currentDragPosition.x;
            float deltaY = initialDragPosition.y - currentDragPosition.y;

            directionX = 0.0f;
            directionY = 0.0f;

            if (Mathf.Abs(deltaX) > movementTresshold && !movingY)
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

            offCounter = sensibility;
        }
        else
        {
            offCounter--;
        }
    }

    public void MoveXZ()
    {
        float cameraWorkspaceAngle = Vector3.SignedAngle(cameraForward, workspaceForward, Vector3.up);
        float speedX = workspacePositionChange * directionX * Time.deltaTime;
        float speedY = workspacePositionChange * directionY * Time.deltaTime;
        if (cameraWorkspaceAngle > -45.0 && cameraWorkspaceAngle <= 45.0) //1
        {
            Vector3 newPosition = new Vector3(transform.position.x - speedX, transform.position.y, transform.position.z - speedY);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle <= -45.0 && cameraWorkspaceAngle >= -135.0)//4
        {
            Vector3 newPosition = new Vector3(transform.position.x - speedY, transform.position.y, transform.position.z + speedX);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle > 45.0 && cameraWorkspaceAngle <= 135.0)//2
        {
            Vector3 newPosition = new Vector3(transform.position.x + speedY, transform.position.y, transform.position.z - speedX);
            transform.position = newPosition;
        }
        else if (cameraWorkspaceAngle < -135.0 || cameraWorkspaceAngle > 135.0)//3
        {
            Vector3 newPosition = new Vector3(transform.position.x - speedX, transform.position.y, transform.position.z - speedY);
            transform.position = newPosition;
        }
    }
    public void MoveY()
    {
        float speedY = workspacePositionChange * directionY * Time.deltaTime;
        Vector3 newPosition = new Vector3(transform.position.x, transform.position.y - speedY, transform.position.z);
        transform.position = newPosition;
    }

    public void Rotate()
    {
        float rotationSpeed = workspaceRotationChange * directionX * Time.deltaTime;
        Vector3 newRotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y - rotationSpeed, transform.eulerAngles.z);
        transform.eulerAngles = newRotation;
    }

    public void Scale()
    {
        float cameraWorkspaceAngle = Vector3.SignedAngle(cameraForward, workspaceForward, Vector3.up);
        float speedX = workspaceScaleChange * directionX * Time.deltaTime;
        float speedY = workspaceScaleChange * directionY * Time.deltaTime;

        if (cameraWorkspaceAngle > -45.0 && cameraWorkspaceAngle <= 45.0)//3
        {
            float scaleX = transform.localScale.x - speedX;
            float scaleY = transform.localScale.z - speedY;
            scaleX = scaleX < minimumScale ? minimumScale : scaleX;
            scaleY = scaleY < minimumScale ? minimumScale : scaleY;
            Vector3 newScale = new Vector3(scaleX, transform.localScale.y, scaleY);
            transform.localScale = newScale;
        }
        else if (cameraWorkspaceAngle <= -45.0 && cameraWorkspaceAngle >= -135.0)//2
        {
            float scaleX = transform.localScale.x - speedY;
            float scaleY = transform.localScale.z - speedX;
            scaleX = scaleX < minimumScale ? minimumScale : scaleX;
            scaleY = scaleY < minimumScale ? minimumScale : scaleY;
            Vector3 newScale = new Vector3(scaleX, transform.position.y, scaleY);
            transform.localScale = newScale;
        }
        else if (cameraWorkspaceAngle > 45.0 && cameraWorkspaceAngle <= 135.0)//4
        {
            float scaleX = transform.localScale.x - speedY;
            float scaleY = transform.localScale.z - speedX;
            scaleX = scaleX < minimumScale ? minimumScale : scaleX;
            scaleY = scaleY < minimumScale ? minimumScale : scaleY;
            Vector3 newScale = new Vector3(scaleX, transform.position.y, scaleY);
            transform.localScale = newScale;
        }
        else if (cameraWorkspaceAngle < -135.0 || cameraWorkspaceAngle > 135.0) //1
        {
            float scaleX = transform.localScale.x - speedX;
            float scaleY = transform.localScale.z - speedY;
            scaleX = scaleX < minimumScale ? minimumScale : scaleX;
            scaleY = scaleY < minimumScale ? minimumScale : scaleY;
            Vector3 newScale = new Vector3(scaleX, transform.localScale.y, scaleY);
            transform.localScale = newScale;
        }
    }
    public void ResetToInitial()
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