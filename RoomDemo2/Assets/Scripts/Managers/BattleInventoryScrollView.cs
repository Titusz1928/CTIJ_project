using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleInventoryScrollView : MonoBehaviour
{
    public TextMeshProUGUI TextInventory; // Reference to the TextMeshPro component
    public RectTransform ContentTransform; // Reference to the Content RectTransform

    public void UpdateInventoryUI(string inventoryText)
    {
        if (TextInventory != null)
        {
            TextInventory.text = inventoryText; // Set the text value

            // Force content to update its size
            Canvas.ForceUpdateCanvases();
            ContentTransform.sizeDelta = new Vector2(ContentTransform.sizeDelta.x, TextInventory.preferredHeight);
        }
        else
        {
            Debug.LogError("TextInventory is not assigned in the Inspector!");
        }
    }
}

