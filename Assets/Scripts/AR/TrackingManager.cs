using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/**
 * Behaviour that manages the the plane detection functions using for the room configuration
 */
public class TrackingManager : MonoBehaviour
{
    //------------------------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------------------------

    private GameObject trackables;          // Parent gameobject for all the AR planes detected
    private ARPlaneManager planeManager;    // Reference to the ARPlaneManager beheviour

    //------------------------------------------------------------------------------------------------------
    // Monobehaviour Functions
    //------------------------------------------------------------------------------------------------------

    void Start()
    {
        trackables = transform.Find("Trackables").gameObject;
        planeManager = GetComponent<ARPlaneManager>();
    }

    //------------------------------------------------------------------------------------------------------
    // Functions
    //------------------------------------------------------------------------------------------------------

    /**
     * Activates or deactives the ARPlaneManager behaviour. Activation can only happen if the current client
     * is the room's host
     */
    public void EnableTracking(bool isAble)
    {
        if (NetworkManager.Singleton.IsHost)
            planeManager.enabled = isAble;
    }

    /**
     * Destroys all the planes detected and tracked by the ARPlaneManager behaviour, leaving the Trakables
     * node empty
     */
    public void CleanTrackables()
    {
        foreach (Transform child in trackables.transform)
        {
            Destroy(child.gameObject);
        }
    }
}