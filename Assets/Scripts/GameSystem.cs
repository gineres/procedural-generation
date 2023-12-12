using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public CaveMapGenerator caveMapGenerator;
    public Transform playerTransform;
    public float searchRadius = 5f;

    bool breakLoop;
    string mapSeed = Vector3.zero.ToString();

    void Update()
    {
        CheckForPortalsInVisibleRadius();
    }

    void CheckForPortalsInVisibleRadius(){
        Collider[] colliders = Physics.OverlapSphere(playerTransform.position, searchRadius);

        if (colliders.Length >= 3)
        {
            breakLoop = true;
        } else
        {
            breakLoop = false;
        }

        // Iterate through the colliders to get the objects of interest
        foreach (Collider collider in colliders)
        {
            // Check if the collider's GameObject has a specific tag or component
            if (collider.CompareTag("Portal"))
            {
                // Access the GameObject or perform actions based on your requirements
                GameObject foundObject = collider.gameObject;
                //Debug.Log("Found object with tag: " + foundObject.name);
                string currentMapSeed = foundObject.transform.position.ToString();
                if (currentMapSeed == mapSeed)
                {
                    break;
                }
                mapSeed = currentMapSeed;
                caveMapGenerator.seed = mapSeed;
                caveMapGenerator.GenerateMap();
                if (breakLoop)
                {
                    break;
                }
            }
        }   
    }
}
