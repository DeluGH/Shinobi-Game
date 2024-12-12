using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour
{
    [Header("References (Auto)")]
    public FirstPersonController fpsController;
    public CharacterController charController;
    public PlayerNoise noiseScript;

    [Header("Debug References")]
    public bool isAssassinating = false;
    public bool isWalking = false;
    public bool isCrouching = false;
    public LayerMask enemyLayer;

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
        Debug.Log("Player Made Noise!");

        noiseScript.NotifyEnemies(isWalking);
    }
}
