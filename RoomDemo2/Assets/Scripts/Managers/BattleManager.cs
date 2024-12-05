using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private GameObject battleCanvas;

    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerStaminaText;

    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private Slider playerStaminaSlider;

    [SerializeField] private List<Image> enemyImages; // Predefined UI image slots (Enemy1, ..., Enemy5)
    [SerializeField] private Sprite enemySprite; // Default sprite for enemies with NavigationScript
    [SerializeField] private Sprite guardSprite; // Default sprite for enemies without NavigationScript
    [SerializeField] private GameObject enemyHealthPrefab; // Prefab for "enemyhealth" gameobject with slider setup

    private List<GameObject> activeEnemyHealthUI = new List<GameObject>(); // Track active health bars

    public void StartBattle(List<IEnemy> enemies, float playerHealth, float playerStamina)
    {
        if (battleCanvas == null || enemyImages == null || enemyImages.Count == 0 || enemyHealthPrefab == null)
        {
            Debug.LogWarning("BattleCanvas, enemy image slots, or enemyHealthPrefab are not properly assigned.");
            return;
        }

        // Activate the BattleCanvas
        battleCanvas.SetActive(true);

        // Display player health and stamina text
        playerHealthText.text = $"{Mathf.RoundToInt(playerHealth)}/100";
        playerStaminaText.text = $"{Mathf.RoundToInt(playerStamina)}/100";

        // Update sliders with the current health and stamina values
        if (playerHealthSlider != null)
            playerHealthSlider.value = playerHealth;

        if (playerStaminaSlider != null)
            playerStaminaSlider.value = playerStamina;

        // Assign sprites to enemy UI slots, activate them, and create health sliders
        int enemyCount = Mathf.Min(enemies.Count, enemyImages.Count);
        for (int i = 0; i < enemyCount; i++)
        {
            IEnemy enemy = enemies[i];
            GameObject enemyObject = (enemy as MonoBehaviour)?.gameObject;

            if (enemyObject != null)
            {
                Image enemyImageSlot = enemyImages[i]; // Get corresponding UI slot

                if (enemyImageSlot != null)
                {
                    enemyImageSlot.gameObject.SetActive(true); // Activate the image slot

                    // Assign sprite based on NavigationScript presence
                    if (enemyObject.GetComponent<NavigationScript>() != null)
                    {
                        enemyImageSlot.sprite = enemySprite;
                    }
                    else
                    {
                        enemyImageSlot.sprite = guardSprite;
                    }

                    // Create health slider above the enemy image
                    GameObject healthUI = Instantiate(enemyHealthPrefab, enemyImageSlot.transform.parent);
                    RectTransform healthRect = healthUI.GetComponent<RectTransform>();
                    healthRect.anchoredPosition = enemyImageSlot.rectTransform.anchoredPosition + new Vector2(0, 150); // Position above the image
                    activeEnemyHealthUI.Add(healthUI);

                    // Configure slider
                    Slider healthSlider = healthUI.GetComponent<Slider>();
                    if (healthSlider != null)
                    {
                        float maxHealth = enemyObject.GetComponent<HealthManager>()?.maxHealth ?? 100;
                        float currentHealth = enemyObject.GetComponent<HealthManager>()?.CurrentHealth ?? 100;

                        healthSlider.maxValue = maxHealth;
                        healthSlider.value = currentHealth;

                        // Assign the fill image
                        Image fillImage = healthUI.transform.Find("fill")?.GetComponent<Image>();
                        if (fillImage != null)
                        {
                            healthSlider.fillRect = fillImage.rectTransform;
                        }

                        // Set enemy name and health info text
                        TextMeshProUGUI healthInfoText = healthUI.GetComponentInChildren<TextMeshProUGUI>();
                        if (healthInfoText != null)
                        {
                            string enemyName = enemyObject.GetComponent<NavigationScript>() != null ? "Soldier" : "Guard";
                            healthInfoText.text = $"{enemyName}\n{Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(maxHealth)}";
                        }
                    }
                }
            }
        }

        // Deactivate unused enemy image slots
        for (int i = enemyCount; i < enemyImages.Count; i++)
        {
            enemyImages[i].gameObject.SetActive(false);
        }

        // Prepare and display enemy info
        string battleInfo = "Enemies in Battle:\n";
        foreach (IEnemy enemy in enemies)
        {
            GameObject enemyObject = (enemy as MonoBehaviour)?.gameObject;

            if (enemyObject != null)
            {
                HealthManager healthManager = enemyObject.GetComponent<HealthManager>();
                if (healthManager != null)
                {
                    float currentHealth = healthManager.CurrentHealth;
                    battleInfo += $"- {enemy.GetType().Name} (State: {enemy.getCurrentState()}, Health: {Mathf.RoundToInt(currentHealth)}/{Mathf.RoundToInt(healthManager.maxHealth)})\n";
                }
                else
                {
                    battleInfo += $"- {enemy.GetType().Name} (State: {enemy.getCurrentState()}, Health: Unknown)\n";
                }
            }
        }
        infoText.text = battleInfo;
    }

    public void OnExitBattleButtonClick()
    {
        EndBattle();
        Debug.Log("Exited Battle");
        // Additional logic to reset the game state can go here if needed
    }

    public void EndBattle()
    {
        if (battleCanvas != null)
        {
            battleCanvas.SetActive(false);
        }

        // Deactivate all enemy image slots
        foreach (Image image in enemyImages)
        {
            image.gameObject.SetActive(false);
        }

        // Destroy active health UI sliders
        foreach (GameObject healthUI in activeEnemyHealthUI)
        {
            Destroy(healthUI);
        }
        activeEnemyHealthUI.Clear();
    }
}
