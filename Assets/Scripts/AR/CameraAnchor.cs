using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraAnchor : NetworkBehaviour
{
    private GameObject mainCamera;

    override public void OnNetworkSpawn()
    {
        mainCamera = Camera.main.gameObject;
        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = mainCamera.transform.position;
        transform.rotation = mainCamera.transform.rotation;
    }
}