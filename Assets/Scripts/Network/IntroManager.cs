using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [SerializeField] UIManager uiManager;
    [SerializeField] TMP_InputField nameInputField;

    private string roomName;
    private bool isHost;

    public void RoomNameEdit(string roomName)
    {
        this.roomName = roomName;
    }

    public void AcceptCreateRoom()
    {
        nameInputField.text = "";
        isHost = true;
    }

    public void CancelCreateRoom()
    {
        roomName = "";
        nameInputField.text = "";
        uiManager.CancelCreateRoom();
    }
}