using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InfoDisplayManager : MonoBehaviour
{
    [SerializeField] private GameObject infoCanvas;  // Reference to the UI Canvas GameObject
    [SerializeField] private TextMeshProUGUI seedText;  // Reference to the TextMeshPro for displaying seed
    [SerializeField] private TextMeshProUGUI gameModeText;
    [SerializeField] private TextMeshProUGUI coordsText;  // Reference to the TextMeshPro for displaying coordinates
    [SerializeField] private GameObject player;  // Reference to the player GameObject

    // Start is called before the first frame update
    void Start()
    {
        if (seedText != null)
        {
            seedText.SetText("Seed: " + GameManager.Instance.Seed.ToString());
        }
        if (gameModeText != null)
        {
            if (GameManager.Instance.CreativeMode)
            {
                gameModeText.SetText("Spectator mode");
            }
            else
            {
                gameModeText.SetText("Survival mode");
            }
        }

        if (infoCanvas != null)
        {
            infoCanvas.SetActive(false);  // Initially set the canvas to be invisible
        }
        else
        {
            Debug.LogWarning("Info Canvas is not assigned in the Inspector!");
        }

        if (player == null)
        {
            Debug.LogWarning("Player GameObject is not assigned in the Inspector!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle canvas visibility when F3 is pressed
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ToggleCanvasVisibility();
        }

        // Update the player's coordinates
        if (coordsText != null && player != null)
        {
            Vector3 playerPos = player.transform.position;  // Get the player's position

            // Subtract 90 from the x-coordinate and 100 from the z-coordinate
            float adjustedX = playerPos.x - 90f;
            float adjustedZ = playerPos.z - 100f;

            // Update the coordinates text
            coordsText.SetText("Coords: X: " + adjustedX.ToString("F2") + " Y: " + playerPos.y.ToString("F2") + " Z: " + adjustedZ.ToString("F2"));
        }
    }

    // Toggle the visibility of the canvas
    private void ToggleCanvasVisibility()
    {
        if (infoCanvas != null)
        {
            bool isActive = infoCanvas.activeSelf;
            infoCanvas.SetActive(!isActive);  // Toggle the active state of the canvas
        }
    }
}
