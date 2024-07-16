using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragWorkspace : MonoBehaviour
{
    [SerializeField] private float horizontalTresshold;
    [SerializeField] private float verticalTresshold;

    private WorkspaceConfig config;
    private float initialDragPosition;

    private bool hasClicked = false;
    
    // Start is called before the first frame update
    void Start()
    {
        config = GameObject.Find("ARConfig").GetComponentInChildren<WorkspaceConfig>();
    }

    void OnMouseDrag()
    {
        int currentConfigState = config.GetCurrentConfigState();
        Debug.Log("Dragging Plane with State: " +  currentConfigState);
    }
}