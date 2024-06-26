using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class StartMeshing : NetworkBehaviour
{
    [SerializeField] private ARMeshManager meshManager;

    // Start is called before the first frame update
    void Start()
    {
        meshManager.enabled = false;
    }

    public override void OnNetworkSpawn()
    {
        meshManager.enabled = true;
    }
}