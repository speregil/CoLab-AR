using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingManager : MonoBehaviour
{
    private GameObject trackables;

    // Start is called before the first frame update
    void Start()
    {
        trackables = transform.Find("Trackables").gameObject;
    }

    public void CleanTrackables()
    {
        foreach (Transform child in trackables.transform)
        {
            Destroy(child.gameObject);
        }
    }
}