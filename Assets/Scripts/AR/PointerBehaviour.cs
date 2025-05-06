using Unity.Netcode;
using UnityEngine;

public class PointerBehaviour : MonoBehaviour
{
    [SerializeField] private float lifespan;
    private NetworkObject no;
    private float aliveFor = 0.0f;

    void Start()
    {
        no = GetComponent<NetworkObject>();
        aliveFor = 0.0f;
    }

    void Update()
    {
        
        aliveFor += Time.deltaTime;
        if (aliveFor >= lifespan)
        {
            Destroy(gameObject);
        }
    }
}