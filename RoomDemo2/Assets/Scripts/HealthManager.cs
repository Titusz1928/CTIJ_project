using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthSlider; // Reference to the health slider UI element

    public float CurrentHealth => currentHealth;

    void Start()
    {
        // Check if this object implements IEnemy
        IEnemy enemy = GetComponent<IEnemy>();
        if (enemy != null)
        {
            // Randomize health within the enemy's defined range
            maxHealth = Random.Range(enemy.MinPossibleHealth, enemy.MaxPossibleHealth);
        }
        else
        {
            Debug.LogWarning("No IEnemy implementation found on this object.");
        }

        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update the slider value
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} has died.");
            Destroy(gameObject); // Optionally, destroy the enemy
        }
    }

    public void IncreaseHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update the slider value
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
    }
}
