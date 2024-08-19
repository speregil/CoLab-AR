using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class TrackingManager : MonoBehaviour
{
    private GameObject trackables;
    private ARPlaneManager planeManager;

    // Start is called before the first frame update
    void Start()
    {
        trackables = transform.Find("Trackables").gameObject;
        planeManager = gameObject.GetComponent<ARPlaneManager>();

        if(NetworkManager.Singleton.IsHost)
            planeManager.enabled = true;
    }

    public void CleanTrackables()
    {
        foreach (Transform child in trackables.transform)
        {
            Destroy(child.gameObject);
        }
    }
}