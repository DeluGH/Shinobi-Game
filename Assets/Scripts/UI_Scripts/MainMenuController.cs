using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public Camera mainMenuCamera; // Assign your MainMenuCamera in the Inspector
    public Camera playerCamera;   // Assign your PlayerCamera in the Inspector
    public GameObject mainMenuUI; // Assign the Main Menu UI Canvas in the Inspector

    public void StartGame()
    {
        // Disable the main menu camera
        mainMenuCamera.gameObject.SetActive(false);

        // Enable the player camera
        playerCamera.gameObject.SetActive(true);

        // Disable the main menu UI
        mainMenuUI.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
