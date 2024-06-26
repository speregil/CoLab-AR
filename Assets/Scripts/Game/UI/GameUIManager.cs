using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameUIManager : NetworkBehaviour
{
    [SerializeField] private Canvas createGameCanvas;
    [SerializeField] private Canvas controllerCanvas;

    // Start is called before the first frame update
    void Start()
    {
        ShowCreateGameCanvas();
    }

    void ShowCreateGameCanvas()
    {
        createGameCanvas.gameObject.SetActive(true);
        controllerCanvas.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        createGameCanvas.gameObject.SetActive(false);
        controllerCanvas.gameObject.SetActive(true);
    }
}
