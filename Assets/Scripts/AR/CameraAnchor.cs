using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraAnchor : NetworkBehaviour
{
    [SerializeField] private SessionManager sessionManager;
    [SerializeField] private GameObject nameplateCanvas;

    private GameObject mainCamera;

    override public void OnNetworkSpawn()
    {
        nameplateCanvas.GetComponent<Canvas>().worldCamera = Camera.main;
        mainCamera = Camera.main.gameObject;

        if (!IsOwner) return;

        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(nameplateCanvas.activeInHierarchy)
            nameplateCanvas.transform.LookAt(mainCamera.transform);

        if (!IsOwner) return;

        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
    }
}