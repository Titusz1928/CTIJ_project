using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] float movementSpeed = 3f;
    [SerializeField] float sprintMultiplier = 1.5f;  // Multiplier for sprinting speed
    //[SerializeField] float jumpForce = 3f;
    [SerializeField] float rotationSpeed = 2f;
    [SerializeField] float maxStamina = 100f;   // Maximum stamina
    [SerializeField] float staminaDrainRate = 10f; // Stamina drain per second
    [SerializeField] float staminaRegenRate = 5f;  // Stamina regeneration per second
    [SerializeField] Slider staminaBar;       // Reference to the UI stamina bar

    private float currentStamina;
    private float mouseY = 0f; // Store cumulative vertical rotation
    [SerializeField] float viewRange = 40f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentStamina = maxStamina;   // Set initial stamina to max
        staminaBar.maxValue = maxStamina;
        staminaBar.value = currentStamina;
    }

    void Update()
    {
        // Movement based on keyboard input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        moveDirection = transform.TransformDirection(moveDirection);

        // Sprinting logic
        float currentSpeed = movementSpeed;
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && (horizontalInput != 0 || verticalInput != 0);

        if (isSprinting && currentStamina > 0)
        {
            currentSpeed *= sprintMultiplier;
            currentStamina -= staminaDrainRate * Time.deltaTime;  // Drain stamina
            if (currentStamina < 0) currentStamina = 0; // Clamp stamina to 0
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;  // Regenerate stamina
            if (currentStamina > maxStamina) currentStamina = maxStamina; // Clamp to max
        }

        rb.velocity = new Vector3(moveDirection.x * currentSpeed, rb.velocity.y, moveDirection.z * currentSpeed);

        // Update stamina bar UI
        staminaBar.value = currentStamina;

        // Mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY += Input.GetAxis("Mouse Y") * rotationSpeed; // Accumulate vertical rotation
        mouseY = Mathf.Clamp(mouseY, -viewRange, viewRange); // Clamp the rotation

        // Horizontal rotation (left/right) - rotate player body
        transform.Rotate(Vector3.up * mouseX);

        // Vertical rotation (up/down) - rotate the camera only
        Camera.main.transform.localRotation = Quaternion.Euler(-mouseY, 0f, 0f);

        // Jumping
/*        if (Input.GetButtonDown("Jump") && currentStamina>=10)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);

            currentStamina = currentStamina - 10;

        }*/
    }
}
