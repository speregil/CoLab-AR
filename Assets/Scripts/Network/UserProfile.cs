using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserProfile
{
    [SerializeField] private string username;
    private Color userColor;

    public UserProfile()
    {
        this.username = "New Profile";
        userColor = Color.grey;
    }

    public string GetUsername() { return username; }
    public void SetUsername(string username) { this.username = username; }
    public Color GetUserColor() { return userColor; }
    public void SetUserColor(Color color) { userColor = color; }

    public bool LoadProfile() 
    { 
        username = PlayerPrefs.GetString("username");
        float r = PlayerPrefs.GetFloat("r");
        float g = PlayerPrefs.GetFloat("g");
        float b = PlayerPrefs.GetFloat("b");
        userColor = new Color(r, g, b);
        return true; 
    }

    public bool SaveProfile() 
    { 
        PlayerPrefs.SetString("username", username);
        PlayerPrefs.SetFloat("r", userColor.r);
        PlayerPrefs.SetFloat("g", userColor.g);
        PlayerPrefs.SetFloat("b", userColor.b);
        return true; 
    }
}