using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class Player : MonoBehaviour
{
    [Header("References (Auto)")]
    public FirstPersonController fpsController;
    public CharacterController charController;

    [Header("Debug References")]
    public bool isAssassinating = false;

    private void Start()
    {
        fpsController = GetComponent<FirstPersonController>();
        charController = GetComponent<CharacterController>();

        if (fpsController == null) Debug.LogWarning("No fpsController reference!!");
        if (charController == null) Debug.LogWarning("No charController reference!!");
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
}
