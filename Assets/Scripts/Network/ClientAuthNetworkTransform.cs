using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

/**
 * Simple override script to configure the network transform of an object as client authoritative
 */
[DisallowMultipleComponent]

public class ClientAuthNetworkTransform : NetworkTransform
{
    /**
     * Overrides this object as client authoritative
     */
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}