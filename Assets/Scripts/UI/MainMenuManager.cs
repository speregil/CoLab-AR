using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainButton;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject participantMenu;
    [SerializeField] private GameObject participantLabelPrefab;

    private SessionManager sessionManager;

    private TMP_Text mainButtonLbl;
    private Button mainButtonBtn;
    

    // Start is called before the first frame update
    void Start()
    {
        mainButtonLbl = mainButton.transform.GetComponentInChildren<TMP_Text>();
        mainButtonBtn = mainButton.GetComponentInChildren<Button>();

    }

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        mainButtonLbl.text = "Close";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseMainMenu);
    }

    public void OpenParticipantsMenu()
    {
        mainMenu.SetActive(false);
        participantMenu.SetActive(true);
        mainButtonLbl.text = "Back";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseParticipantsMenu);

        List<string> participants = sessionManager.GetParticipantsList();
        if (participants != null)
        {
            foreach (string participant in participants)
            {
                AddParticipantToList(participant);
            }
        }
        else
            Debug.Log("Invalid list");

    }

    private void AddParticipantToList(string username)
    {
        GameObject labelInstance = GameObject.Instantiate(participantLabelPrefab);
        TMP_Text usernameLbl = labelInstance.GetComponentInChildren<TMP_Text>();
        usernameLbl.text = username;
        labelInstance.transform.SetParent(participantMenu.transform, false);
    }

    public void CloseMainMenu()
    {
        mainMenu.SetActive(false);
        mainButtonLbl.text = "Open";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(OpenMainMenu);
    }

    public void CloseParticipantsMenu()
    {
        foreach (Transform child in participantMenu.transform)
        {
            Destroy(child.gameObject);
        }

        mainMenu.SetActive(true);
        participantMenu.SetActive(false);
        mainButtonLbl.text = "Close";
        mainButtonBtn.onClick.RemoveAllListeners();
        mainButtonBtn.onClick.AddListener(CloseMainMenu);
    }

    public void SetSessionManager(SessionManager sessionManager)
    {
        this.sessionManager = sessionManager;
    }
}