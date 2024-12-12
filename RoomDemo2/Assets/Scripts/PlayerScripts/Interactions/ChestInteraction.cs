using UnityEngine;
using TMPro;
using GDS.Core;
using GDS.Sample;
using GDS.Minimal;

public class ChestInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera; // Assign your player's camera in the Inspector
    [SerializeField] private float interactionDistance = 3f; // Range for interaction
    [SerializeField] private TextMeshProUGUI chestHintTextUI; // Hint text for "Press E to open"
    [SerializeField] private TextMeshProUGUI chestUI; // Actual chest UI shown after interaction
    [SerializeField] private LayerMask openableLayer;

    private bool isUIVisible = false;


    void Start()
    {

        // Ensure the UI starts hidden
        chestHintTextUI.text = "";
        chestHintTextUI.gameObject.SetActive(false);
        chestUI.text = "";
        chestUI.gameObject.SetActive(false);
        Debug.LogWarning("Chest interaction script initialized.");
    }

    void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Debug ray for visualization
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance, Color.green);

        // Raycast logic
        if (Physics.Raycast(ray, out hit, interactionDistance, openableLayer))
        {

            Chest chest = hit.collider.GetComponent<Chest>();

            if (chest != null && !isUIVisible)
            {
                // Show hint text
                chestHintTextUI.text = "Press E to open the chest";
                chestHintTextUI.gameObject.SetActive(true);

                // Check for interaction key (Enter key)
                if (Input.GetKeyDown(KeyCode.E))
                {
                    OpenChest(chest);
                }
            }
            else
            {
                // Hide hint text if not looking at a chest
                chestHintTextUI.text = "";
                chestHintTextUI.gameObject.SetActive(false);
            }
        }
        else
        {
            // Hide the hint text if the raycast hits nothing
            chestHintTextUI.text = "";
            chestHintTextUI.gameObject.SetActive(false);
        }

        // Hide chest UI if the player starts walking
        if (isUIVisible && IsWalking())
        {
            HideChestText();
        }
    }

    private void OpenChest(Chest chest)
    {
        Debug.Log("Chest opened!");

        // Hide hint text
        chestHintTextUI.text = "";
        chestHintTextUI.gameObject.SetActive(false);

        // Check if there are items left in the chest
        if (chest.openedCount < chest.totalItemsInChest)
        {
            // Pick a random item from the database
            var randomItemBase = DB.AllBases[Random.Range(0, DB.AllBases.Count)];

            // Create a new item instance
            Item newItem = new Item(
                Id: GDS.Core.ItemFactory.Id(),
                ItemBase: randomItemBase,
                ItemData: new ItemData(Quant: 1)
            );

            // Add the item to the main inventory
            bool wasAdded = Store.Instance.MainInventory.AddItem(newItem);

            if (wasAdded)
            {
                Debug.Log($"{randomItemBase.Name} added to inventory!");
                chestUI.text = $"You found a {randomItemBase.Name}!";
            }
            else
            {
                Debug.Log($"Failed to add {randomItemBase.Name} to inventory. No space.");
                chestUI.text = "Your inventory is full!";
            }

            // Increment the counter
            chest.openedCount++;
        }
        else
        {
            // No more items in the chest
            Debug.Log("The chest is empty.");
            chestUI.text = "The chest is empty.";
        }

        // Show chest UI temporarily
        chestUI.gameObject.SetActive(true);
        Invoke(nameof(HideChestText), 2f); // Hide UI after 2 seconds
    }

    void HideChestText()
    {
        chestUI.text = "";
        chestUI.gameObject.SetActive(false); // Hide chest UI
        isUIVisible = false;
    }

    bool IsWalking()
    {
        // Check for WASD key presses
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
    }
}
