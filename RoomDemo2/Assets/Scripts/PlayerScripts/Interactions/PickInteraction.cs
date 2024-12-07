using UnityEngine;
using TMPro;

public class PickInteraction : MonoBehaviour
{
    [SerializeField] private Camera playerCamera; // Assign your player's camera in the Inspector
    [SerializeField] private float interactionDistance = 1f;
    [SerializeField] private TextMeshProUGUI interactionText; // Assign the TextMeshPro element in the Inspector
    [SerializeField] private LayerMask pickableLayer; // Layer for pickable objects


    // Action to be executed when the object is picked up
    [SerializeField] private PickableAction pickableAction;

    void Start()
    {
        // Ensure the text starts hidden
        interactionText.text = "";
    }

    void Update()
    {
        // Perform a raycast to detect objects, using the specified layer mask
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Debug ray for visualizing in the scene
        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactionDistance, Color.red);

        // Check if the ray hits a collider within the interaction distance and with the pickableLayer
        if (Physics.Raycast(ray, out hit, interactionDistance, pickableLayer))
        {
            // Check if the object has the "Pickable" tag
            if (hit.collider.CompareTag("Pickable"))
            {
                interactionText.text = "Press E to pick up"; // Show interaction prompt

                // Check for the interaction key
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Perform the action assigned to this pickable object
                    if (pickableAction != null)
                    {
                        pickableAction.ExecuteAction(hit.collider.gameObject); // Execute the specific action for this object
                    }
                }
            }
        }
        else
        {
            // Clear the text if no object is hit
            interactionText.text = "";
        }
    }

    // Method to assign the custom action for the pickable object
    public void SetPickableAction(PickableAction action)
    {
        pickableAction = action;
    }
}
