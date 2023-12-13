using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSystem : MonoBehaviour
{
    public GameObject entrancePortal;

    public CaveMapGenerator caveMapGenerator;
    public Transform playerTransform;
    public float searchRadius = 5f;

    bool breakLoop;
    string mapSeed = Vector3.zero.ToString();

    private Dictionary<Vector3, GameObject> itemsOnScreen = new Dictionary<Vector3, GameObject>();
    private Dictionary<Vector3, GameObject> inventory = new Dictionary<Vector3, GameObject>();

    public Dictionary<Vector3, GameObject> GetItemsOnScreen() {
        return itemsOnScreen;
    }
    public Dictionary<Vector3, GameObject> GetInventory() {
        return inventory;
    }

    public void AddItem(Vector3 position, GameObject gameObject, bool isInventory)
    {
        if (isInventory)
        {
            if (!inventory.ContainsKey(position))
            {
                inventory.Add(position, gameObject);
            }
            return;
        }
        if (!itemsOnScreen.ContainsKey(position))
        {
            itemsOnScreen.Add(position, gameObject);
        }
    }

    public void RemoveItem(Vector3 position, bool isInventory)
    {
        if (isInventory)
        {
            if (inventory.ContainsKey(position))
            {
                inventory.Remove(position);
            }
        }
        if (itemsOnScreen.ContainsKey(position))
        {
            itemsOnScreen.Remove(position);
        }
    }

    public void ClearObjects()
    {
        itemsOnScreen.Clear();
    }
    
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
