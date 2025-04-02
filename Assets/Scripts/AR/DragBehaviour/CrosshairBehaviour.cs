using UnityEngine;

public class CrosshairBehaviour : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;

    private bool onClick = false;
    private bool onTouch = false;

    private Vector3 currentPosition;

    private void Start()
    {
        gameObject.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
    }

    void Update()
    {
        // Manage click events
        if (onClick && Input.GetMouseButtonUp(0)) onClick = false;

        if (!onClick && Input.GetMouseButtonDown(0))
        {
            onClick = true;
            if (!uiManager.IsPositionOnButton(Input.mousePosition))
            {
                currentPosition = Input.mousePosition;
                gameObject.transform.position = currentPosition;
            }
        }

        if (onClick)
        {
            if (!uiManager.IsPositionOnButton(Input.mousePosition))
            {
                currentPosition = Input.mousePosition;
                gameObject.transform.position = currentPosition;
            }
        }

        // Manage touch events
        if (onTouch && Input.touchCount == 0) onTouch = false;

        if (!onTouch && Input.touchCount > 0)
        {
            onTouch = true;
            if (!uiManager.IsPositionOnButton(Input.GetTouch(0).position))
            {
                currentPosition = Input.GetTouch(0).position;
                gameObject.transform.position = currentPosition;
            }
        }

        if (onTouch)
        {
            if (!uiManager.IsPositionOnButton(Input.GetTouch(0).position))
            {
                currentPosition = Input.GetTouch(0).position;
                gameObject.transform.position = currentPosition;
            }
        }
    }

    public Vector3 GetCurrentPosition()
    {
        return currentPosition;
    }

    public void ResetCrosshair()
    {
       gameObject.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
       onClick = false;
       onTouch = false;
    }
}