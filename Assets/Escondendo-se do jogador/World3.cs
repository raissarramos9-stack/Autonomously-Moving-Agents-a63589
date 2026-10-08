using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class World3
{
    private static readonly World3 instance = new World3();

    private static GameObject[] hidingSpots;

    static World3()
    {
        hidingSpots = GameObject.FindGameObjectsWithTag("hide");
    }

    private World3()
    {

    }

    // Tem de ser PUBLIC
    public static World3 Instance
    {
        get { return instance; }
    }

    public GameObject[] GetHidingSpots()
    {
        return hidingSpots;
    }
}