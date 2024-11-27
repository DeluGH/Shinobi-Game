using Obi;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

//Taken inspiration from Dave / GameDevelopment from YouTube

public class GrapplingHookScript : MonoBehaviour
{
    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse1;
    public LayerMask grappleableLayer;
    public ObiRope rope; // Reference to the Obi Rope for rendering
    public FirstPersonController fpsController;
    public CharacterController characterController;
    public GameObject grapplingHook; // Reference to the grappling hook
    public float hookReachTime = 2f; // Time for the hook to reach the target
    public float hookrange = 10f;
    [Range(0f, 1f)]
    public float grappleJointMaxDistance = .8f;
    [Range(0f, 1f)]
    public float grappleJointMinDistance = .25f;
    public float grappleJointSpring = 4.5f, grappleJointDamper = 7f, grappleJointMassScale = 4.5f;
    public float swingJumpForce = 1000f;
    [Tooltip("Used to detect collisions to turn back on Character Controller")]
    public CapsuleCollider grapplingCollider;

    private Camera cam;
    //private Vector3 targetPosition; // Target position for the grappling hook
    private Vector3 startPosition;  //  Starting place for hook when thrown
    private SpringJoint joint;
    private bool inPhysicsMovementState = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(swingKey)) ThrowGrapplingHook();   //may change to using an Object-Oriented approach using a PlayerEquipManager or something
        if (Input.GetKeyUp(swingKey)) StopSwing();

        var rb = fpsController.GetComponent<Rigidbody>();
        //if (rb.linearVelocity.y == 0 && !fpsController.m_IsGrappling)
        Debug.DrawRay(fpsController.transform.position, Vector3.down, Color.cyan, characterController.height / 2 + 0.1f);
        if (Physics.Raycast(fpsController.transform.position, Vector3.down, characterController.height/2 + 0.1f))
        {
            
            inPhysicsMovementState = false;

            // Re-enable CharacterController and disable Rigidbody physics
            characterController.enabled = true;
            fpsController.GetComponent<Rigidbody>().isKinematic = true; // Stop Rigidbody physics
            ToggleGrapplingCollider(false);
        }
    }


    private void StopSwing()
    {
        RenderGrapplingHook(false);
        Destroy(joint);

        var rb = fpsController.GetComponent<Rigidbody>();
        rb.AddForce(cam.transform.forward.normalized * swingJumpForce);

        fpsController.m_IsGrappling = false;
    }

    private void StartSwing(Vector3 swingPoint)
    {
        //Debug.Log("Swinging");
        inPhysicsMovementState = true;
        var rb = fpsController.GetComponent<Rigidbody>();

        // Disable CharacterController and enable Rigidbody physics
        characterController.enabled = false;
        rb.isKinematic = false; // Allow Rigidbody physics
        rb.linearVelocity = characterController.velocity;

        joint = fpsController.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        float distanceFromPoint = Vector3.Distance(fpsController.transform.position, swingPoint);

        //  set max and min distance to keep from grapple point
        joint.maxDistance = distanceFromPoint * .8f;
        joint.minDistance = distanceFromPoint * .25f;

        ToggleGrapplingCollider(true);
    }

    private void ThrowGrapplingHook()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, hookrange, grappleableLayer))
        {
            //  demo
            //grapplingHook.transform.position = hit.point;
            fpsController.m_IsGrappling = true;
            startPosition = grapplingHook.transform.position = fpsController.transform.position;    //  in the futrure, set a specific location for grappling hook whilst player equipping it

            StartCoroutine(LerpGrapplingHookTravel(hit.point, hookReachTime));
            RenderGrapplingHook(true);
        }


    }

    IEnumerator LerpGrapplingHookTravel(Vector3 targetPosition, float duration)
    {
        float timePassed = 0;

        while (timePassed < duration)
        {
            float t = timePassed / duration;

            // Move the hook
            grapplingHook.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            timePassed += Time.deltaTime;
            yield return null;

        }

        //  Ensure final pos
        grapplingHook.transform.position = targetPosition;

        //  Once hooked, start swinging
        StartSwing(targetPosition);

    }
    void RenderGrapplingHook(bool isRendered)
    {
        rope.GetComponent<ObiRopeExtrudedRenderer>().enabled = isRendered;
        grapplingHook.GetComponent<Renderer>().enabled = isRendered;
    }

    void ToggleGrapplingCollider(bool isEnable)
    {
        grapplingCollider.enabled = isEnable;

        grapplingCollider.height = characterController.height;
        grapplingCollider.center = characterController.center;
        grapplingCollider.radius = characterController.radius;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!fpsController.m_IsGrappling && !inPhysicsMovementState)
        {
            // Re-enable CharacterController and disable Rigidbody physics
            characterController.enabled = true;
            fpsController.GetComponent<Rigidbody>().isKinematic = true; // Stop Rigidbody physics
            ToggleGrapplingCollider(false);
        }

    }

}