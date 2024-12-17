using System.Collections;
using UnityEngine;
using GDS.Minimal;
using GDS.Core;
using GDS.Sample;
using System;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryUI; // Reference to the inventory UI GameObject
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private ArmorInventory armorInventory;

    private bool isInventoryOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        if (isInventoryOpen && Input.GetKeyDown(KeyCode.Q))
        {
            DropDraggedItem();
        }

        if (isInventoryOpen && Input.GetMouseButtonDown(1))
        {
            HandleRightClick();
        }

    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryUI.SetActive(isInventoryOpen); // Show/hide the inventory UI
        Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isInventoryOpen;
    }



    private void HandleRightClick()
    {
        var draggedItem = Store.Instance.DraggedItem.Value; // Access the actual item from the Observable
        Debug.Log("Right-click detected");

        if (draggedItem == Item.NoItem)
        {
            Debug.Log("No item is currently being dragged.");
            return;
        }

        switch (draggedItem.ItemBase.ItemClass)
        {
            case GDS.Sample.ItemClass.BodyArmor:
            case GDS.Sample.ItemClass.Boots:
            case GDS.Sample.ItemClass.Helmet:
                armorInventory.EquipArmor(draggedItem);
                Store.Instance.DraggedItem.SetValue(Item.NoItem); // Remove item from inventory after equipping
                armorInventory.DisplayEquippedArmor();
                break;

            case GDS.Sample.ItemClass.Consumable:
                HandleConsumable(draggedItem);
                break;

            default:
                Debug.Log("Dragged item is of another type.");
                break;
        }
    }

    private void HandleConsumable(Item draggedItem)
    {
        Debug.Log("Dragged item is consumable.");

        // Convert ItemBase.Id (string) to BaseId (enum)
        if (Enum.TryParse<BaseId>(draggedItem.ItemBase.Id, out var baseId))
        {
            if (ConsumableManager.Effects.TryGetValue(baseId, out var effect))
            {
                Debug.Log($"This consumable will restore {effect.health} HP and {effect.stamina} Stamina.");

                UpdateItemQuantity(draggedItem);

                // Update the player's health
                playerHealth.IncreaseHealth(effect.health);
            }
            else
            {
                Debug.Log("No effect data found for this consumable.");
            }
        }
        else
        {
            Debug.LogError($"Failed to parse '{draggedItem.ItemBase.Id}' as a BaseId.");
        }
    }

    private void UpdateItemQuantity(Item draggedItem)
    {
        // Create a new ItemData with the updated quantity
        var newItemData = draggedItem.ItemData with { Quant = draggedItem.ItemData.Quant - 1 };

        // If the quantity is less than or equal to 0, set the item to NoItem
        if (newItemData.Quant <= 0)
        {
            Store.Instance.DraggedItem.SetValue(Item.NoItem);
        }
        else
        {
            // Otherwise, update the item with the new quantity
            Store.Instance.DraggedItem.SetValue(draggedItem with { ItemData = newItemData });
        }
    }

    private void DropDraggedItem()
    {
        var draggedItem = Store.Instance.DraggedItem.Value;

        if (draggedItem == Item.NoItem)
        {
            Debug.Log("No item is currently being dragged.");
            return;
        }

        // Get the prefab associated with the dragged item's ID
        var prefab = PrefabRegistry.Instance?.GetPrefab(draggedItem.ItemBase.Id);
        if (prefab == null)
        {
            Debug.LogError($"No prefab found for item: {draggedItem.ItemBase.Name}");
            return;
        }

        // Determine the drop position
        Vector3 dropPosition = transform.position + transform.forward;

        // Generate a random rotation
        Quaternion randomRotation = Quaternion.Euler(
            UnityEngine.Random.Range(0f, 360f), // Random X rotation
            UnityEngine.Random.Range(0f, 360f), // Random Y rotation
            UnityEngine.Random.Range(0f, 360f)  // Random Z rotation
        );

        // Instantiate the prefab at the desired position with random rotation
        GameObject droppedObject = Instantiate(prefab, dropPosition, randomRotation);

        // Set the tag and layer for the dropped object
        droppedObject.tag = "Pickable"; // Set the tag to "Pickable"
        droppedObject.layer = LayerMask.NameToLayer("PickableObjects"); // Set the layer to "PickableObjects"

        // Set the Y-coordinate to 2.5 to ensure correct height
        droppedObject.transform.position = new Vector3(droppedObject.transform.position.x, 1.5f, droppedObject.transform.position.z);

        droppedObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        // Add required components to the instantiated object
        if (!droppedObject.TryGetComponent<Rigidbody>(out _))
        {
            var rb = droppedObject.AddComponent<Rigidbody>();
            rb.mass = 1f; // Set mass or other Rigidbody properties if needed
        }

        // Check if the prefab has a BoxCollider, and apply its values if it does
        var prefabCollider = prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
            Debug.Log("Collider found");
            // Copy the BoxCollider properties from the prefab to the instantiated object
            BoxCollider droppedObjectCollider = droppedObject.GetComponent<BoxCollider>();
            if (droppedObjectCollider == null)
            {
                droppedObjectCollider = droppedObject.AddComponent<BoxCollider>();
            }

            droppedObjectCollider.center = prefabCollider.center;
            droppedObjectCollider.size = prefabCollider.size;
        }
        else
        {
            Debug.Log("No collider found");
            // If the prefab doesn't have a BoxCollider, add a default one
            var collider = droppedObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.3f, 0.3f, 0.3f); // Adjust the size as necessary
        }

        // Add GenericPickableAction component if it's not already attached
        if (!droppedObject.TryGetComponent<GenericPickableAction>(out _))
        {
            droppedObject.AddComponent<GenericPickableAction>();
        }



        var newItemData = draggedItem.ItemData with { Quant = draggedItem.ItemData.Quant - 1 };

        UpdateItemQuantity(draggedItem);

        Debug.Log($"Dropped item: {draggedItem.ItemBase.Name}, with components: Rigidbody, BoxCollider, GenericPickableAction, Random Rotation");
    }





}
