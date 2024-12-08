using System.Linq;
using UnityEngine;
using GDS.Core;
using GDS.Sample;
using GDS.Minimal;

public class GenericPickableAction : PickableAction
{
    public override void ExecuteAction(GameObject pickableObject)
    {
        // Use the name of the GameObject to identify the item
        string prefabName = pickableObject.name.Replace("(Clone)", "").Trim(); // Remove "(Clone)" suffix if instantiated

        // Map the prefab name to a BaseId
        if (!System.Enum.TryParse<BaseId>(prefabName, out var baseId))
        {
            Debug.LogError($"No BaseId found for prefab name: {prefabName}");
            return;
        }

        // Find the item base from the database
        var itemBase = DB.AllBases.FirstOrDefault(baseItem => baseItem.BaseId == baseId);

        if (itemBase != null)
        {
            // Create a new item instance
            Item newItem = new Item(
                Id: (int)baseId,  // Assuming BaseId maps directly to the ID
                ItemBase: itemBase,
                ItemData: new ItemData(Quant: 1) // Quantity is 1 by default
            );

            // Add the item to the main inventory
            bool wasAdded = Store.Instance.MainInventory.AddItem(newItem);

            if (wasAdded)
            {
                Debug.Log($"{itemBase.Name} added to inventory!");
                Destroy(pickableObject); // Remove the object from the scene
            }
            else
            {
                Debug.Log($"Failed to add {itemBase.Name} to inventory. No space.");
            }
        }
        else
        {
            Debug.LogError($"ItemBase not found for BaseId: {baseId}");
        }
    }
}
