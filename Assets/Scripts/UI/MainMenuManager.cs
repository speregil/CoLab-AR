using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject openButton;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject participantMenu;

    private TMP_Text openButtonLbl;

    // Start is called before the first frame update
    void Start()
    {
        openButtonLbl = openButton.transform.GetComponentInChildren<TMP_Text>();
    }

    public void OpenMainMenu()
    {
        mainMenu.SetActive(true);
        openButtonLbl.text = "Close";
    }

    public void OpenParticipantsMenu()
    {
        mainMenu.SetActive(false);
        participantMenu.SetActive(true);
        openButtonLbl.text = "Back";
    }

    public void CloseMainMenu()
    {
        mainMenu.SetActive(false);
        openButtonLbl.text = "Open";
    }

    public void CloseParticipantsMenu()
    {
        mainMenu.SetActive(true);
        participantMenu.SetActive(false);
        openButtonLbl.text = "Close";
    }
}