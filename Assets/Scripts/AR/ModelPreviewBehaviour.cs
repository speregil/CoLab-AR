using System.Collections;
using UnityEngine;

public class ModelPreviewBehaviour : MonoBehaviour
{
    [SerializeField] private float lifespan;

    private float timeSinceSpawn;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator TrackTimeSinceSpawn()
    {
        timeSinceSpawn = 0f;

        while (true)
        {
            // Increment the time since spawn
            timeSinceSpawn += Time.deltaTime;

            // Output the time since spawn to the console
            Debug.Log("Time since spawn: " + timeSinceSpawn + " seconds");

            // Wait for the next frame
            yield return null;
        }
    }
}
