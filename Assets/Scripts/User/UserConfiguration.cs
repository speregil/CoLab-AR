using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Behaviour that controls the local parameters of a user and the information that is independet of
 * a particular network session
 */
public class UserConfiguration : MonoBehaviour
{
    //-------------------------------------------------------------------------------------
    // Fields
    //-------------------------------------------------------------------------------------

    private UserProfile profile;                // Profile data of the user storaged in the local machine

    //-------------------------------------------------------------------------------------
    // MonoBehaviour Functions
    //-------------------------------------------------------------------------------------

    void Start()
    {
        profile = new UserProfile();
        LoadProfile();
    }

    //-------------------------------------------------------------------------------------
    // Functions
    //-------------------------------------------------------------------------------------

    /**
     * Obtains the user's current username storaged in the profile
     * @return string Username storaged in the profile, a default value if there is none
     */
    public string GetUsername()
    {
        return profile.username;
    }

    /**
     * Obtains the user's current selected color storaged in the profile
     * @return Color A color representation selected by the user, a default grey if there is none
     */
    public Color GetUserColor()
    {
        Color userColor = new Color();
        userColor.r = profile.userColor[0];
        userColor.g = profile.userColor[1];
        userColor.b = profile.userColor[2];
        return userColor;
    }

    /**
     * Tries to load a previous profile storaged in the local machine, leaves the default values of the profile
     * if there is no info storaged
     * @return string Error message if there was no profile in the local machine, empty otherwise 
     */
    public string LoadProfile()
    {
        string err = "";
        string username = PlayerPrefs.GetString("username", "");
        float r = PlayerPrefs.GetFloat("r", -1.0f);
        float g = PlayerPrefs.GetFloat("g", -1.0f);
        float b = PlayerPrefs.GetFloat("b", -1.0f);

        if (username != "")
            profile.username = username;
        else
            err = "User profile not found";

        if(r >= 0 && g >= 0 && b >= 0)
        {
            profile.userColor[0] = r;
            profile.userColor[1] = r;
            profile.userColor[2] = r;
        }
        else
            err = "User profile not found";

        return err;
    }

    /**
     * Updates the currentProfile with the information received as parameters and tries to save it in the
     * local machine
     * @param username Updated username of the user
     * @param userColor Updated color selected by the user
     * @return string A message detailing the result of the operation
     */
    public string UpdateAndSaveProfile(string username, Color userColor)
    {
        profile.username = username;
        profile.userColor[0] = userColor.r;
        profile.userColor[1] = userColor.g;
        profile.userColor[2] = userColor.b;

        return SaveToLocalMachine();
    }

    /**
     * Tries to save the current information in the profile in the local machine
     * @return string A message detailing the result of the operation and a console log of the error, if any
     */
    public string SaveToLocalMachine()
    {
        string msg = "Profile saved";
        try { 
            PlayerPrefs.SetString("username", profile.username);
            PlayerPrefs.SetFloat("r", profile.userColor[0]);
            PlayerPrefs.SetFloat("g", profile.userColor[1]);
            PlayerPrefs.SetFloat("b", profile.userColor[2]);
        }
        catch (Exception e)
        {
            msg = "There was a problem saving your profile";
            Debug.LogError(e.StackTrace);
        }
        return msg;
    }
}