using GDS.Sample;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthSlider2;

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
        if (healthSlider2 != null)
        {
            healthSlider2.maxValue = maxHealth;
            healthSlider2.value = currentHealth;
        }
    }

    public void DecreaseHealth(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update the slider value
        if (healthSlider2 != null)
        {
            healthSlider2.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Debug.Log($"{gameObject.name} has died.");
            HandleDrops();
            Destroy(gameObject); // Optionally, destroy the enemy
        }
    }

    public void IncreaseHealth(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Update the slider value
        if (healthSlider2 != null)
        {
            healthSlider2.value = currentHealth;
        }
    }

    private void HandleDrops()
    {
        // Determine enemy type
        string enemyType = gameObject.name.Replace("(Clone)", "").Trim(); // Adjust to remove (Clone)

        // Get potential drops
        var drops = EnemyManager.GetDrops(enemyType);

        foreach (var (itemId, dropChance) in drops)
        {
            if (Random.value <= dropChance)
            {
                DropItem(itemId);
            }
        }
    }

    private void DropItem(BaseId itemId)
    {
        // Get the prefab for the item from the registry
        var prefab = PrefabRegistry.Instance?.GetPrefab(itemId.ToString());
        if (prefab == null)
        {
            Debug.LogError($"Prefab not found for item ID: {itemId}");
            return;
        }

        // Drop position and rotation
        Vector3 dropPosition = transform.position;
        dropPosition.y = 1.5f; // Set Y coordinate to 1.5f
        Quaternion randomRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);

        // Instantiate the item
        GameObject droppedItem = Instantiate(prefab, dropPosition, randomRotation);
        Debug.Log($"Dropped object: {droppedItem.name} at {dropPosition}");
        droppedItem.tag = "Pickable";
        droppedItem.layer = LayerMask.NameToLayer("PickableObjects");

        var particleSystem = droppedItem.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play();
        }

        // Add necessary components if missing
        if (!droppedItem.TryGetComponent<Rigidbody>(out _))
        {
            var rb = droppedItem.AddComponent<Rigidbody>();
            rb.mass = 1f;
        }

        if (!droppedItem.TryGetComponent<BoxCollider>(out _))
        {
            var collider = droppedItem.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.3f, 0.3f, 0.3f);
        }

        Debug.Log($"Dropped {DB.AllBasesDict[itemId].Name}");
    }
}
