using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100; // Maximum health
    [SerializeField] private Slider healthSlider; // Slider to display health

    private int currentHealth;

    public int getCurrentHealth()
    {
        return currentHealth; 
    }


    private void Start()
    {
        // Initialize health
        currentHealth = maxHealth;

        // Ensure the slider is properly configured
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void Update()
    {
        /*// Decrease health by 5 when "D" key is pressed
        if (Input.GetKeyDown(KeyCode.D))
        {
            DecreaseHealth(5);
        }*/
    }

    public void DecreaseHealth(int amount)
    {
        currentHealth -= amount;

        // Clamp health to a minimum of 0
        currentHealth = Mathf.Max(currentHealth, 0);

        // Update the slider
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log($"Player health decreased by {amount}. Current health: {currentHealth}");
    }

    public void IncreaseHealth(int amount)
    {
        
        currentHealth += amount;

        // Clamp health to a maximum of maxHealth
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // Update the slider
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        Debug.Log($"Player health increased by {amount}. Current health: {currentHealth}");
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
