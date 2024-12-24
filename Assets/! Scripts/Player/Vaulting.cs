using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

//Credit to JohnDevTutorial from YouTube
//https://github.com/JonDevTutorial/LedgeClimbingTut

public class Vaulting : MonoBehaviour
{
    //public LayerMask vaultLayer;
    private Camera cam;
    private FirstPersonController fpsController;
    private CharacterController characterController;
    private bool isVaulting;
    private AudioSource audioSource;
    //[SerializeField] private float playerHeight = 2f;
    //[SerializeField] private float playerRadius = 0.5f;
    [SerializeField] private float vaultRange = 1.0f;   //  max distance to be able to vault
    [SerializeField] private float vaultHeightLimit = 0.6f; //  prevents vaulting objects that are too high
    [SerializeField] private float vaultDuration = 0.7f;
    [SerializeField] private AudioClip vaultingSound;
    [SerializeField] private bool playAudioWhilstVaulting;

    [Header("Cursor Tip")]
    public CursorTipsManager.Tip tip;

    void Start()
    {
        //vaultLayer = ~vaultLayer;
        cam = Camera.main;
        characterController = GetComponent<CharacterController>();
        fpsController = GetComponent<FirstPersonController>();
        isVaulting = false;
        audioSource = GetComponent<AudioSource>();
        if (vaultingSound != null)
            audioSource.clip = vaultingSound;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsGrappleValid())
        {
            if (CursorTipsManager.Instance != null)
            {
                tip.key = KeybindManager.Instance.keybinds["Jump"];
                tip.tipMessage = "Vault";
                CursorTipsManager.Instance.MakeTip(tip);
            }
                
        }
        else
        {
            if (CursorTipsManager.Instance != null)
            {
                tip.key = KeybindManager.Instance.keybinds["Jump"];
                tip.tipMessage = "Vault";
                CursorTipsManager.Instance.RemoveTip(tip);
            }
                
        }

        Vault();
    }

    private void Vault()
    {

        if ((Input.GetKeyDown(KeybindManager.Instance.keybinds["Jump"])) && !isVaulting)
        {
            //cast a ray to see if in range to vault and check if its a vaultable object
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var firstHit, vaultRange))
            {
                // check if its a climbable grapple point
                Grappable grappable = null;
                bool grappableValid = false;
                if (firstHit.collider.gameObject.tag == "Grappable")
                {
                    grappable = firstHit.collider.gameObject.GetComponent<Grappable>();
                    grappableValid = grappable.climbable;
                }

                //cast to a point to go to after vaulting
                if ((firstHit.collider.gameObject.tag == "Vaultable" || grappableValid)
                    && Physics.Raycast(firstHit.point + (cam.transform.forward * characterController.radius) + (Vector3.up * vaultHeightLimit * characterController.height), Vector3.down, out var secondHit, characterController.height))
                {
                    //  if grappling hook swining, stop it
                    if (fpsController.m_IsGrappling)
                        fpsController.GetComponent<GrapplingHookScript>().StopSwing();

                    Debug.Log("Vault triggered");
                    isVaulting = true;
                    fpsController.m_MoveDir = Vector3.zero;
                    fpsController.m_Jumping = false;

                    //play vaulting sound
                    if (vaultingSound != null && playAudioWhilstVaulting)
                        audioSource.Play();

                    StartCoroutine(LerpVault(secondHit.point + Vector3.up * (characterController.height / 2f - 0.1f), vaultDuration));
                }
            }
        }

    }

    private bool IsGrappleValid()
    {
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var firstHit, vaultRange))
        {
            Grappable grappable = null;
            bool grappableValid = false;

            if (firstHit.collider.gameObject.tag == "Grappable")
            {
                grappable = firstHit.collider.gameObject.GetComponent<Grappable>();
                grappableValid = grappable.climbable;
            }

            if ((firstHit.collider.gameObject.tag == "Vaultable" || grappableValid)
                && Physics.Raycast(firstHit.point + (cam.transform.forward * characterController.radius) + (Vector3.up * vaultHeightLimit * characterController.height), Vector3.down, out var secondHit, characterController.height))
            {
                return grappableValid;
            }
            
        }
        return false;
    }

    IEnumerator LerpVault(Vector3 targetPosition, float duration)
    {
        fpsController.m_IsVaulting = true; // Start vaulting

        float timePassed = 0;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion downwardRotation = Quaternion.Euler(30, transform.eulerAngles.y, transform.eulerAngles.z); // Rotate downwards
        Quaternion finalRotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);    // End up looking straight ahead

        // Temporarily disable the CharacterController
        characterController.enabled = false;
        // Block player input
        fpsController.enabled = false;


        while (timePassed < duration)
        {
            float t = timePassed / duration;

            // Move the player
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            // Rotate the player downwards at the start and back to looking straight ahead
            if (t < 0.5f)
            {
                transform.rotation = Quaternion.Slerp(startRotation, downwardRotation, t * 2);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(downwardRotation, finalRotation, (t - 0.5f) * 2);
            }

            timePassed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position and rotation
        transform.position = targetPosition;
        transform.rotation = finalRotation;

        // Re-enable the CharacterController
        characterController.enabled = true;
        // Re-enable player input
        fpsController.enabled = true;

        isVaulting = false;
        ToggleVaultingAfterDelay();
    }


    public void ToggleVaultingAfterDelay()
    {
        StartCoroutine(SwitchVaultingAfterDelay(0.1f));
    }

    private IEnumerator SwitchVaultingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        fpsController.m_IsVaulting = !fpsController.m_IsVaulting; // Toggle the boolean value
    }
}