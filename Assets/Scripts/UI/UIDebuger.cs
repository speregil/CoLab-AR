using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIDebuger : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    public void Log(string msg)
    {
        text.text = msg;
    }
}