using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;



[RequireComponent(typeof(PlayerInputControls))]
public class PlayerMovement : NetworkBehaviour
{
    private PlayerInputControls _playerInputControls;
    private const float MOVE_SPEED = .004f;
    private const float MOVE_THRESHOLD = 0.01f;

    public override void OnNetworkSpawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControls = GetComponent<PlayerInputControls>();
            _playerInputControls.OnMoveInput += PlayerInputControlsOnOnMoveInput;
        }
    }

    private void PlayerInputControlsOnOnMoveInput(Vector3 inputMovement)
    {
        if (inputMovement.magnitude < MOVE_THRESHOLD) return;

        transform.position += inputMovement * MOVE_SPEED;
    }

    public override void OnNetworkDespawn()
    {
        if (GetComponent<NetworkObject>().IsOwner)
        {
            _playerInputControls.OnMoveInput -= PlayerInputControlsOnOnMoveInput;
        }
    }
}