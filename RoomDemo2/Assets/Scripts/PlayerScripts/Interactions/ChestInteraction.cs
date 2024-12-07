using UnityEngine;
using TMPro;

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
            if (hit.collider.CompareTag("Chest") && !isUIVisible)
            {
                // Show hint text
                chestHintTextUI.text = "Press E to open the chest";
                chestHintTextUI.gameObject.SetActive(true);

                // Check for interaction key (Enter key)
                if (Input.GetKeyDown(KeyCode.E))
                {
                    ShowChestText();
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

    void ShowChestText()
    {
        Debug.Log("Chest opened!");
        chestHintTextUI.text = ""; // Hide hint text
        chestHintTextUI.gameObject.SetActive(false);

        chestUI.text = "You opened the chest!";
        chestUI.gameObject.SetActive(true); // Show chest UI
        isUIVisible = true;
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
