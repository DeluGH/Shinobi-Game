using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class PlayerInteract : MonoBehaviour
{
    [Header("Interacting")]
    [SerializeField] private float interactRange = 3;
    public float scanAngle = 35f;
    private Interactable interactable;
    private RaycastHit interactionHit;

    [Header("Debug Refer")]
    public GameObject lookingAt;

    [Header("References (Auto)")]
    public Player playerScript;
    public LayerMask interactLayer;


    private void Start()
    {
        if (playerScript == null) playerScript = GetComponent<Player>();
        if (playerScript == null) Debug.LogWarning("No Playerscript found!");

        if (interactLayer == 0) interactLayer = LayerMask.GetMask("Interact");
    }

    // Update is called once per frame
    void Update()
    {
        //  To whover is gonna be fixing this, there are two problems
        // 1. interactable.DoInteraction(); is not being ran
        // 2. after hovering over an interactable for a while, player control is just gone for some reason


        //if (Physics.Raycast(transform.position, playerScript.cameraFacing.forward, out interactionHit, interactRange))
        //{
        //    if (interactionHit.collider.CompareTag(interactTag))
        //    {
        //        interactable = interactionHit.collider.GetComponent<Interactable>();
        //        interactable.DoHoverOverActions();


        //        if (interactable != null) // Ensure it's not null
        //        {
        //            // Perform hover-over actions
        //            interactable.DoHoverOverActions();

        //            // Check for interaction input
        //            if (Input.GetKeyDown(KeybindManager.Instance.keybinds[interactKeyString]))
        //            {
        //                interactable.DoInteraction();
        //            }
        //        }

        //    }
        //}

        if (ScanForInteractable())
        {
            interactable = lookingAt?.GetComponent<Interactable>();

            if (interactable != null)
            {
                interactable.DoHoverOverActions();

                if (Input.GetKeyDown(KeybindManager.Instance.keybinds["Interact"]))
                {
                    interactable.DoInteraction();
                }
            }
        }
        else
        {
            interactable = null; 
        }

    }

    public bool ScanForInteractable() // Closest/First rayhit enemy is Target
    {
        Collider[] hitColliders = Physics.OverlapSphere(playerScript.cameraFacing.position, interactRange, interactLayer);
        float closestDistance = float.MaxValue;
        GameObject closestEnemy = null;

        foreach (Collider collider in hitColliders)
        {
            Vector3 closestPointOnCollider = collider.ClosestPoint(playerScript.cameraFacing.position);
            Vector3 directionToPoint = (closestPointOnCollider - playerScript.cameraFacing.position).normalized;

            float angleToPoint = Vector3.Angle(playerScript.cameraFacing.forward, directionToPoint);
            if (angleToPoint <= scanAngle / 2)
            {
                float distanceToPoint = Vector3.Distance(playerScript.cameraFacing.position, closestPointOnCollider);

                if (distanceToPoint < closestDistance)
                {
                    closestDistance = distanceToPoint;
                    closestEnemy = collider.gameObject;
                }
            }
        }

        if (closestEnemy != null)
        {
            lookingAt = closestEnemy;
            return true;
        }

        lookingAt = null;
        return false;
    }
}
