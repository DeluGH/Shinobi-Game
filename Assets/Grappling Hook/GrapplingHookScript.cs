using Obi;
using System.Collections;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class GrapplingHookScript : MonoBehaviour
{
    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse1;
    public KeyCode climbKey = KeyCode.Space;
    public KeyCode descendKey = KeyCode.LeftControl;
    public ObiRope rope; // Reference to the Obi Rope for rendering
    public FirstPersonController fpsController;
    public CharacterController characterController;
    public GameObject grapplingHook; // Reference to the grappling hook
    public float hookReachTime = 2f; // Time for the hook to reach the target
    public float hookRange = 10f;
    public float swingJumpForce = 300f;
    public float climbSpeed = 2f;
    public float maxSwingSpeed = 10f;
    [Tooltip("Force applied when moving swing direction")]
    public float swingControlForce = 1f;
    [Tooltip("Used to detect collisions to turn back on Character Controller")]
    public CapsuleCollider grapplingCollider;


    private Camera cam;
    private Vector3 startPosition;
    private ConfigurableJoint joint;
    private float currentRopeLength;
    private bool inPhysicsMovementState = false;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (Input.GetKeyDown(swingKey)) ThrowGrapplingHook();
        if (Input.GetKeyUp(swingKey)) StopSwing();

        if (joint != null)
        {
            HandleClimbing();
            ApplySwingControl();
        }

        var rb = fpsController.GetComponent<Rigidbody>();
        //if (rb.linearVelocity.y == 0 && !fpsController.m_IsGrappling)
        //Debug.DrawRay(fpsController.transform.position, Vector3.down, Color.cyan, characterController.height / 2 + 0.1f);
        if (Physics.Raycast(fpsController.transform.position, Vector3.down, characterController.height / 2 + 0.1f) && !fpsController.m_IsGrappling)
        {

            inPhysicsMovementState = false;

            ReactivateCharacterController();
        }
    }

    private void ReactivateCharacterController()
    {
        // Re-enable CharacterController and disable Rigidbody physics
        characterController.enabled = true;
        fpsController.GetComponent<Rigidbody>().isKinematic = true; // Stop Rigidbody physics
        ToggleGrapplingCollider(false);
    }

    public void StopSwing()
    {
        RenderGrapplingHook(false);
        Destroy(joint);

        var rb = fpsController.GetComponent<Rigidbody>();
        rb.AddForce(cam.transform.forward.normalized * swingJumpForce);

        fpsController.m_IsGrappling = false;
        joint = null;
    }

    private void StartSwing(Vector3 swingPoint)
    {
        fpsController.m_IsGrappling = true;
        var rb = fpsController.GetComponent<Rigidbody>();

        // Disable CharacterController and enable Rigidbody physics
        characterController.enabled = false;
        rb.isKinematic = false;
        rb.linearVelocity = characterController.velocity;
        inPhysicsMovementState = true;

        joint = fpsController.gameObject.AddComponent<ConfigurableJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = swingPoint;

        // Configure the joint
        float distanceFromPoint = Vector3.Distance(fpsController.transform.position, swingPoint);
        currentRopeLength = distanceFromPoint;

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;

        SoftJointLimit limit = new SoftJointLimit { limit = distanceFromPoint };
        joint.linearLimit = limit;

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        ToggleGrapplingCollider(true);
    }

    private void ThrowGrapplingHook()
    {
        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, hookRange))
        {
            if (hit.collider.gameObject.tag == "Grappable") 
            {
                startPosition = grapplingHook.transform.position = fpsController.transform.position;

                StartCoroutine(LerpGrapplingHookTravel(hit.point, hookReachTime));
                RenderGrapplingHook(true);
            }

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

        // Ensure final position
        grapplingHook.transform.position = targetPosition;

        // Once hooked, start swinging
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

    private void ApplySwingControl()
    {
        if (!fpsController.m_IsGrappling) return;

        // Get input for horizontal and vertical movement
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float verticalInput = Input.GetAxis("Vertical");     // W/S or Up/Down Arrow

        // Calculate movement direction relative to the camera
        Vector3 moveDirection = (cam.transform.right * horizontalInput + cam.transform.forward * verticalInput).normalized;

        // Apply force to the Rigidbody
        Rigidbody rb = fpsController.GetComponent<Rigidbody>();
        rb.AddForce(moveDirection * swingControlForce, ForceMode.Acceleration);
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSwingSpeed);
    }

    private void HandleClimbing()
    {
        if (Input.GetKey(climbKey))
        {
            AdjustRopeLength(-climbSpeed * Time.deltaTime);
        }
        else if (Input.GetKey(descendKey))
        {
            AdjustRopeLength(climbSpeed * Time.deltaTime);
        }
    }

    private void AdjustRopeLength(float adjustment)
    {
        currentRopeLength = Mathf.Clamp(currentRopeLength + adjustment, 1f, hookRange);

        SoftJointLimit limit = joint.linearLimit;
        limit.limit = currentRopeLength;
        joint.linearLimit = limit;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!fpsController.m_IsGrappling && !inPhysicsMovementState)
        {
            ReactivateCharacterController();
        }
    }
}
