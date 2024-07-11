using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [SerializeField] TMP_InputField createNameInputField;
    [SerializeField] TMP_InputField joinNameInputField;

    UIManager uiManager;

    private string roomName;
    private bool isHost;

    private void Start()
    {
        uiManager = GetComponent<UIManager>();
    }

    public bool IsHost()
    {
        return isHost;
    }

    public string GetRoomName() 
    { 
        return roomName; 
    }

    public void RoomNameEdit(string roomName)
    {
        this.roomName = roomName;
    }

    public void AcceptCreateRoom()
    {
        createNameInputField.text = "";
        isHost = true;
        uiManager.AcceptCreateRoom();
    }

    public void AcceptJoinRoom()
    {
        joinNameInputField.text = "";
        isHost = false;
        uiManager.AcceptJoinRoom();
    }

    public void CancelCreateRoom()
    {
        roomName = "";
        createNameInputField.text = "";
        uiManager.CancelCreateRoom();
    }

    public void CancelJoinRoom()
    {
        roomName = "";
        joinNameInputField.text = "";
        uiManager.CancelJoinRoom();
    }
}