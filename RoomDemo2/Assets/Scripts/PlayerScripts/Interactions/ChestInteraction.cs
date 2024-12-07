using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChestInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactionRange = 5f;
    public TextMeshProUGUI chestTextUI;

    private bool isUIVisible = false;

    void Update()
    {
        // Handle interaction with right click
        if (Input.GetMouseButtonDown(1)) // Right-click
        {
            if (isUIVisible)
            {
                HideChestText();
            }
            else
            {
                TryOpenChest();
            }
        }

        // Hide UI when player starts walking
        if (isUIVisible && IsWalking())
        {
            HideChestText();
        }
    }

    void TryOpenChest()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            if (hit.collider.CompareTag("Chest"))
            {
                ShowChestText();
            }
        }
    }

    void ShowChestText()
    {
        chestTextUI.gameObject.SetActive(true);
        chestTextUI.text = "You opened the chest!";
        isUIVisible = true;
    }

    void HideChestText()
    {
        chestTextUI.gameObject.SetActive(false);
        isUIVisible = false;
    }

    bool IsWalking()
    {
        // Detect if WASD keys are being pressed
        return Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
    }
}
