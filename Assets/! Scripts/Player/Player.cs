using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour
{
    [Header("References (Auto)")]
    public FirstPersonController fpsController;
    public CharacterController charController;
    public PlayerNoise noiseScript;
    public Rigidbody rb;

    [Header("Health")]
    public int maxHealth;
    public bool assingImmunity = true;

    [Header("Debug References")]
    public int currentHealth;
    public bool isAssassinating = false;
    public bool isDead = false;
    public bool isWalking = false;
    public bool isCrouching = false;
    public bool isFalling = false;
    public LayerMask enemyLayer;

    public float fallHeight = 0f;
    private float startFallHeight = 0f;
    private float lastPositionY = 0f;

    private void Start()
    {
        fpsController = GetComponent<FirstPersonController>();
        charController = GetComponent<CharacterController>();
        noiseScript = GetComponentInChildren<PlayerNoise>();

        if (fpsController == null) Debug.LogWarning("No fpsController reference!!");
        if (charController == null) Debug.LogWarning("No charController reference!!");
        if (noiseScript == null) Debug.LogWarning("No noiseScript reference!!");

        enemyLayer = LayerMask.GetMask("Enemy");
        if (enemyLayer == 0) Debug.LogWarning("Enemy layer reference is missing!");

        currentHealth = maxHealth;
        GameplayUIController.Instance.UpdateHealthSlider(currentHealth, maxHealth);
    }

    private void Update()
    {
        CheckFalling();
    }

    public void PlayerHit()
    {
        if (assingImmunity && isAssassinating) return;

        if (currentHealth > 0)
        {
            currentHealth--;
            GameplayUIController.Instance.UpdateHealthSlider(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            isDead = true;
        }
    }

    public void PlayerHealing(int amount)
    {
        if (amount <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); //clamping

        GameplayUIController.Instance.UpdateHealthSlider(currentHealth, maxHealth);
    }

    public void DoingAss()
    {
        isAssassinating = true;

        fpsController.enabled = false;
        charController.enabled = false;
    }

    public void DoneAss()
    {
        isAssassinating = false;

        fpsController.enabled = true;
        charController.enabled = true;
    }

    public void FPSControllerNoiseHandler(bool isWalk, bool isCrouched) // Called at FPSController's Sound Play (Footsteps)
    {
        //if isWalk is false, = isRunning
        isWalking = isWalk;
        isCrouching = isCrouched;
        if (!isCrouched) PlayerMadeNoise();
    }

    public void PlayerMadeNoise()
    {
        //Debug.Log("Player Made Noise!");

        noiseScript.NotifyEnemies(isWalking);
    }

    private void CheckFalling()
    {
        float currentY = transform.position.y;

        if (!charController.isGrounded)
        {
            // Check if we are now falling (we started moving down from the peak)
            if (!isFalling && currentY < lastPositionY)
            {
                //Debug.Log("Player started falling!");
                isFalling = true;
                startFallHeight = lastPositionY; // Save the height when fall starts
            }

            // Calculate fall height if falling
            if (isFalling)
            {
                fallHeight = startFallHeight - currentY; // Calculate the height fallen
                //Debug.Log("Height fallen: " + fallHeight);
            }
        }
        else
        {
            // Player has landed
            if (isFalling)
            {
                //Debug.Log("Player landed!");
                isFalling = false;
                fallHeight = 0f; // Reset fall height after landing
            }
        }

        // Update last known Y position
        lastPositionY = currentY;
    }
}
