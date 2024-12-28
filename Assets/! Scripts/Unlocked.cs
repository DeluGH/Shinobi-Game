using UnityEngine;

public class Unlocked : MonoBehaviour
{
    private const string GhostModeKey = "GhostModeUnlocked";

    public static Unlocked Instance;

    public bool isGhostMode = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure there's only one instance
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: keeps the singleton alive across scenes
    }

    public void UnlockGhostMode()
    {
        isGhostMode = true;

        PlayerPrefs.SetInt(GhostModeKey, 1); // 1 means true
        PlayerPrefs.Save(); // Save PlayerPrefs to ensure the change persists
        Debug.Log("Ghost Mode has been unlocked!");
    }

    public bool IsGhostModeUnlocked()
    {
        return PlayerPrefs.GetInt(GhostModeKey, 0) == 1; // 0 (default) means false
    }

    private void Start()
    {
        if (IsGhostModeUnlocked()) isGhostMode = true;
    }
}
