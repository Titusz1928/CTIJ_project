using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // Assign the Pause Menu Panel in the Inspector
    public GameObject gameMenuUI;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false); // Hide the pause menu
        gameMenuUI.SetActive(true);
        Time.timeScale = 1f;          // Resume game time
        isPaused = false;
    }

    public void Pause()
    {
        Time.timeScale = 0f; // Freeze game time
        gameMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true); // Show the pause menu
        isPaused = true;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;         // Ensure game time is resumed
        SceneManager.LoadScene("MainMenu1"); // Replace with your actual Main Menu scene name
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game is quitting..."); // Only visible in the editor
    }
}
