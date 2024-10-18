using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraAnchor : NetworkBehaviour
{
    [SerializeField] private SessionManager sessionManager;

    private GameObject mainCamera;

    override public void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        mainCamera = Camera.main.gameObject;
        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
    }
}