using UnityEngine;
using Unity.Netcode;
using TMPro;

public class CameraAnchor : NetworkBehaviour
{
    [SerializeField] private GameObject mainBody;
    [SerializeField] private GameObject gaze;
    [SerializeField] private GameObject nameplate;
    
    private SessionManager sessionManager;
    private GameObject mainCamera;

    override public void OnNetworkSpawn()
    {
        sessionManager = GetComponent<SessionManager>();
        nameplate.GetComponent<Canvas>().worldCamera = Camera.main;
        mainCamera = Camera.main.gameObject;

        if (!IsOwner) return;

        gaze.SetActive(false);
        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
        
        base.OnNetworkSpawn();
    }

    // Update is called once per frame
    void Update()
    {
        if(nameplate.activeInHierarchy)
            nameplate.transform.LookAt(mainCamera.transform);

        if (!IsOwner) return;

        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
    }

    public void ApplyAnchorColor(Color participantColor)
    {
        Material mainMaterial = mainBody.GetComponent<Renderer>().material;
        mainMaterial.SetColor("_Color", normalizeColor(participantColor));
    }

    public void UpdateAnchorNameplate(string username)
    {
        TMP_Text label = nameplate.transform.Find("Panel").gameObject.GetComponentInChildren<TMP_Text>();
        label.text = username;
    }

    private Color normalizeColor(Color baseColor)
    {
        Color normalizedColor = new Color();
        normalizedColor.r = baseColor.r / 255;
        normalizedColor.g = baseColor.g / 255;
        normalizedColor.b = baseColor.b / 255;
        return normalizedColor;
    }
}