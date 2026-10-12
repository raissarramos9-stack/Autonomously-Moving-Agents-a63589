using UnityEditor;
using UnityEngine;

public sealed class World4 { 

    private static readonly World4 instance = new World4();
    private static GameObject[] hidingSpots;

    static World4() {

        hidingSpots = GameObject.FindGameObjectsWithTag("hide");
    }

    private World4() { }

    public static World4 Instance {  get { return instance; } }

    public GameObject[] GetHidingSpots() { return hidingSpots; }
}