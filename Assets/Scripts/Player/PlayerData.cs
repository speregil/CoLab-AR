using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientID;
    public bool playerPlaced;

    public PlayerData(ulong clientID, bool playerPlaced)
    {
        this.clientID = clientID;
        this.playerPlaced = playerPlaced;
    }

    public bool Equals(PlayerData other)
    {
        return other.clientID == clientID && other.playerPlaced == playerPlaced;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientID);
        serializer.SerializeValue(ref playerPlaced);
    }
}
