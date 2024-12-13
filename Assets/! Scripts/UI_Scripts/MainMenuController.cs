using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class MainMenuController : MonoBehaviour
{
    public enum MenuState
    {
        MainMenu,
        PauseMenu,
        Settings,
        Gameplay
    }

    public Camera mainMenuCamera; // MainMenuCamera
    public GameObject playerController; // FPSController
    public CharacterController characterController; // FPSController character controller
    public GameObject mainMenuUI; // MainMenuUI
    public GameObject settingsPanel; // SettingsPanel
    public GameObject pauseMenuPanel; // PauseMenuPanel
    public Slider audioSlider; // Audio slider
    public AudioSource gameAudioSource; // AudioSource
    public bool isFullscreen = true;  // Fullscreen boolean
    public GameObject resolution;   // resolution
    public GameObject gameplayPanel;    // gameplay

    private bool isPaused = false; // Tracks the pause state
    private MenuState currentMenuState = MenuState.MainMenu; // Tracks the current menu state

    // Start game
    public void StartGame()
    {
        Debug.Log("Game started.");
        mainMenuCamera.gameObject.SetActive(false);
        playerController.gameObject.SetActive(true);
        mainMenuUI.SetActive(false);
        playerController.GetComponent<FirstPersonController>().enabled = true;
        characterController.enabled = true;
        gameplayPanel.SetActive(true);
        currentMenuState = MenuState.Gameplay; // Set state to Gameplay

        // Lex's Code
        playerController.GetComponentInChildren<Inventory>().enabled = true;

        // Lock and hide the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Open settings
    public void OpenSettings()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Settings menu opened.");
        settingsPanel.SetActive(true);

        if (pauseMenuPanel.activeSelf || isPaused)
        {
            pauseMenuPanel.SetActive(false);
            currentMenuState = MenuState.Settings;
            Debug.Log("Reanimate cursor");

            // Keep the cursor unlocked and visible
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            mainMenuUI.SetActive(false);
            currentMenuState = MenuState.Settings;
            Debug.Log("Reanimate cursor");
            // Ensure the cursor is unlocked and visible here as well
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Close settings
    public void BackFromSettings()
    {
        if (currentMenuState == MenuState.Settings)
        {
            if (isPaused)
            {
                Debug.Log("Returned to pause menu from settings.");
                settingsPanel.SetActive(false);
                pauseMenuPanel.SetActive(true);
                currentMenuState = MenuState.PauseMenu;

                // Keep the cursor unlocked and visible
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Debug.Log("Returned to main menu from settings.");
                settingsPanel.SetActive(false);
                mainMenuUI.SetActive(true);
                currentMenuState = MenuState.MainMenu;

                // Keep the cursor visible and unlocked
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    // Quit game
    public void QuitGame()
    {
        Debug.Log("Game quit.");
        Application.Quit();
    }

    // Settings Group
    // Change screen resolution
    public void ChangeResolution()
    {
        TMP_Dropdown dropdown = resolution.GetComponent<TMP_Dropdown>();
        int option = dropdown.value;

        switch (option)
        {
            case 0:
                Screen.SetResolution(1980, 1080, Screen.fullScreen);
                Debug.Log($"Resolution changed to 1980x1080.");
                break;
            case 1:
                Screen.SetResolution(1280, 720, Screen.fullScreen);
                Debug.Log($"Resolution changed to 1280x720.");
                break;
            case 2:
                Screen.SetResolution(800, 600, Screen.fullScreen);
                Debug.Log($"Resolution changed to 800x600 .");
                break;
        }
    }

    // Toggle fullscreen mode
    public void ToggleFullScreen()
    {
        isFullscreen = !isFullscreen;

        Screen.fullScreen = isFullscreen;
        Debug.Log($"Fullscreen mode set to: {isFullscreen}");
    }

    // Adjust audio volume
    public void AdjustVolume()
    {
        if (gameAudioSource != null && audioSlider != null)
        {
            gameAudioSource.volume = audioSlider.value;
            Debug.Log($"Audio volume changed to: {audioSlider.value}");
        }
        else
        {
            Debug.LogError("Game audio source is not assigned.");
        }
    }

    // Toggle Pause Menu
    public void TogglePauseMenu()
    {
        if (!isPaused && playerController.GetComponent<Rigidbody>().isKinematic && characterController.isGrounded && playerController.GetComponent<FirstPersonController>().m_IsGrappling == false)
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                Time.timeScale = 0f; // Pause game
                playerController.GetComponent<FirstPersonController>().enabled = false;
                characterController.enabled = false;
                pauseMenuPanel.SetActive(true);
                gameplayPanel.SetActive(false);
                currentMenuState = MenuState.PauseMenu;

                // Enable and unlock the cursor
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                Debug.Log("Game paused.");
            }
            else
            {
                Time.timeScale = 1f; // Resume game
                pauseMenuPanel.SetActive(false);
                playerController.GetComponent<FirstPersonController>().enabled = true;
                characterController.enabled = true;
                gameplayPanel.SetActive(true);
                currentMenuState = MenuState.Gameplay; // Back to gameplay

                // Lock and hide the cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                Debug.Log("Game resumed.");
            }
        }
    }


    // Resume game (called by the Resume button)
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        playerController.GetComponent<FirstPersonController>().enabled = true;
        characterController.enabled = true;
        currentMenuState = MenuState.Gameplay; // Resumes to Gameplay state

        // Lock and hide the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Game resumed from pause menu.");
    }

    // Quit to main menu from pause menu
    public void QuitToMainMenu()
    {
        Debug.Log("Returned to main menu from pause menu.");
        Time.timeScale = 1f; // Resume game time
        mainMenuUI.SetActive(true);
        gameplayPanel.SetActive(false);
        mainMenuCamera.gameObject.SetActive(true);
        playerController.gameObject.SetActive(false);
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false); // Ensure settings panel is hidden
        currentMenuState = MenuState.MainMenu;

        isPaused = false; // Reset pause state

        // Enable and unlock the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Update()
    {
        if (currentMenuState == MenuState.MainMenu)
        {
            // Disable Escape key during main menu
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentMenuState == MenuState.Settings)
            {
                // If in settings menu, return to pause menu or main menu
                BackFromSettings();
            }
            else if (currentMenuState == MenuState.Gameplay || currentMenuState == MenuState.PauseMenu)
            {
                // Toggle pause menu
                TogglePauseMenu();
            }
        }
    }

    // Debugging
    private void SetMenuState(MenuState newState)
    {
        Debug.Log($"Menu state changed from {currentMenuState} to {newState}");
        currentMenuState = newState;
    }
}
