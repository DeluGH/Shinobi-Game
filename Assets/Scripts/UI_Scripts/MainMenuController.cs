using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Camera mainMenuCamera; // MainMenuCamera
    public GameObject playerController;   // FPSController
    public GameObject mainMenuUI; // MainMenuUI
    public GameObject settingsPanel; // SettingsPanel 

    // Audio
    public Slider audioSlider; //  audio slider 
    public AudioSource gameAudioSource; // AudioSource

    public void StartGame()
    {
        Debug.Log("Game started.");
        mainMenuCamera.gameObject.SetActive(false);
        playerController.gameObject.SetActive(true);
        mainMenuUI.SetActive(false);
    }

    public void OpenSettings()
    {
        Debug.Log("Settings menu opened.");
        settingsPanel.SetActive(true);
        mainMenuUI.SetActive(false);
    }

    public void CloseSettings()
    {
        Debug.Log("Settings menu closed.");
        settingsPanel.SetActive(false);
        mainMenuUI.SetActive(true);
    }

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
}
