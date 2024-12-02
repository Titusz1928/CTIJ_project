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
