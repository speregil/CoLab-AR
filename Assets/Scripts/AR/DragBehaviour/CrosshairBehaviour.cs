using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosshairBehaviour : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Sprite[] imageOption;

    private InputAction touchPress;
    private InputAction touchPosition;

    private bool onTouch = false;

    private Vector3 currentPosition;
    private Vector3 previousPosition;
    private Image currentImage;
    private bool isBouncy = false;

    private void Awake()
    {
        touchPress = playerInput.actions["TouchPress"];
        touchPosition = playerInput.actions["TouchPosition"];
    }

    private void Start()
    {
        gameObject.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        currentPosition = gameObject.transform.position;
        previousPosition = gameObject.transform.position;
    }

    private void OnEnable()
    {
        currentImage = GetComponent<Image>();
    }

    void Update()
    {
        if(touchPress.WasPerformedThisFrame()) onTouch = true;

        if (touchPress.WasCompletedThisFrame())
        {
            if(isBouncy)
                ResetCrosshair();
            else 
            {
                previousPosition = currentPosition;
                onTouch = false;
            }
        }

        if (onTouch)
        {
            Vector2 position = touchPosition.ReadValue<Vector2>();
            Debug.Log(uiManager.IsPositionOnButton(position));
            if (!uiManager.IsPositionOnButton(position))
            {
                previousPosition = currentPosition;
                currentPosition = position;
                gameObject.transform.position = currentPosition;
            }
        }
    }

    public Vector3 GetCurrentPosition()
    {
        return currentPosition;
    }

    public Vector3 GetDiffPosition()
    {
        return new Vector3(currentPosition.x - previousPosition.x, currentPosition.y - previousPosition.y, 0);
    }

    public void SetAsBouncy(bool bouncy)
    {
        isBouncy = bouncy;
    }

    public void SetImage(int index)
    {
        if (index < imageOption.Length)
           currentImage.sprite = imageOption[index];
    }

    public void ResetCrosshair()
    {
       gameObject.transform.position = new Vector3(Screen.width / 2, Screen.height / 2, 0);
       currentPosition = gameObject.transform.position;
       previousPosition = gameObject.transform.position;
       onTouch = false;
    }
}