using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreativeModeToggle : MonoBehaviour
{
    public Toggle creativeModeToggle;

    void Start()
    {
        if (creativeModeToggle != null)
        {
            // Set the toggle state based on the current Creative Mode
            creativeModeToggle.isOn = GameManager.Instance != null && GameManager.Instance.CreativeMode;

            // Add a listener for toggle changes
            creativeModeToggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    private void OnDestroy()
    {
        if (creativeModeToggle != null)
        {
            creativeModeToggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        if (GameManager.Instance != null)
        {
            // Update the creative mode state
            GameManager.Instance.ToggleCreativeMode();
        }
    }
}

