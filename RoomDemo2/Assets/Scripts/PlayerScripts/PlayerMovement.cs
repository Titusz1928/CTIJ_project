using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] float movementSpeed = 3f;
    [SerializeField] float sprintMultiplier = 1.5f;
    [SerializeField] float sneakMultiplier = 0.5f;
    [SerializeField] float jumpForce = 3f;
    [SerializeField] float rotationSpeed = 2f;
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrainRate = 10f;
    [SerializeField] float staminaRegenRate = 5f;
    [SerializeField] Slider staminaBar;

    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask ground;

    [SerializeField] Transform cameraTransform;
    [SerializeField] float normalCameraHeight = 0.6f;
    [SerializeField] float sneakCameraHeight = 0.2f;
    [SerializeField] float cameraTransitionSpeed = 5f;

    [SerializeField] float sneakDetectionMultiplier = 0.5f; // Multiplier for reducing enemy detection range when sneaking

    [SerializeField] GameObject attractionObjectPrefab; // Prefab for the attraction object
    [SerializeField] float sprintAttractionDistance = 10f; // Attraction radius when sprinting
    [SerializeField] float jumpAttractionDistance = 15f; // Attraction radius when jumping

    [SerializeField] GameObject battleCanvas; // Reference to the BattleCanvas

    private float currentStamina;
    private float mouseY = 0f;
    [SerializeField] float viewRange = 40f;

    private bool isSneaking = false;

    public static float DetectionMultiplier { get; private set; } = 1f; // Default detection multiplier

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentStamina = maxStamina;
        staminaBar.maxValue = maxStamina;
        staminaBar.value = currentStamina;

        Vector3 cameraPosition = cameraTransform.localPosition;
        cameraPosition.y = normalCameraHeight;
        cameraTransform.localPosition = cameraPosition;
    }

    void Update()
    {
        // Movement inputs
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        moveDirection = transform.TransformDirection(moveDirection);

        float currentSpeed = movementSpeed;

        // Sprinting
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.C) && (horizontalInput != 0 || verticalInput != 0);

        if (isSprinting && currentStamina > 0)
        {
            currentSpeed *= sprintMultiplier;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina < 0) currentStamina = 0;

            // Spawn attraction object for sprinting
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                SpawnAttractionObject(sprintAttractionDistance);
            }
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina) currentStamina = maxStamina;
        }

        // Sneaking
        if (Input.GetKey(KeyCode.C))
        {
            isSneaking = true;
            currentSpeed *= sneakMultiplier;
            DetectionMultiplier = sneakDetectionMultiplier;

            Vector3 cameraPosition = cameraTransform.localPosition;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, sneakCameraHeight, Time.deltaTime * cameraTransitionSpeed);
            cameraTransform.localPosition = cameraPosition;
        }
        else
        {
            isSneaking = false;
            DetectionMultiplier = 1f;

            Vector3 cameraPosition = cameraTransform.localPosition;
            cameraPosition.y = Mathf.Lerp(cameraPosition.y, normalCameraHeight, Time.deltaTime * cameraTransitionSpeed);
            cameraTransform.localPosition = cameraPosition;
        }

        rb.velocity = new Vector3(moveDirection.x * currentSpeed, rb.velocity.y, moveDirection.z * currentSpeed);

        staminaBar.value = currentStamina;

        // Mouse rotation
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY += Input.GetAxis("Mouse Y") * rotationSpeed;
        mouseY = Mathf.Clamp(mouseY, -viewRange, viewRange);

        transform.Rotate(Vector3.up * mouseX);
        Camera.main.transform.localRotation = Quaternion.Euler(-mouseY, 0f, 0f);

        // Jumping
        if (Input.GetButtonDown("Jump") && currentStamina >= 10 && IsGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
            currentStamina -= 10;

            // Spawn attraction object for jumping
            SpawnAttractionObject(jumpAttractionDistance);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Find all enemies in the scene (You can replace this with your actual way of finding enemies)
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        List<IEnemy> enemiesInBattle = new List<IEnemy>();

        // Loop through all enemies
        foreach (GameObject enemyObject in enemies)
        {
            IEnemy enemy = enemyObject.GetComponent<IEnemy>();

            // Check if the enemy has the IEnemy component and is in the "chasing" state
            if (enemy != null && enemy.getCurrentState() == "Chasing")
            {
                // Check if the enemy is within 10 units of the player
                float distanceToPlayer = Vector3.Distance(enemyObject.transform.position, transform.position); // Assuming `transform.position` is the player's position

                if (distanceToPlayer < 10f)
                {
                    // Add the enemy to the list
                    enemiesInBattle.Add(enemy);

                    // Log the enemy's state in the console
                    Debug.Log($"Enemy {enemyObject.name} with state: {enemy.getCurrentState()} is entering the battle.");
                }
            }
        }

        // If any enemies were found, start the battle
        if (enemiesInBattle.Count > 0)
        {
            if (GameManager.BattleCanvas != null)
            {
                GameManager.BattleCanvas.SetActive(true);

                // Find the InfoText component inside the BattleCanvas
                TMPro.TextMeshProUGUI infoText = GameManager.BattleCanvas.transform
                    .Find("Panel/InfoText")
                    .GetComponent<TMPro.TextMeshProUGUI>();

                if (infoText != null)
                {
                    // Prepare the battle info message
                    string battleInfo = "Enemies in Battle:\n";
                    foreach (IEnemy enemy in enemiesInBattle)
                    {
                        GameObject enemyObject = (enemy as MonoBehaviour)?.gameObject; // Get the GameObject associated with the enemy
                        if (enemyObject != null)
                        {
                            HealthManager healthManager = enemyObject.GetComponent<HealthManager>();
                            if (healthManager != null)
                            {
                                float currentHealth = healthManager.CurrentHealth;
                                battleInfo += $"- {enemy.GetType().Name} (State: {enemy.getCurrentState()}, Health: {currentHealth}/{healthManager.maxHealth})\n";
                            }
                            else
                            {
                                battleInfo += $"- {enemy.GetType().Name} (State: {enemy.getCurrentState()}, Health: Unknown)\n";
                            }
                        }
                        else
                        {
                            battleInfo += $"- {enemy.GetType().Name} (State: {enemy.getCurrentState()}, Health: Unknown)\n";
                        }
                    }

                    // Update the InfoText with the battle info
                    infoText.text = battleInfo;
                    Debug.Log("Battle info updated in InfoText!");
                }
                else
                {
                    Debug.LogWarning("InfoText component not found in the BattleCanvas!");
                }
            }
        }
        else
        {
            Debug.Log("no enemies which could enter battle");
        }
    }


    bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, .1f, ground);
    }

    void SpawnAttractionObject(float attractionRadius)
    {
        Debug.Log("spawning attraction object");
        // Get player's X, Z position and set Y to 0
        Vector3 targetPosition = new Vector3(transform.position.x, 0, transform.position.z);

        // Check if the position is on the NavMesh
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            // Instantiate the attraction object on the NavMesh
            GameObject attractionObject = Instantiate(attractionObjectPrefab, hit.position, Quaternion.identity);

            // Adjust attraction object properties
            var collider = attractionObject.GetComponent<SphereCollider>();
            if (collider != null)
            {
                collider.radius = attractionRadius; // Set attraction range
            }
        }
        else
        {
            Debug.LogWarning("No valid NavMesh surface found at the target position.");
        }
    }
}
