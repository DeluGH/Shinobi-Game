using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

//Credit to JohnDevTutorial from YouTube
//https://github.com/JonDevTutorial/LedgeClimbingTut

public class Vaulting : MonoBehaviour
{
    private int vaultLayer;
    private Camera cam;
    private FirstPersonController fpsController;
    private CharacterController characterController;
    //[SerializeField] private float playerHeight = 2f;
    //[SerializeField] private float playerRadius = 0.5f;
    [SerializeField] private float vaultRange = 1.0f;   //  max distance to be able to vault
    [SerializeField] private float vaultHeightLimit = 0.6f; //  prevents vaulting objects that are too high
    [SerializeField] private float vaultDuration = 0.5f;



    void Start()
    {
        vaultLayer = LayerMask.NameToLayer("VaultLayer");
        vaultLayer = ~vaultLayer;
        cam = Camera.main;
        characterController = GetComponent<CharacterController>();
        fpsController = GetComponent<FirstPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vault();
    }
    private void Vault()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {

            //cast a ray to see if in range to vault and check if its a vaultable object
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var firstHit, vaultRange, vaultLayer))
            {

                //cast to a point to go to after vaulting
                if (Physics.Raycast(firstHit.point + (cam.transform.forward * characterController.radius) + (Vector3.up * vaultHeightLimit * characterController.height), Vector3.down, out var secondHit, characterController.height))
                {
                    Debug.Log("Vault triggered");
                    fpsController.m_MoveDir = Vector3.zero;
                    fpsController.m_Jumping = false;
                    StartCoroutine(LerpVault(secondHit.point + Vector3.up * (characterController.height / 2f - 0.1f), vaultDuration));
                }
            }
        }

    }
    IEnumerator LerpVault(Vector3 targetPosition, float duration)
    {
        fpsController.m_IsVaulting = true; // Start vaulting

        float timePassed = 0;
        Vector3 startPosition = transform.position;

        // Temporarily disable the CharacterController
        characterController.enabled = false;

        while (timePassed < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timePassed / duration);
            timePassed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;


        //fpsController.m_IsVaulting = false; // End vaulting
        //fpsController.m_PreviouslyGrounded = true;

        // Temporarily reenable the CharacterController
        characterController.enabled = true;
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