using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public int totalItemsInChest; // Total items in this chest
    public int openedCount = 0;  // How many times this chest has been opened

    private void Awake()
    {
        // Initialize with a random number of items (1-3)
        totalItemsInChest = Random.Range(1, 4);
        Debug.Log($"Chest initialized with {totalItemsInChest} items.");
    }
}
