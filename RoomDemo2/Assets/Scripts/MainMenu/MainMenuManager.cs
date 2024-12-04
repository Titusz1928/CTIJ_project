using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public void StartGame()
    {
        // Load the game scene
        SceneManager.LoadScene("maptest1");
    }

    public void ExitGame()
    {
        // Exit the application
        Debug.Log("Exiting Game..."); // This will only be visible in the Editor
        Application.Quit();
    }
}
