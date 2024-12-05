using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    [SerializeField] private GameObject battleCanvas;

    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerStaminaText;
    [SerializeField] private TextMeshProUGUI gameOverText;

    [SerializeField] private Button[] gameButtons;

    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private Slider playerStaminaSlider;

    [SerializeField] private List<Image> enemyImages; // Predefined UI image slots (Enemy1, ..., Enemy5)
    [SerializeField] private Sprite enemySprite; // Default sprite for enemies with NavigationScript
    [SerializeField] private Sprite guardSprite; // Default sprite for enemies without NavigationScript
    [SerializeField] private GameObject enemyHealthPrefab; // Prefab for "enemyhealth" gameobject with slider setup

    private List<GameObject> activeEnemyHealthUI = new List<GameObject>(); // Track active health bars
    private List<HealthManager> enemyHealthManagers = new List<HealthManager>(); // Track enemy health managers
    [SerializeField] private PlayerHealth playerHealthManager; // Track player health manager

    public void StartBattle(List<IEnemy> enemies, float playerHealth, float playerStamina)
    {
        gameOverText.gameObject.SetActive(false);
        enemyHealthManagers.Clear();

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

                    // Add the enemy's HealthManager to the list
                    HealthManager healthManager = enemyObject.GetComponent<HealthManager>();
                    if (healthManager != null)
                    {
                        enemyHealthManagers.Add(healthManager); // Populate enemyHealthManagers
                    }
                    else
                    {
                        Debug.LogWarning($"Enemy {i + 1} does not have a HealthManager component.");
                    }

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

    public void EndTurn()
    {
        foreach (Button btn in gameButtons)
        {
            btn.interactable = false;
            btn.gameObject.SetActive(false);
        }
        StartCoroutine(ExecuteEndTurn());
        Debug.Log("button works");
    }

    private IEnumerator ExecuteEndTurn()
    {
        Debug.Log("Starting end-turn sequence.");

        // Process enemies first
        Debug.Log("enemies remaining:" + enemyHealthManagers.Count);

        for (int i = 0; i < enemyHealthManagers.Count; i++)
        {
            HealthManager enemyHealth = enemyHealthManagers[i];
            Debug.Log("enemy nr. "+ i+ ", health="+ enemyHealth);
            if (enemyHealth != null)
            {
                Debug.Log($"Processing enemy {i + 1}/{enemyHealthManagers.Count} with initial health: {enemyHealth.CurrentHealth}");

                // Decrease health using the method

                enemyHealth.DecreaseHealth(50);
                Debug.Log($"Enemy {i + 1} health decreased. Current health: {enemyHealth.CurrentHealth}");

                // Update UI slider
                if (activeEnemyHealthUI[i] != null)
                {
                    Slider healthSlider = activeEnemyHealthUI[i].GetComponent<Slider>();
                    if (healthSlider != null)
                    {
                        healthSlider.value = enemyHealth.CurrentHealth;
                        Debug.Log($"Enemy {i + 1} UI health slider updated to: {healthSlider.value}");
                    }

                    // Update info text
                    TextMeshProUGUI healthInfoText = activeEnemyHealthUI[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (healthInfoText != null)
                    {
                        healthInfoText.text = $"{enemyHealth.CurrentHealth}/{enemyHealth.maxHealth}";
                        Debug.Log($"Enemy {i + 1} UI health text updated to: {healthInfoText.text}");
                    }
                }

                // Wait a few seconds
                yield return new WaitForSeconds(2f);
            }
            else
            {
                Debug.LogWarning($"Enemy {i + 1} does not have a valid HealthManager component.");
            }
        }

        // Process player health
        if (playerHealthManager != null)
        {
            Debug.Log($"Processing player health with initial health: {playerHealthManager.getCurrentHealth()}");

            playerHealthManager.DecreaseHealth(10);
            Debug.Log($"Player health decreased. Current health: {playerHealthManager.getCurrentHealth()}");

            // Update player health UI
            if (playerHealthSlider != null)
            {
                playerHealthSlider.value = playerHealthManager.getCurrentHealth();
                Debug.Log($"Player health slider updated to: {playerHealthSlider.value}");
            }

            if (playerHealthText != null)
            {
                playerHealthText.text = $"{playerHealthManager.getCurrentHealth()}/100";
                Debug.Log($"Player health text updated to: {playerHealthText.text}");
            }

            // Wait for player health animation (future feature)
            yield return new WaitForSeconds(2f);

            if (playerHealthManager.getCurrentHealth() <= 0)
            {
                StartCoroutine(HandleGameOver());
            }
        }
        else
        {
            Debug.LogWarning("PlayerHealthManager is null. Skipping player health processing.");
        }

        int nullCount = 0;
        foreach (var eHealth in enemyHealthManagers)
        {
            if (eHealth == null)
            {
                nullCount++;
            }
        }

        // Check if all enemies have null health
        Debug.Log("!!!!      " + nullCount + "?" + enemyHealthManagers.Count);
        if (nullCount == enemyHealthManagers.Count)
        {
            EndBattle(); // Call your EndBattle method if all enemies are defeated
        }

        // Update info text for the next turn
        infoText.text = "Turn ended. Prepare for the next move!";
        Debug.Log("End-turn sequence complete. Info text updated.");

        foreach (Button btn in gameButtons)
        {
            btn.interactable = true;
            btn.gameObject.SetActive(true);
        }
    }

    IEnumerator HandleGameOver()
    {
        // Display the Game Over text
        gameOverText.gameObject.SetActive(true);


        // Wait for the specified time
        yield return new WaitForSeconds(5);

        // Load the MainMenu1 scene
        SceneManager.LoadScene("MainMenu1");
    }



    public void OnExitBattleButtonClick()
    {
        EndBattle();
        Debug.Log("Exited Battle");
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
