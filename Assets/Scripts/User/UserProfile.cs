using System;
using System.Collections;
using System.Collections.Generic;

/**
 * Simple clas to storage all the option for the user personalization and configuration, storaged locally in the user's machine
 */
public class UserProfile
{
    //------------------------------------------------------------------------------------
    // Fields
    //------------------------------------------------------------------------------------

    public string username { get; set; }            // User's selected name to show to other participants
    public float[] userColor { get; set; }            // User's selected color to show to other paticipants, represented as a 3 int RGB array

    //------------------------------------------------------------------------------------
    // Constructor
    //------------------------------------------------------------------------------------
    public UserProfile()
    {
        // Initialize default parameters for the profile 
        username = "Default User";
        userColor = new float[3];
        userColor[0] = 150.0f;
        userColor[1] = 150.0f;
        userColor[2] = 150.0f;
    }
}