using System.Collections;
using UnityEngine;
using GDS.Minimal;
using GDS.Core;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryUI; // Reference to the inventory UI GameObject

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
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        inventoryUI.SetActive(isInventoryOpen); // Show/hide the inventory UI
        Cursor.lockState = isInventoryOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isInventoryOpen;
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
            Random.Range(0f, 360f), // Random X rotation
            Random.Range(0f, 360f), // Random Y rotation
            Random.Range(0f, 360f)  // Random Z rotation
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

        // Reset the dragged item
        Store.Instance.DraggedItem.SetValue(Item.NoItem);

        Debug.Log($"Dropped item: {draggedItem.ItemBase.Name}, with components: Rigidbody, BoxCollider, GenericPickableAction, Random Rotation");
    }





}
