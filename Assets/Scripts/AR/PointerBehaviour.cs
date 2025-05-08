using UnityEngine;

/**
 * PointerBehaviour is responsible for managing the lifespan of a pointer object in the AR environment.
 * It handles the pointer's lifetime and its interaction with the pointed model.
 */
public class PointerBehaviour : MonoBehaviour
{
    //--------------------------------------------------------------------------------------------------------
    // Fields
    //--------------------------------------------------------------------------------------------------------

    [SerializeField] private float lifespan;        // The lifespan of the pointer in seconds

    private float aliveFor = 0.0f;                  // The time the pointer has been alive
    private ModelData pointedModel = null;          // The model that the pointer is currently pointing at, if any

    //--------------------------------------------------------------------------------------------------------
    // MonoBehaviour Functions
    //--------------------------------------------------------------------------------------------------------

    void Start()
    {
        aliveFor = 0.0f;
    }

    void Update()
    {
        aliveFor += Time.deltaTime;
        if (aliveFor >= lifespan)
        {
            if(pointedModel != null)
                pointedModel.SetPingOn(false);

            Destroy(gameObject);
        }
    }

    //--------------------------------------------------------------------------------------------------------
    // Public Functions
    //--------------------------------------------------------------------------------------------------------

    /**
     * Asigns the model data that the pointer is currently pointing at.
     * @param model The model data to be assigned.
     */
    public void SetPointedModel(ModelData model)
    {
        pointedModel = model;
    }
}