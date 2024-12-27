using UnityEngine;

public class GameStats : MonoBehaviour
{
    [Header("Stats")]
    public int killCount;
    public int alertedEnemies;
    public float totalSuspcion;

    [Header("Timer")]
    public bool isGameRunning;
    public int seconds;

    public static GameStats Instance;
    private float timer;

    private void Awake()
    {
        // Check if an instance already exists and ensure there's only one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Optionally, persist this object across scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (isGameRunning)
        {
            timer += Time.deltaTime; // Increment timer by the time since the last frame
            if (timer >= 1f)
            {
                seconds++;
                timer = 0f; // Reset the internal timer
            }
        }
    }

    public void IncreaseKill()
    {
        killCount++;
    }

    public void IncreaseAlerted()
    {
        alertedEnemies++;
    }

    public void IncreaseSuspicion(float suspicion)
    {
        totalSuspcion += suspicion;
    }

    public void ResumeTimer() // game load
    {
        isGameRunning = true;
    }

    public void StopTimer() //win, but not in main menu
    {
        isGameRunning = false;
    }

    public void ResetTimer() // game load
    {
        seconds = 0;
        timer = 0f;
    }
}
