using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragWorkspace : MonoBehaviour
{
    [SerializeField] private float movementTresshold;
    [SerializeField] private float workspacePositionChange;
    [SerializeField] private float workspaceRotationChange;
    [SerializeField] private int sensibility;

    private WorkspaceConfig config;

    private Vector3 initialDragPosition;
    private Vector3 cameraForward;
    private int offCounter;

    private bool hasClicked = true;
    
    // Start is called before the first frame update
    void Start()
    {
        config = GameObject.Find("ARConfig").GetComponentInChildren<WorkspaceConfig>();
        offCounter = 0;
    }

    void Update()
    {
        if(Input.touchCount > 0) 
        {
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
            else if(touchData.phase == TouchPhase.Ended)
                hasClicked = true;
            else if(touchData.phase == TouchPhase.Moved)
            {
                ManageInputDrag(touchData.position);
            }
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
        ManageInputDrag(Input.mousePosition);
    }

    private void ManageInputDrag(Vector3 currentDragPosition)
    {
        if (offCounter == 0)
        {
            float deltaX = initialDragPosition.x - currentDragPosition.x;
            float direction = deltaX / Mathf.Abs(deltaX);

            if (Mathf.Abs(deltaX) > movementTresshold)
            {
                Move(Camera.main.transform.forward, transform.forward, direction);
                offCounter = sensibility;
            }
        }
        else
        {
            offCounter--;
        }
    }

    private void Move(Vector3 cameraForward, Vector3 workspaceForward, float direction)
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
}