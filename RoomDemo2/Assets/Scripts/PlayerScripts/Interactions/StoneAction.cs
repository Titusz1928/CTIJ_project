using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GDS.Core;
using GDS.Sample;
using GDS.Minimal;
using System.Linq;

public class StoneAction : PickableAction
{
    public override void ExecuteAction(GameObject pickableObject)
    {
        var mainInventory = Store.Instance.MainInventory;

        Debug.Log("Stone picked up");

        // Retrieve the Stone item from the database
        var stoneBase = DB.AllBases.FirstOrDefault(baseItem => baseItem.BaseId == BaseId.Stone);

        if (stoneBase != null)
        {
            // Rename the second stoneBase variable to avoid conflict
            ItemBase newStoneBase = new ItemBase(
                Id: BaseId.Stone.ToString(),  // Assuming BaseId.Stone is an enum or similar value that can be converted to string
                Name: "Stone",
                IconPath: "Shared/Icons/stone",
                Stackable: true,
                Size: new Size(1, 1)  // Adjust Size as needed
            );

            ItemData stoneData = new ItemData(Quant: 1);  // Set Quant to 1 (or any other quantity)

            // Now create the Item using the constructor
            var stoneItem = new Item(
                Id: (int)BaseId.Stone,  // Cast BaseId.Stone to int if necessary
                ItemBase: newStoneBase,
                ItemData: stoneData
            );

            // Try adding the stone item to the player's main inventory
            bool wasAdded = mainInventory.AddItem(stoneItem);

            if (wasAdded)
            {
                Debug.Log("Stone added to inventory!");
                // Destroy the pickable object
                Destroy(pickableObject);
            }
            else
            {
                Debug.Log("Failed to add Stone to inventory. No space.");
            }
        }
        else
        {
            Debug.LogError("Stone item not found in the database!");
        }
    }
}
