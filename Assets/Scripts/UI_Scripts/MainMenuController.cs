using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Camera mainMenuCamera; // MainMenuCamera
    public GameObject playerController; // FPSController
    public GameObject mainMenuUI; // MainMenuUI
    public GameObject settingsPanel; // SettingsPanel
    public GameObject pauseMenuPanel; // PauseMenuPanel
    public Slider audioSlider; // Audio slider
    public AudioSource gameAudioSource; // AudioSource

    private bool isPaused = false; // Tracks the pause state

    // Start game
    public void StartGame()
    {
        Debug.Log("Game started.");
        mainMenuCamera.gameObject.SetActive(false);
        playerController.gameObject.SetActive(true);
        mainMenuUI.SetActive(false);
    }

    // Open settings
    public void OpenSettings()
    {
        Debug.Log("Settings menu opened.");
        settingsPanel.SetActive(true);
        mainMenuUI.SetActive(false);
    }

    // Close settings
    public void CloseSettings()
    {
        Debug.Log("Settings menu closed.");
        settingsPanel.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    // Quit game
    public void QuitGame()
    {
        Debug.Log("Game quit.");
        Application.Quit();
    }

    // Change screen resolution
    public void ChangeResolution(int width, int height)
    {
        Screen.SetResolution(width, height, Screen.fullScreen);
        Debug.Log($"Resolution changed to {width} x {height}.");
    }

    // Toggle fullscreen mode
    public void ToggleFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
        Debug.Log($"Fullscreen mode set to: {isFullScreen}");
    }

    // Adjust audio volume
    public void AdjustVolume(float volume)
    {
        if (gameAudioSource != null)
        {
            gameAudioSource.volume = volume;
            Debug.Log($"Audio volume changed to: {volume}");
        }
        else
        {
            Debug.LogError("Game audio source is not assigned.");
        }
    }

    // Toggle Pause Menu
    public void TogglePauseMenu()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            // Pause the game
            Time.timeScale = 0f; // Stop all in-game movement
            pauseMenuPanel.SetActive(true);
            Debug.Log("Game paused.");
        }
        else
        {
            // Resume the game
            Time.timeScale = 1f; // Resume all in-game movement
            pauseMenuPanel.SetActive(false);
            Debug.Log("Game resumed.");
        }
    }

    // Resume game (called by the Resume button)
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
        Debug.Log("Game resumed from pause menu.");
    }

    // Open settings from pause menu
    public void OpenPauseMenuSettings()
    {
        Debug.Log("Settings menu opened from pause menu.");
        settingsPanel.SetActive(true);
        pauseMenuPanel.SetActive(false); // Hide pause menu
    }

    // Back to pause menu from settings
    public void BackFromSettings()
    {
        if (pauseMenuPanel.activeSelf)
        {
            // Return to the pause menu if the pause menu is active
            Debug.Log("Returned to pause menu from settings.");
            settingsPanel.SetActive(false);
            pauseMenuPanel.SetActive(true);
        }
        else
        {
            // Otherwise, return to the main menu
            Debug.Log("Returned to main menu from settings.");
            settingsPanel.SetActive(false);
            mainMenuUI.SetActive(true);
        }
    }

    // Quit to main menu from pause menu
    public void QuitToMainMenu()
    {
        Debug.Log("Returned to main menu from pause menu.");

        // Unpause the game
        Time.timeScale = 1f;

        // Enable main menu UI and main menu camera
        mainMenuUI.SetActive(true);
        mainMenuCamera.gameObject.SetActive(true);

        // Disable player controller
        playerController.gameObject.SetActive(false);

        // Hide pause menu
        pauseMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf)
            {
                // If in settings menu, return to pause menu
                BackFromSettings();
            }
            else
            {
                // Toggle pause menu
                TogglePauseMenu();
            }
        }
    }
}
