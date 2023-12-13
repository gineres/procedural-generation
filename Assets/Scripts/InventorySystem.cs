using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    // Dictionary to store inventory items with their positions
    private Dictionary<Vector3, int> inventory = new Dictionary<Vector3, int>();

    void Start()
    {
        // Example of adding items to the inventory
        AddItem(new Vector3(1, 0, 1), 10);
        AddItem(new Vector3(2, 0, 2), 5);
        AddItem(new Vector3(0, 0, 0), 3);

        // Example of accessing and displaying inventory contents
        DisplayInventory();

        // Example of clearing the inventory
        ClearInventory();

        // Display the inventory after clearing
        DisplayInventory();
    }

    void AddItem(Vector3 position, int quantity)
    {
        // Check if the position is already in the inventory
        if (inventory.ContainsKey(position))
        {
            // If so, add the quantity to the existing quantity
            inventory[position] += quantity;
        }
        else
        {
            // If not, add a new entry with the specified quantity
            inventory.Add(position, quantity);
        }
    }

    void RemoveItem(Vector3 position, int quantity)
    {
        // Check if the position is in the inventory
        if (inventory.ContainsKey(position))
        {
            // Subtract the quantity from the existing quantity
            inventory[position] -= quantity;

            // Remove the entry if the quantity becomes zero or negative
            if (inventory[position] <= 0)
            {
                inventory.Remove(position);
            }
        }
        // Optionally handle the case where the position is not in the inventory
    }

    void DisplayInventory()
    {
        // Iterate through the inventory and print the contents
        foreach (var entry in inventory)
        {
            Debug.Log("Position: " + entry.Key + ", Quantity: " + entry.Value);
        }
    }

    void ClearInventory()
    {
        // Clear the inventory
        inventory.Clear();
        Debug.Log("Inventory cleared.");
    }
}
