using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using GDS.Minimal;
using Unity.VisualScripting;
using GDS.Sample;
using System.Linq;
using System.Buffers.Text;
using static UnityEngine.EventSystems.EventTrigger;

public class BattleManager : MonoBehaviour
{

    [SerializeField] private GameObject battleCanvas;

    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI playerStaminaText;
    [SerializeField] private TextMeshProUGUI gameOverText;


    [SerializeField] private Button[] gameButtons;

    //Battle Log
    [SerializeField] private TextMeshProUGUI logTitle;
    [SerializeField] private TextMeshProUGUI logText;

    //Inventory
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameObject battleInventory;
    private string[] textInventoryWeapons;
    private string[] textInventoryConsums;
    private string[] textInventoryMats;

    //Skills
    [SerializeField] private GameObject skillsPanel;

    private int[] inventoryWeaponsAmounts;
    private int[] inventorConsumsAmounts;
    private int[] inventoryMatsAmounts;

    [SerializeField] private TextMeshProUGUI armorValueText;
    [SerializeField] private ArmorInventory armorInventory;
    [SerializeField] private TextMeshProUGUI equippedItemText;

    private int gameArmorValue;
    private string gameEquippedItem = "";
    private float gamePlayerStamina = 100;
    /// 


    [SerializeField] private Button buttonPrefab; // Reference to the Button prefab
    [SerializeField] private RectTransform contentTransform;


    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private Slider playerStaminaSlider;

    [SerializeField] private List<Image> enemyImages; // Predefined UI image slots (Enemy1, ..., Enemy5)
    [SerializeField] private Image targetedEnemyBorder;

    [SerializeField] private Sprite enemySprite; // Default sprite for enemies with NavigationScript
    [SerializeField] private Sprite guardSprite; // Default sprite for enemies without NavigationScript
    [SerializeField] private Sprite dogSprite;
    [SerializeField] private GameObject enemyHealthPrefab; // Prefab for "enemyhealth" gameobject with slider setup

    private List<GameObject> activeEnemyHealthUI = new List<GameObject>(); // Track active health bars
    private List<HealthManager> enemyHealthManagers = new List<HealthManager>(); // Track enemy health managers
    //private List<EnemyDamageManager> enemyDamageManagers = new List<EnemyDamageManager>(); //track enemy damage managers
    private Dictionary<int, EnemyDamageManager> enemyDamageManagersMap = new Dictionary<int, EnemyDamageManager>();
    private string[] enemyNames;

    [SerializeField] private PlayerHealth playerHealthManager; // Track player health manager
    [SerializeField] private PlayerMovement playerMovementManager; //for stamina managing

    //FOR ENEMY TARGETING
    private int targetedEnemyIndex = -1;
    private int enemyToBeTargeted = -1;

    public void StartBattle(List<IEnemy> enemies, float playerHealth, float playerStamina)
    {
        SceneMusicController musicController = FindObjectOfType<SceneMusicController>();

        if (musicController != null)
        {
            // Play the battle start sound effect
            if (AudioManager.Instance != null && musicController.battleStartSound != null)
            {
                Debug.Log("calling Audio Manager and attempting to play sound effect");
                AudioManager.Instance.PlaySoundEffect(musicController.battleStartSound);
            }
            int nrEnemies = Mathf.Min(enemies.Count, enemyImages.Count);
            float hpCount = 0;
            for (int i = 0; i < nrEnemies; i++)
            {
                GameObject enemyObject = (enemies[i] as MonoBehaviour)?.gameObject;
                hpCount = hpCount + (float)enemyObject.GetComponent<HealthManager>()?.maxHealth;
            }
            Debug.Log("total enemy hp=" + hpCount);
            if (hpCount < 250)
            {
                musicController.playBattleMusic();
            }
            else
            {
                musicController.playBigBattleMusic();
            }
        }
        else
        {
            Debug.LogError("SceneMusicController not found in the scene!");
        }

        if (inventoryManager.isInventoryOpen)
        {
            inventoryManager.ToggleInventory();
        }
        //setting up battle log (by default it appears first)
        logText.gameObject.SetActive(true);
        logTitle.gameObject.SetActive(true);

        //settimg up battle inventory text
        battleInventory.SetActive(false);
        Debug.Log(Store.Instance.LogMainInventoryItems());
        //TextInventory.text = Store.Instance.LogMainInventoryItems();
        calculateInventory();

        calculateArmorValue();

        //enabling mouse
        Cursor.lockState = CursorLockMode.None; // Unlock the cursor
        Cursor.visible = true; // Make the cursor visible again

        //hiding and clearing texts and arrays
        gameOverText.gameObject.SetActive(false);
        equippedItemText.SetText("-");
        gameEquippedItem = "";
        enemyHealthManagers.Clear();
        enemyDamageManagersMap.Clear();

        //default targeted enemy
        targetedEnemyIndex = 0; 

        if (battleCanvas == null || enemyImages == null || enemyImages.Count == 0 || enemyHealthPrefab == null)
        {
            Debug.LogWarning("BattleCanvas, enemy image slots, or enemyHealthPrefab are not properly assigned.");
            return;
        }

        // Activate the BattleCanvas
        battleCanvas.SetActive(true);

        int playerHealthRoundedUp= Mathf.CeilToInt(playerHealth);
        int playerStaminaRoundedUp = Mathf.CeilToInt(playerStamina);
        // Display player health and stamina text
        playerHealthText.text = $"{Mathf.RoundToInt(playerHealthRoundedUp)}/100";
        playerStaminaText.text = $"{Mathf.RoundToInt(playerStaminaRoundedUp)}/100";

        // Update sliders with the current health and stamina values
        if (playerHealthSlider != null)
            playerHealthSlider.value = playerHealth;

        gamePlayerStamina = playerStamina;
        if (playerStaminaSlider != null)
            playerStaminaSlider.value = playerStamina;

        playerHealthSlider.gameObject.SetActive(true);
        playerStaminaSlider.gameObject.SetActive(true);

        // Assign sprites to enemy UI slots, activate them, and create health sliders
        int enemyCount = Mathf.Min(enemies.Count, enemyImages.Count);
        enemyNames = new string[enemyCount];
        for (int i = 0; i < enemyCount; i++)
        {
            IEnemy enemy = enemies[i];
            enemyNames[i]=enemy.GetType().Name;
            GameObject enemyObject = (enemy as MonoBehaviour)?.gameObject;

            if (enemyObject != null)
            {
                Image enemyImageSlot = enemyImages[i]; // Get corresponding UI slot

                if (enemyImageSlot != null)
                {
                    enemyImageSlot.gameObject.SetActive(true); // Activate the image slot

                    // Assign sprite based on NavigationScript presence
                    /*                    if (enemyObject.GetComponent<NavigationScript>() != null)
                                        {
                                            enemyImageSlot.sprite = enemySprite;
                                        }
                                        else
                                        {
                                            enemyImageSlot.sprite = guardSprite;
                                        }*/
                    if (enemyObject.GetComponent<NavigationScript>() != null)
                    {
                        // Handle soldier logic
                        enemyImageSlot.sprite = enemySprite;
                    }
                    else if (enemyObject.GetComponent<GuardNavigation>() != null)
                    {
                        // Handle guard logic
                        enemyImageSlot.sprite = guardSprite;
                    }
                    else if (enemyObject.GetComponent<DogNavigationScript>() != null)
                    { 
                        enemyImageSlot.sprite = dogSprite;
                    }
                    else
                    {
                        enemyImageSlot.sprite = enemySprite;
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

                    // Add the enemy's DamageManagers to the list
                    EnemyDamageManager enemyDamageManager = enemyObject.GetComponent<EnemyDamageManager>();
                    if (enemyDamageManager != null)
                    {
                        // Assign a unique index or ID to each enemy
                        int customIndex = enemyDamageManagersMap.Count; // or use a unique ID
                        enemyDamageManagersMap.Add(customIndex, enemyDamageManager);
                        // Populate other lists or structures as necessary
                    }
                    else
                    {
                        Debug.LogWarning($"Enemy {i + 1} does not have a EnemyDamageManager component.");
                    }


                    Transform buttonTransform = enemyImageSlot.transform.Find("SelectButton");
                    Button button = buttonTransform?.GetComponent<Button>();

                    if (button != null)
                    {
                        button.onClick.RemoveAllListeners();

                        // Pass the correct index via a lambda expression to ensure the correct index is captured
                        int capturedIndex = i; // Capture the current value of i in a local variable
                        button.onClick.AddListener(() => SelectTargetEnemy(capturedIndex)); // Use capturedIndex
                    }
                    else
                    {
                        Debug.LogWarning($"Button not found for enemy image slot {i}.");
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
                        /*                        TextMeshProUGUI healthInfoText = healthUI.GetComponentInChildren<TextMeshProUGUI>();
                                                if (healthInfoText != null)
                                                {
                                                    string enemyName = enemyObject.GetComponent<NavigationScript>() != null ? "Soldier" : "Guard";
                                                    healthInfoText.text = $"{enemyName}\n{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
                                                }*/
                        TextMeshProUGUI healthInfoText = healthUI.GetComponentInChildren<TextMeshProUGUI>();
                        if (healthInfoText != null)
                        {
                            if (enemyObject.GetComponent<NavigationScript>() != null) { 
                                string enemyName = "Soldier"; // Retrieve enemy name from dictionary
                                healthInfoText.text = $"{enemyName}\n{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
                            }
                            if (enemyObject.GetComponent<GuardNavigation>() != null)
                            {
                                string enemyName = "Guard"; // Retrieve enemy name from dictionary
                                healthInfoText.text = $"{enemyName}\n{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
                            }
                            if (enemyObject.GetComponent<DogNavigationScript>() != null)
                            {
                                string enemyName = "Dog"; // Retrieve enemy name from dictionary
                                healthInfoText.text = $"{enemyName}\n{Mathf.CeilToInt(currentHealth)}/{Mathf.CeilToInt(maxHealth)}";
                            }
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
/*        string battleInfo = "Enemies in Battle:\n";
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
        }*/
        infoText.text = "You were ambushed!";
    }

    public void calculateInventory()
    {
        textInventoryWeapons = Store.Instance.getMainInventoryWeapons();
        textInventoryConsums = Store.Instance.getMainInventoryConsumables();
        textInventoryMats = Store.Instance.getMainInventoryMaterials();
        inventoryWeaponsAmounts = Store.Instance.getMainInventoryWeaponsAmounts();
        inventorConsumsAmounts = Store.Instance.getMainInventoryConsumablesAmounts();
        inventoryMatsAmounts = Store.Instance.getMainInventoryMaterialsAmounts();
        UpdateInventoryUI(textInventoryWeapons, inventoryWeaponsAmounts);
    }

    public void calculateArmorValue()
    {
        gameArmorValue = 0;
        string helmetname = armorInventory.HelmetSlot.ItemBase.Id.ToString();
        // Try to parse the helmet ID as a BaseId
        if (Enum.TryParse<GDS.Sample.BaseId>(helmetname, out var helmetBaseId))
        {
            // Now use the parsed BaseId to look up in the ArmorManager.Effects dictionary
            if (ArmorManager.Effects.TryGetValue(helmetBaseId, out var helmetArmorValue))
            {
                gameArmorValue += helmetArmorValue; // Add the value to the total armor value
            }
            else
            {
                Debug.LogWarning($"No armor value found for helmet: {helmetBaseId}");
            }
        }
        else
        {
            Debug.Log($"Failed to parse '{helmetname}' as a BaseId.");
        }

        string bodyarmorname = armorInventory.BodyArmorSlot.ItemBase.Id.ToString();

        // Try to parse the helmet ID as a BaseId
        if (Enum.TryParse<GDS.Sample.BaseId>(bodyarmorname, out var bodyarmorBaseId))
        {
            // Now use the parsed BaseId to look up in the ArmorManager.Effects dictionary
            if (ArmorManager.Effects.TryGetValue(bodyarmorBaseId, out var bodyArmorValue))
            {
                gameArmorValue += bodyArmorValue; // Add the value to the total armor value
            }
            else
            {
                Debug.LogWarning($"No armor value found for helmet: {bodyarmorBaseId}");
            }
        }
        else
        {
            Debug.Log($"Failed to parse '{bodyarmorname}' as a BaseId.");
        }

        string bootname = armorInventory.BootsSlot.ItemBase.Id.ToString();

        // Try to parse the helmet ID as a BaseId
        if (Enum.TryParse<GDS.Sample.BaseId>(bootname, out var bootBaseId))
        {
            // Now use the parsed BaseId to look up in the ArmorManager.Effects dictionary
            if (ArmorManager.Effects.TryGetValue(bootBaseId, out var bootArmorValue))
            {
                gameArmorValue += bootArmorValue; // Add the value to the total armor value
            }
            else
            {
                Debug.LogWarning($"No armor value found for helmet: {bootBaseId}");
            }
        }
        else
        {
            Debug.Log($"Failed to parse '{bootname}' as a BaseId.");
        }

        armorValueText.SetText(gameArmorValue.ToString());
        return;

    }

    public void EndTurn()
    {
        ShowBattleLog();
        foreach (Button btn in gameButtons)
        {
            btn.interactable = false;
            btn.gameObject.SetActive(false);
        }
        StartCoroutine(ExecuteEndTurn());
    }

    private IEnumerator ExecuteEndTurn()
    {
        bool isConsuming = false;
        bool isAttacking = false;
        Debug.Log("Starting end-turn sequence.");

        // Process enemies first
        Debug.Log("enemies remaining:" + enemyHealthManagers.Count);
        infoText.text = $"You equipped: {gameEquippedItem}";


        //decrease health for every enemy (area damage attacks)
        /*        for (int i = 0; i < enemyHealthManagers.Count; i++)
                {
                    HealthManager enemyHealth = enemyHealthManagers[i];
                    Debug.Log("enemy nr. " + i + ", health=" + enemyHealth);
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
                                // Preserve the enemy name by splitting the current text if necessary
                                string[] lines = healthInfoText.text.Split('\n');
                                string enemyName = lines.Length > 0 ? lines[0] : "Unknown";

                                // Update the text while keeping the enemy name
                                healthInfoText.text = $"{enemyName}\n{Mathf.RoundToInt(enemyHealth.CurrentHealth)}/{Mathf.RoundToInt(enemyHealth.maxHealth)}";
                                Debug.Log($"Enemy {i + 1} UI health text updated to: {healthInfoText.text}");
                            }
                        }

                        // Wait a few seconds
                        yield return new WaitForSeconds(2f);

                        // Check if the enemy has been removed or is null
                        if (enemyHealth == null)
                        {
                            Debug.Log($"Enemy {i + 1} has been defeated. Deactivating UI.");

                            // Deactivate the UI components for the defeated enemy
                            if (activeEnemyHealthUI[i] != null)
                            {
                                activeEnemyHealthUI[i].SetActive(false); // Disable the health slider
                            }

                            if (enemyImages[i] != null)
                            {
                                enemyImages[i].gameObject.SetActive(false); // Disable the enemy image
                            }
                            yield return new WaitForSeconds(1f);
                        }

                    }
                    else
                    {
                        Debug.LogWarning($"Enemy {i + 1} does not have a valid HealthManager component.");
                    }
                }
        */


        if (gameEquippedItem != null || gameEquippedItem=="")
        {
            // Try to find the equipped item in the AllBasesDict using the BaseId key
            if (Enum.TryParse<BaseId>(gameEquippedItem, out var baseId) && DB.AllBasesDict.TryGetValue(baseId, out var equippedItem))
            {
                // Log the ItemClass of the equipped item
                Debug.Log($"Equipped Item: {equippedItem.Name}, Class: {equippedItem.ItemClass}");


                if (equippedItem.ItemClass == ItemClass.Weapon1H)
                {
                    isAttacking = true;
                }
                if(equippedItem.ItemClass == ItemClass.Consumable)
                {
                    isConsuming = true;
                }
            }
            else
            {
                // Log an error if the BaseId was not found
                Debug.LogError($"Equipped item '{gameEquippedItem}' could not be found in the database.");
            }
        }
        else
        {
            Debug.LogWarning("No item is currently equipped.");
        }


        //if selected item is consumable
        if (isConsuming)
        {
            consumeItem();
            yield return new WaitForSeconds(2f);
        }

        //if selected item is a weapon
        HealthManager enemyHealth = enemyHealthManagers[targetedEnemyIndex];
        Debug.Log("enemy nr. " + targetedEnemyIndex + ", health=" + enemyHealth);
        if (enemyHealth != null && isAttacking)
        {


            Debug.Log($"Processing enemy {targetedEnemyIndex + 1}/{enemyHealthManagers.Count} with initial health: {enemyHealth.CurrentHealth}");

            // Decrease health using the method

            float damageToEnemy = calculateDamageToEnemy();
            //int damageToEnemy = 60;

            enemyHealth.DecreaseHealth(damageToEnemy);
            Debug.Log($"Enemy {targetedEnemyIndex + 1} health decreased. Current health: {enemyHealth.CurrentHealth}");

            // Update UI slider
            if (activeEnemyHealthUI[targetedEnemyIndex] != null)
            {
                Slider healthSlider = activeEnemyHealthUI[targetedEnemyIndex].GetComponent<Slider>();
                if (healthSlider != null)
                {
                    healthSlider.value = enemyHealth.CurrentHealth;
                    Debug.Log($"Enemy {targetedEnemyIndex + 1} UI health slider updated to: {healthSlider.value}");
                }

                // Update info text
                TextMeshProUGUI healthInfoText = activeEnemyHealthUI[targetedEnemyIndex].GetComponentInChildren<TextMeshProUGUI>();
                if (healthInfoText != null)
                {
                    // Preserve the enemy name by splitting the current text if necessary
                    string[] lines = healthInfoText.text.Split('\n');
                    string enemyName = lines.Length > 0 ? lines[0] : "Unknown";

                    // Update the text while keeping the enemy name
                    healthInfoText.text = $"{enemyName}\n{Mathf.CeilToInt(enemyHealth.CurrentHealth)}/{Mathf.CeilToInt(enemyHealth.maxHealth)}";
                    Debug.Log($"Enemy {targetedEnemyIndex + 1} UI health text updated to: {healthInfoText.text}");
                }
            }

            // Wait a few seconds
            yield return new WaitForSeconds(2f);

            // Check if the enemy has been removed or is null
            if (enemyHealth == null)
            {



                int realIndex = -1;
                int i = 0;
                //Debug.LogWarning("enemies:");
                foreach (var pair in enemyDamageManagersMap)
                {
                    i++;
                    //Debug.LogWarning("custom index"+pair.Key +"   , real index"+i);
                    if (pair.Key == targetedEnemyIndex)
                    {
                        realIndex = pair.Key;  // The key is the real index
                        break;
                    }
                }


                if (realIndex != -1)
                {
                    // Remove the enemy DamageManager using the real index (custom index)
                    enemyDamageManagersMap.Remove(realIndex);
                    Debug.Log($"Enemy with custom index {realIndex} has been defeated.");
                }
                else
                {
                    Debug.LogError($"Enemy with custom index {targetedEnemyIndex} not found.");
                }



                Debug.Log($"Enemy {targetedEnemyIndex + 1} has been defeated. Deactivating UI.");

                SelectNextNonNullEnemy();

                // Deactivate the UI components for the defeated enemy
                if (activeEnemyHealthUI[targetedEnemyIndex] != null)
                {
                    activeEnemyHealthUI[targetedEnemyIndex].SetActive(false); // Disable the health slider
                }

                if (enemyImages[targetedEnemyIndex] != null)
                {
                    enemyImages[targetedEnemyIndex].gameObject.SetActive(false); // Disable the enemy image
                }
                yield return new WaitForSeconds(1f);
                targetedEnemyIndex = enemyToBeTargeted;
            }

        }
        else
        {
            Debug.LogWarning($"Enemy {targetedEnemyIndex + 1} does not have a valid HealthManager component.");
        }

        bool playerAlive = true;
        // Process player health
        if (playerHealthManager != null)
        {
            Debug.Log($"Processing player health with initial health: {playerHealthManager.getCurrentHealth()}");


            foreach (var enemyDamageManagerPair in enemyDamageManagersMap)
            {
                int customIndex = enemyDamageManagerPair.Key; // Get the custom index
                //Debug.LogWarning("player will take damage from customindex:"+ customIndex);
                EnemyDamageManager damageManager = enemyDamageManagerPair.Value; // Get the corresponding EnemyDamageManager

                int rawdamage = Mathf.RoundToInt(UnityEngine.Random.Range(damageManager.getMinDamage, damageManager.getMaxDamage));
                Debug.Log("calculated enemy damage: " + rawdamage);

                int finaldamage = calculateFinalDamage(rawdamage);

                playerHealthManager.DecreaseHealth(finaldamage);
                infoText.SetText("Enemy inflicted " + finaldamage + " damage");
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
                    playerAlive= false;
                    StartCoroutine(HandleGameOver());
                }
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
       // Debug.Log("!!!!      " + nullCount + "?" + enemyHealthManagers.Count);
        if (nullCount == enemyHealthManagers.Count && playerAlive)
        {
            EndBattle(); // Call your EndBattle method if all enemies are defeated
        }

        Debug.Log(playerAlive);
        if (playerAlive)
        {
            foreach (Button btn in gameButtons)
            {
                btn.interactable = true;
                btn.gameObject.SetActive(true);
            }
        }

        //turn ended, player gains 5 stamina by default
        gamePlayerStamina = gamePlayerStamina + 5;
        updatePlayerStaminaDisplay();

        // Update info text for the next turn
        infoText.text = "Turn ended. Prepare for the next move!";
        Debug.Log("End-turn sequence complete. Info text updated.");

    }

    private float calculateDamageToEnemy()
    {
        bool isCritical = false;
        if (string.IsNullOrEmpty(gameEquippedItem))
        {
            Debug.LogWarning("No item is equipped to use.");
            return 0f;
        }
        try
        {
            // Parse the string value of gameEquippedItem to the corresponding BaseId enum
            if (Enum.TryParse(gameEquippedItem, out BaseId itemId))
            {
                // Check if the item exists in the Effects dictionary
                if (WeaponManager.Effects.TryGetValue(itemId, out var effect))
                {
                    if (gamePlayerStamina < effect.staminaUse)
                        return 0f;

                    float chance = effect.critchance;
                    float baseDamage = effect.damage;
                    float multiplier = effect.critmultiplier;
                    int damageTypeIndex = effect.damageType;


                    EnemyResistanceManager.EnemyResistances.TryGetValue(enemyNames[targetedEnemyIndex], out var enemyResistanceArray);

                    gamePlayerStamina = gamePlayerStamina - effect.staminaUse;
                    gamePlayerStamina = Mathf.Max(gamePlayerStamina, 0);
                    updatePlayerStaminaDisplay();

                    float randomValue = UnityEngine.Random.Range(0f, 100f);
                    float damage;

                    if (randomValue <= chance) // Critical hit occurs
                    {
                        damage = baseDamage * multiplier; // Apply critical multiplier
                        Debug.Log($"Critical hit ({damage}) inflicted with {gameEquippedItem}");
                        isCritical = true;
                    }
                    else
                    {
                        damage = baseDamage; // Normal damage
                        Debug.Log($"Enemy was hit ({damage}) with {gameEquippedItem}");
                    }
                    damage = damage * enemyResistanceArray[damageTypeIndex];
                    // Apply randomness to the damage
                    float randomFactor = baseDamage * 0.05f; // 5% of the base damage
                    float minDamage = damage - randomFactor;
                    float maxDamage = damage + randomFactor;
                    float finalDamage = UnityEngine.Random.Range(minDamage, maxDamage);

                    if(isCritical)
                        infoText.SetText($"Critical damage inflicted ({Mathf.RoundToInt(finalDamage)}) with {gameEquippedItem}");
                    infoText.SetText($"Enemy was hit ({Mathf.RoundToInt(finalDamage)}) with {gameEquippedItem}");
                    return Mathf.RoundToInt(finalDamage); // Return rounded final damage
                }
                else
                {
                    Debug.LogWarning($"Item {gameEquippedItem} is not a weapon.");
                }
            }
            else
            {
                Debug.LogError($"Invalid item name: {gameEquippedItem}. Cannot parse to BaseId.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error using item {gameEquippedItem}: {ex.Message}");
        }
        return 0;
    }

    private void updatePlayerStaminaDisplay()
    {
        // Update player health UI
        if (playerStaminaSlider != null)
        {
            playerStaminaSlider.value = gamePlayerStamina;
            Debug.Log($"Player health slider updated to: {playerStaminaSlider.value}");
        }

        if (playerStaminaText != null)
        {
            int staminaRoundedUp = Mathf.CeilToInt(gamePlayerStamina);
            playerStaminaText.text = $"{staminaRoundedUp}/100";
            Debug.Log($"Player health text updated to: {playerStaminaText.text}");
        }
    }



    private void consumeItem()
    {
        // Check if the player has equipped an item
        if (string.IsNullOrEmpty(gameEquippedItem))
        {
            Debug.LogWarning("No item is equipped to consume.");
            return;
        }

        try
        {
            // Parse the string value of gameEquippedItem to the corresponding BaseId enum
            if (Enum.TryParse(gameEquippedItem, out BaseId itemId))
            {
                // Check if the item exists in the Effects dictionary
                if (ConsumableManager.Effects.TryGetValue(itemId, out var effect))
                {
                    int healthToRestore = effect.health;
                    gamePlayerStamina = gamePlayerStamina + effect.stamina;
                    updatePlayerStaminaDisplay();

                    // Apply the health effect
                    playerHealthManager.IncreaseHealth(healthToRestore);
                    playerHealthSlider.value = playerHealthManager.getCurrentHealth();
                    int healthRoundedUp = Mathf.CeilToInt(playerHealthManager.getCurrentHealth());
                    playerHealthText.text = $"{healthRoundedUp}/100";

                    removeItemFromInventory();

                    Debug.Log($"Consumed {gameEquippedItem}, restored {healthToRestore} health.");
                    infoText.SetText($"Consumed {gameEquippedItem}, restored {healthToRestore} health.");

                }
                else
                {
                    Debug.LogWarning($"Item {gameEquippedItem} is not a consumable.");
                }
            }
            else
            {
                Debug.LogError($"Invalid item name: {gameEquippedItem}. Cannot parse to BaseId.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error consuming item {gameEquippedItem}: {ex.Message}");
        }

        return;
    }

    private void removeItemFromInventory()
    {
        if (string.IsNullOrEmpty(gameEquippedItem))
        {
            Debug.LogWarning("No item is equipped to remove.");
            return;
        }

        // Access the MainInventory Slots
        var slots = Store.Instance.MainInventory.Slots;

        // Find the specific item in the slots
        for (int i = 0; i < slots.Count; i++)
        {
            var currentSlot = slots[i];

            // Check if the slot contains an item matching the equipped item
            if (currentSlot.Item.ItemBase.Name == gameEquippedItem)
            {
                // Create updated item data with decreased quantity
                var updatedItemData = currentSlot.Item.ItemData with { Quant = currentSlot.Item.ItemData.Quant - 1 };

                if (updatedItemData.Quant <= 0)
                {
                    // Remove the item completely
                    slots[i] = currentSlot with { Item = GDS.Core.Item.NoItem };
                    Debug.Log($"Item '{gameEquippedItem}' completely consumed and removed from inventory.");
                    gameEquippedItem = "";
                    equippedItemText.SetText("-");
                }
                else
                {
                    // Update the slot with the new quantity
                    slots[i] = currentSlot with
                    {
                        Item = currentSlot.Item with { ItemData = updatedItemData }
                    };
                    Debug.Log($"Item '{gameEquippedItem}' quantity decreased to {updatedItemData.Quant}.");
                }

                textInventoryConsums = Store.Instance.getMainInventoryConsumables();
                inventorConsumsAmounts = Store.Instance.getMainInventoryConsumablesAmounts();
                UpdateInventoryUI(textInventoryConsums, inventorConsumsAmounts);
                return; // Exit after updating the inventory
            }
        }

        // If no matching item is found
        Debug.LogWarning($"Item '{gameEquippedItem}' not found in the inventory.");
    }

    private int calculateFinalDamage(int rawDamage)
    {
        // Check if armor can fully absorb the damage
        if (rawDamage / 2 > gameArmorValue)
        {
            return rawDamage - gameArmorValue; // Flat armor reduction
        }
        else
        {
            // Scale the remaining damage using a percentage reduction
            return Mathf.Max(1, rawDamage / 2); // Ensure the minimum damage is at least 1
        }
    }


    IEnumerator HandleGameOver()
    {
        SceneMusicController musicController = FindObjectOfType<SceneMusicController>();
        AudioManager.Instance.PlaySoundEffect(musicController.gameoverSound);
        // Display the Game Over text
        gameOverText.gameObject.SetActive(true);
        playerHealthSlider.gameObject.SetActive(false);
        playerStaminaSlider.gameObject.SetActive(false);

        //reset the inventory
        Store.Instance.Reset();

        // Wait for the specified time
        yield return new WaitForSeconds(5);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Load the MainMenu1 scene
        SceneManager.LoadScene("MainMenu1");
    }



    public void OnExitBattleButtonClick()
    {
        ShowBattleLog();
        // Generate a random number to determine success
        if (UnityEngine.Random.value > 0.5f) // Random.value generates a float between 0.0 and 1.0
        {
            EndBattle();
            Debug.Log("Exited Battle");
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            gameEquippedItem = "";
            EndTurn();
            // Escape failed, update the log message
            Debug.Log("Escape attempt failed!");
            logText.SetText("You attempted to escape but you weren't successful.");
        }
    }

    public void EndBattle()
    {
        playerMovementManager.setStamina(gamePlayerStamina);
        //playerMovementManager.
        if (battleCanvas != null)
        {
            battleCanvas.SetActive(false);
        }

        RectTransform enemyImageRectTransform = enemyImages[0].rectTransform;
        targetedEnemyBorder.GetComponent<RectTransform>().position = enemyImageRectTransform.position;

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
        SceneMusicController musicController = FindObjectOfType<SceneMusicController>();

        if (musicController != null)
        {
            musicController.playGameMusic();
        }
        else
        {
            Debug.LogError("SceneMusicController not found in the scene!");
        }
    }

    private void UpdateBorderPosition(int index)
    {
        if (index >= 0 && index < enemyImages.Count)
        {
            // Move the border to the selected enemy's position using the RectTransform of the image
            RectTransform enemyImageRectTransform = enemyImages[index].rectTransform;
            targetedEnemyBorder.GetComponent<RectTransform>().position = enemyImageRectTransform.position;

            // Make sure the border is visible
            //targetedEnemyBorder.SetActive(true);  // Ensure the border is active
        }
    }

    private void SelectNextNonNullEnemy()
    {
        // Loop through the enemyHealthManagers to find the first non-null enemy
        for (int i = 0; i < enemyHealthManagers.Count; i++)
        {
            if (enemyHealthManagers[i] != null)
            {
                // Once a non-null enemy is found, update the border position and break the loop
                enemyToBeTargeted = i;
                UpdateBorderPosition(i);
                break; // Exit the loop once the first valid enemy is found
            }
        }
    }


    private void SelectTargetEnemy(int index)
    {
        Debug.Log($"SelectTargetEnemy called with index: {index}");
        Debug.Log($"enemyHealthManagers.Count: {enemyHealthManagers.Count}");

        // Check if index is within range
        if (index < 0 || index >= enemyHealthManagers.Count)
        {
            Debug.LogWarning("Index is out of range. Check if the button is linked to the correct enemy.");
            return;
        }

        // Check if the enemyHealthManager at the index is null
        if (enemyHealthManagers[index] == null)
        {
            Debug.LogWarning($"EnemyHealthManager at index {index} is null. Ensure it is properly assigned.");
            return;
        }

        // If everything is valid, select the enemy
        targetedEnemyIndex = index;
        UpdateBorderPosition(index); // Update UI to show selected target
        //infoText.text = $"Targeted Enemy {index + 1}: {enemyHealthManagers[index].CurrentHealth}/{enemyHealthManagers[index].maxHealth} HP";
        Debug.Log($"Enemy {index + 1} selected as target.");
    }


    public void ShowBattleLog()
    {
        skillsPanel.SetActive(false);
        battleInventory.SetActive(false);
        logText.gameObject.SetActive( true );
        logTitle.gameObject.SetActive(true);
    }

    public void ShowInventory()
    {
        skillsPanel.SetActive(false);
        logText.gameObject.SetActive(false);
        logTitle.gameObject.SetActive(false);
        battleInventory.SetActive(true);
    }

    public void ShowSkills()
    {
        logText.gameObject.SetActive(false);
        logTitle.gameObject.SetActive(false);
        battleInventory.SetActive(false);
        skillsPanel.SetActive(true);
    }

    public void weaponsSelected()
    {
        UpdateInventoryUI(textInventoryWeapons, inventoryWeaponsAmounts);
    }
    public void consumablesSelected()
    {
        UpdateInventoryUI(textInventoryConsums, inventorConsumsAmounts);
    }
    public void materialsSelected()
    {
        UpdateInventoryUI(textInventoryMats, inventoryMatsAmounts);
    }

    private void UpdateInventoryUI(string[] itemNames, int[] amounts)
    {
        // Clear any existing buttons in the content area
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // Instantiate a button for each item in the selected category
        for (int i = 0; i < itemNames.Length; i++)
        {
            string itemName = itemNames[i];
            int itemAmount = amounts[i];
            // Instantiate the button prefab
            Button newButton = Instantiate(buttonPrefab, contentTransform);

            // Set the button's text
            TextMeshProUGUI buttonText = newButton.transform.Find("ItemNameText")?.GetComponent<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = itemName;
            }
            else
            {
                Debug.LogWarning("TextMeshProUGUI component not found in button prefab.");
            }

            // Set the amount text
            TextMeshProUGUI amountText = newButton.transform.Find("ItemAmountText")?.GetComponent<TextMeshProUGUI>();
            if (amountText != null)
            {
                amountText.text = "("+itemAmount.ToString()+")"; // itemAmount holds the amount
            }
            else
            {
                Debug.LogWarning("ItemAmountText component not found in button prefab.");
            }

            // Find the item in the database
            var dbItem = DB.AllBases.FirstOrDefault(dbBase => dbBase.Name == itemName);
            if (dbItem != null)
            {
                // Find the child object explicitly named "Image" (or your specific naming convention)
                Transform childImageTransform = newButton.transform.Find("ItemIconImage");
                if (childImageTransform != null)
                {
                    Image childImage = childImageTransform.GetComponent<Image>();
                    if (childImage != null)
                    {
                        Sprite iconSprite = Resources.Load<Sprite>(dbItem.IconPath);
                        if (iconSprite != null)
                        {
                            childImage.sprite = iconSprite;

                            // Adjust the Image's size
                            RectTransform imageRect = childImage.GetComponent<RectTransform>();
                            if (imageRect != null)
                            {
                                imageRect.anchorMin = new Vector2(0.5f, 0.5f); // Center anchoring
                                imageRect.anchorMax = new Vector2(0.5f, 0.5f);
                                imageRect.pivot = new Vector2(0.5f, 0.5f);     // Pivot in the center
                                imageRect.sizeDelta = new Vector2(64, 64);     // Fixed size
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Icon not found for item: {dbItem.Name} at path: {dbItem.IconPath}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Image component not found on child 'Image' in button prefab.");
                    }
                }
                else
                {
                    Debug.LogWarning("Child object 'Image' not found in button prefab.");
                }
            }
            else
            {
                Debug.LogWarning($"Item not found in database: {itemName}");
            }

            // Add a listener for button clicks
            newButton.onClick.AddListener(() => OnItemSelected(itemName));
        }
    }


    // Handle what happens when an item is selected
    private void OnItemSelected(string item)
    {
        // For example, display the selected item's name in the TextInventory
        //TextInventory.text = "Selected Item: " + item;
        equippedItemText.SetText(item);
        gameEquippedItem = item;
        Debug.Log(item);
    }
}
