using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject introMenu;
    [SerializeField] GameObject roomMenu;

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void CreateRoom()
    {
        introMenu.SetActive(false);
        roomMenu.SetActive(true);
    }

    public void AcceptCreateRoom()
    {
        roomMenu.SetActive(false);
        SceneManager.LoadScene(sceneName: "Main");
    }

    public void CancelCreateRoom()
    {
        introMenu.SetActive(true);
        roomMenu.SetActive(false);
    }
}