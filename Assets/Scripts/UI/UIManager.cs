using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject introMenu;
    [SerializeField] GameObject createRoomMenu;
    [SerializeField] GameObject joinRoomMenu;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void CreateRoom()
    {
        introMenu.SetActive(false);
        createRoomMenu.SetActive(true);
    }

    public void JoinRoom()
    {
        introMenu.SetActive(false);
        joinRoomMenu.SetActive(true);
    }

    public void AcceptCreateRoom()
    {
        createRoomMenu.SetActive(false);
        SceneManager.LoadScene(sceneName: "Main");
    }

    public void AcceptJoinRoom()
    {
        joinRoomMenu.SetActive(false);
        SceneManager.LoadScene(sceneName: "Main");
    }

    public void CancelCreateRoom()
    {
        introMenu.SetActive(true);
        createRoomMenu.SetActive(false);
    }

    public void CancelJoinRoom()
    {
        introMenu.SetActive(true);
        joinRoomMenu.SetActive(false);
    }
}