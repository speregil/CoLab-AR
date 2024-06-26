using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerDataManager : NetworkBehaviour
{
    public static PlayerDataManager instance;
    private NetworkList<PlayerData> allPlayerData;

    private void Awake()
    {
        allPlayerData = new NetworkList<PlayerData>();

        if (instance != null && instance != this)
        {
            Destroy(instance);
        }

        instance = this;
    }

    public void AddPlacedPlayer(ulong ID)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == ID)
            {
                PlayerData newData = new PlayerData(allPlayerData[i].clientID, true);
                allPlayerData[i] = newData;
            }
        }
    }

    public bool GetHasPlayerPlaced(ulong ID)
    {
        for (int i = 0; i < allPlayerData.Count; i++)
        {
            if (allPlayerData[i].clientID == ID)
            {
                return allPlayerData[i].playerPlaced;
            }
        }
        return false;
    }

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            AddNewClientToList(NetworkManager.LocalClientId);
        }
    }

    void AddNewClientToList(ulong clientID)
    {
        if (!IsServer) return;

        foreach (var playerData in allPlayerData) {
            if (playerData.clientID == clientID) return;
        }

        PlayerData newPlayerData = new PlayerData();
        newPlayerData.clientID = clientID;
        newPlayerData.playerPlaced = false;

        if (allPlayerData.Contains(newPlayerData)) return;

        allPlayerData.Add(newPlayerData);

        PrintAllPlayerPlayerList();
    }

    void PrintAllPlayerPlayerList()
    {
        foreach (var playerData in allPlayerData)
        {
            Debug.Log("Player ID => " + playerData.clientID + " hasPlaced " + playerData.playerPlaced + " Called by " + NetworkManager.Singleton.LocalClientId);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += AddNewClientToList;
    }

    private void Update(){}
}
