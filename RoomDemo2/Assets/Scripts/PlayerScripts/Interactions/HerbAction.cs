using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HerbAction : PickableAction
{
    [SerializeField] public int healthIncreaseAmount = 10;  // The amount of health to increase
    [SerializeField] public GameObject replacementPrefab;    // The prefab to replace the herb with

    [SerializeField] public PlayerHealth playerhealth; // Reference to player health script

    public override void ExecuteAction(GameObject pickableObject)
    {
        // Find the player health script and increase health
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.IncreaseHealth(healthIncreaseAmount); // Heal the player
        }

        // Destroy the current herb (pickable object)
        Destroy(pickableObject);

        // Set the desired rotation
        Quaternion newRotation = Quaternion.Euler(0, 0, 180);

        // Instantiate the replacement prefab at the same position with the new rotation
        Instantiate(replacementPrefab, pickableObject.transform.position, newRotation);
    }

}
