using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;

    public enum MenuState
    {
        MainMenu,
        PauseMenu,
        Settings,
        Keybinds,
        Volume,
        Gameplay,
        Gameover,
    }

    [Header("Menus")]
    public GameObject mainMenuUI; // MainMenuUI
    public GameObject settingsPanel; // SettingsPanel
    public GameObject pauseMenuPanel; // PauseMenuPanel
    public GameObject resolution;   // resolution
    public GameObject gameplayPanel;    // gameplay
    public GameObject keybindsPanel;
    public GameObject volumePanel;
    public GameObject gameOverPanel;

    [Header("Others")]
    public GameObject loadingScreen;
    public Slider loadingSlider; // The slider representing the loading percentage
    public TextMeshProUGUI loadingText; // Optional: Text to display the percentage as a number (e.g., "50%")
    public Toggle fullscreenToggle;

    [Header("Auto")]
    public GameObject player;
    public Player playerScript;
    public FirstPersonController fpsController; // FPSController
    public Rigidbody rb; // FPSController
    public CharacterController characterController; // FPSController character controller

    [Header("Debug")]
    public bool isSettingsSubMenuOpen = false;
    public bool isFullscreen = true;  // Fullscreen boolean
    public bool isPaused = false; // Tracks the pause state
    public MenuState currentMenuState = MenuState.MainMenu; // Tracks the current menu state

    private void Awake()
    {
        // Ensure singleton pattern
        if (Instance == null)
        {
            Instance = this; // Set the singleton instance
            DontDestroyOnLoad(gameObject); // Optional: Persist across scenes
        }
        else
        {
            Debug.LogWarning("Multiple MainMenuController instances detected. Destroying the duplicate.");
            Destroy(gameObject); // Destroy duplicate instance
        }
    }

    private void Start()
    {
        //Fix toggle tick being wrong
        isFullscreen = Screen.fullScreen;
    }

    void Update()
    {
        if (currentMenuState == MenuState.MainMenu) return; // Disable Escape key during main menu 

        if (Input.GetKeyDown(KeybindManager.Instance.keybinds["Close"]))
        {
            switch(currentMenuState)
            {
                case MenuState.Settings:
                    CloseSettings();
                    break;
                case MenuState.Gameplay:
                case MenuState.PauseMenu:
                    TogglePauseMenu();
                    break;
                case MenuState.Keybinds:
                    ToggleKeybindsMenu();
                    break;
                case MenuState.Volume:
                    ToggleVolumesMenu();
                    break;

            }
        }
    }

    // Start game
    public void InitializePlayerVariables()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) Debug.Log("No Player Found!");

        if (playerScript == null) playerScript = player.GetComponent<Player>();
        if (fpsController == null) fpsController = player.GetComponent<FirstPersonController>();
        if (characterController == null) characterController = player.GetComponent<CharacterController>();
        if (rb == null) rb = player.GetComponent<Rigidbody>();
        if (playerScript == null) Debug.LogWarning("No playerScript Found!");
        if (fpsController == null) Debug.LogWarning("No fpsController Found!");
        if (characterController == null) Debug.LogWarning("No characterController Found!");
        if (rb == null) Debug.LogWarning("No rb Found!");

        Debug.Log("Game started.");
        mainMenuUI.SetActive(false);
        gameplayPanel.SetActive(true);
        currentMenuState = MenuState.Gameplay;

        //GameplayUIController.Instance.UpdateHealthSlider(playerScript.currentHealth, playerScript.maxHealth);

        LockCursor();
    }
    // Open settings
    public void OpenSettings()
    {
        ReanimateCursor();
        settingsPanel.SetActive(true);
        currentMenuState = MenuState.Settings;

        //Toggle wrong value fix
        fullscreenToggle.isOn = isFullscreen;

        if (pauseMenuPanel.activeSelf || isPaused) pauseMenuPanel.SetActive(false);
        else mainMenuUI.SetActive(false);
    }
    // Close settings
    public void CloseSettings()
    {
        ReanimateCursor();
        settingsPanel.SetActive(false);

        if (isPaused)
        {
            pauseMenuPanel.SetActive(true);
            currentMenuState = MenuState.PauseMenu;
        }
        else
        {
            mainMenuUI.SetActive(true);
            currentMenuState = MenuState.MainMenu;
        }
    }
    //Keybinds menu
    public void ToggleKeybindsMenu()
    {
        if (!isSettingsSubMenuOpen) // false, open
        {
            currentMenuState = MenuState.Keybinds;
            keybindsPanel.SetActive(true);
        }
        else // close
        {
            currentMenuState = MenuState.Settings;
            keybindsPanel.SetActive(false);
        }

        isSettingsSubMenuOpen = !isSettingsSubMenuOpen;
    }
    public void ToggleVolumesMenu()
    {
        if (!isSettingsSubMenuOpen) // false, open
        {
            currentMenuState = MenuState.Volume;
            volumePanel.SetActive(true);
        }
        else // close
        {
            currentMenuState = MenuState.Settings;
            volumePanel.SetActive(false);
        }

        isSettingsSubMenuOpen = !isSettingsSubMenuOpen;
    }

    // Toggle Pause Menu
    public void TogglePauseMenu()
    {
        if (!isPaused 
            && rb.isKinematic
            && characterController.isGrounded
            && fpsController.m_IsGrappling == false)
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                Time.timeScale = 0f; // Pause game
                fpsController.enabled = false;
                characterController.enabled = false;
                pauseMenuPanel.SetActive(true);
                gameplayPanel.SetActive(false);
                currentMenuState = MenuState.PauseMenu;

                ReanimateCursor();

                Debug.Log("Game paused.");
            }
            else
            {
                Time.timeScale = 1f; // Resume game
                pauseMenuPanel.SetActive(false);
                fpsController.enabled = true;
                characterController.enabled = true;
                gameplayPanel.SetActive(true);
                currentMenuState = MenuState.Gameplay; // Back to gameplay

                LockCursor();

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
        fpsController.enabled = true;
        characterController.enabled = true;
        currentMenuState = MenuState.Gameplay; // Resumes to Gameplay state

        LockCursor();

        Debug.Log("Game resumed from pause menu.");
    }
    // Quit to main menu from pause menu
    public void QuitToMainMenu(string mainMenuSceneName)
    {
        Debug.Log("Returned to main menu from pause menu.");
        Time.timeScale = 1f; // Resume game time
        mainMenuUI.SetActive(true);
        gameplayPanel.SetActive(false);
        //mainMenuCamera.gameObject.SetActive(true);
        //playerController.gameObject.SetActive(false);
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false); // Ensure settings panel is hidden
        currentMenuState = MenuState.MainMenu;
        isPaused = false; // Reset pause state

        ReanimateCursor();

        LoadScene(mainMenuSceneName);
    }

    //BUTTONS
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
    public void ToggleFullScreen()
    {
        isFullscreen = !isFullscreen;

        Screen.fullScreen = isFullscreen;
        Debug.Log($"Fullscreen mode set to: {isFullscreen}");
    }

    // LOADING
    public void LoadScene(string sceneName)
    {
        mainMenuUI.SetActive(false);

        StartCoroutine(LoadSceneAsync(sceneName));
    }
    public void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        LoadScene(currentScene.name);
    }
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Activate the loading screen
        loadingScreen.SetActive(true);

        // Start loading the scene asynchronously
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);

        // Disable scene activation until progress is complete
        asyncOperation.allowSceneActivation = false;

        // While the scene is still loading, update the slider and text
        while (!asyncOperation.isDone)
        {
            // Update loading percentage (0 to 1)
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f); // Progress will range from 0 to 0.9

            // Update slider and text with the current progress
            loadingSlider.value = progress;
            loadingText.text = Mathf.FloorToInt(progress * 100) + "%"; // Display percentage as text

            // Artificial delay to simulate slower loading
            if (asyncOperation.progress < 0.9f)
            {
                yield return new WaitForSeconds(0.1f); // Simulate loading time
            }

            // When the loading reaches 90% (progress = 0.9), activate the scene
            if (asyncOperation.progress >= 0.9f)
            {
                asyncOperation.allowSceneActivation = true;
            }

            yield return null;
        }

        // DONE
        currentMenuState = MenuState.Gameplay;
        gameplayPanel.SetActive(true);
        loadingScreen.SetActive(false);

        InitializePlayerVariables();
    }

    // GAME OVER
    public void GameOver()
    {
        currentMenuState = MenuState.Gameover;

        gameOverPanel.SetActive(true);
        gameplayPanel.SetActive(false);
    }

    //Lex's simple functions
    public void ReanimateCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void LockCursor()
    {
        // Lock and hide the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Debugging
    private void SetMenuState(MenuState newState)
    {
        Debug.Log($"Menu state changed from {currentMenuState} to {newState}");
        currentMenuState = newState;
    }
}
