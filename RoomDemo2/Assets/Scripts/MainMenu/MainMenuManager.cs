using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // Import to work with UI elements like InputField

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainmenuUI;
    [SerializeField] private GameObject aboutmenuUI;
    [SerializeField] private GameObject pregamnemenuUI;
    [SerializeField] private GameObject settingsmenuUI;
    [SerializeField] private TMP_InputField seedInputField;  // Reference to the InputField

    public void StartGame()
    {
        int seed = 0;

        // Try to parse the input text
        if (int.TryParse(seedInputField.text, out seed) && seed != 0)
        {
            // If valid seed is entered, use it
            GameManager.Instance.SetSeed(seed);
            Debug.Log($"Using entered seed: {seed}");
        }
        else
        {
            // If not valid or empty, generate a random seed
            seed = Random.Range(int.MinValue, int.MaxValue);
            GameManager.Instance.SetSeed(seed);
            Debug.Log($"Generated random seed: {seed}");
        }

        // Load the game scene
        SceneManager.LoadScene("maptest2");
    }

    public void toPreGameMenu()
    { 
        mainmenuUI.SetActive(false);
        aboutmenuUI.SetActive(false);
        settingsmenuUI.SetActive(false);
        pregamnemenuUI.SetActive(true);
       
    }

    public void toAboutMenu()
    {
        pregamnemenuUI.SetActive(false);
        mainmenuUI.SetActive(false);
        settingsmenuUI.SetActive(false);
        aboutmenuUI.SetActive(true);
    }

    public void toMainmenu()
    {
        pregamnemenuUI.SetActive(false);
        aboutmenuUI.SetActive(false);
        settingsmenuUI.SetActive(false);
        mainmenuUI.SetActive(true);
    }

    public void toSettingsMenu()
    {
        pregamnemenuUI.SetActive(false);
        aboutmenuUI.SetActive(false);
        mainmenuUI.SetActive(false);
        settingsmenuUI.SetActive(true);
    }

    public void ExitGame()
    {
        // Exit the application
        Debug.Log("Exiting Game..."); // This will only be visible in the Editor
        Application.Quit();
    }
}