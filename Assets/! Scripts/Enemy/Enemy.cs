using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static UnityEditor.FilePathAttribute;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Settings")]
    public int baseHealth = 4;
    public float baseMovementSpeed = 5f;
    public float baseRotationSpeed = 120f;
    public float maxDistanceFromNodes = 2.5f; //Default: 3
    public float despawnTime = 300f;

    [Header("Combat Settings")]
    public bool hitCauseAlert = true; //melee hits cause aggro
    public bool canBlock = true;
    public bool isBlocking = false;
    public bool isAttacking = false;

    public float combatModeProximity = 8f; // Prefered distance from player
    public float combatModeOutDecayTime = 0.5f; // If player steps out, wait for this amount of time before continuing chase
    public bool combatMode = false;

    [Header("Smoked Settings")]
    public bool inSmoke = false;
    public float smokeChokeAfterDisappear = 2f; // Extra choking time after smoke disappears

    [Header("Debug Stats (Auto)")]
    public int currentHealth = 0;
    public float movementSpeed = 5f;
    public float rotationSpeed = 120f;
    [Space(5f)]
    public bool isWalking = false;
    public bool isRunning = false;
    public bool isStrafing = false;
    public bool strafingRight = false;
    [Space(5f)]
    public bool isChoking = false; // Stunned - Choking
    public float chokingDuration = 0f;
    public float chokingTimer = 0f;
    public bool isStunned = false; // Stunned - Confused?
    public float stunnedDuration = 0f;
    public float stunnedTimer = 0f;
    [Space(5f)]
    public bool isDead = false; //Should stop ALL functions (KILL SWITCH)
    [Space(5f)]
    public bool isExecutingActivity = false; // Prevents multiple activities from running simultaneously
    public bool isActivityPaused = false; // Pause activity when player is detected/chased/investigated
    [Space(5f)]
    public bool playerInDetectionArea = false;

    [Header("References (Auto Assign)")]
    public Transform player;            // Reference to the player
    public NavMeshAgent agent;
    public Animator anim; 

    public DetectionAI detectionScript;
    public ActivityAI activityScript;
    public EnemyAttack attackScript;

    public LayerMask playerLayer;       // Layer mask for detecting players
    public LayerMask enemyLayer;       // Layer mask for detecting enemies
    public LayerMask smokeLayer;
    public LayerMask surfacesLayer;

    public int overlappingSmokes = 0; // Fix bug with overlapping smokes,
                                      // if 1 ends, shouldn't "unsmoke" the enemy
    public float combatModeTimer = 0f;

    private void Awake()
    {
        // Priority: Layers
        if (playerLayer == 0) playerLayer = LayerMask.GetMask("Player");
        if (playerLayer == 0) Debug.LogWarning("Player Layer reference is missing!");
        if (enemyLayer == 0) enemyLayer = LayerMask.GetMask("Enemy");
        if (enemyLayer == 0) Debug.LogWarning("Enemy Layer reference is missing!");
        if (smokeLayer == 0) smokeLayer = LayerMask.GetMask("Smoke");
        if (smokeLayer == 0) Debug.LogWarning("Smoke Layer reference is missing!");
        if (surfacesLayer == 0) surfacesLayer = LayerMask.GetMask("Surfaces");
        if (surfacesLayer == 0) Debug.LogWarning("Surfaces Layer reference is missing!");
        // Find Player w Layer
        if (player == null) FindPlayer();
        if (player == null) Debug.LogWarning("Player reference is missing! Unable to FindPlayer()");
        // Others
        if (detectionScript == null) detectionScript = GetComponent<DetectionAI>();
        if (detectionScript == null) Debug.LogWarning("This Enemy is Missing detectionScript!");
        if (activityScript == null) activityScript = GetComponent<ActivityAI>();
        if (activityScript == null) Debug.LogWarning("This Enemy is Missing activityScript!");
        if (attackScript == null) attackScript = GetComponent<EnemyAttack>();
        if (attackScript == null) Debug.LogWarning("This Enemy is Missing attackScript!");
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (agent == null) Debug.LogWarning("This Enemy is Missing NavMeshAgent! Other AI Scripts will not be able to run!");
        if (anim == null) anim = GetComponentInChildren<Animator>();
        if (anim == null) Debug.LogWarning("This Enemy is Missing anim!");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = baseHealth;
        movementSpeed = baseMovementSpeed;
        rotationSpeed = baseRotationSpeed;

        agent.speed = movementSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.stoppingDistance = maxDistanceFromNodes;

        if (detectionScript != null) detectionScript.enabled = true;
        if (activityScript != null) activityScript.enabled = true;
    }

    private void Update()
    {
        if (player == null) { FindPlayer(); return; }

        if (HasReachedDestination())
        {
            if (isStrafing)
            {
                isStrafing = false;
                SetMovementAnimation();
            }
        }

        if (isChoking)
        {
            chokingTimer += Time.deltaTime;
            if (chokingTimer >= chokingDuration)
            {
                isChoking = false;
                chokingTimer = 0f;
                chokingDuration = 0f;
            }
        }
        
        if (isStunned)
        {
            stunnedTimer += Time.deltaTime;
            if (stunnedTimer >= stunnedDuration)
            {
                isStunned = false;
                stunnedTimer = 0f;
                stunnedDuration = 0f;
            }
        }
    }

    public void SetMovementAnimation()
    {
        if (!canEnemyPerform()) return;

        // GET DIRECTION 
        if (HasReachedDestination())
        {
            isWalking = false;
            isRunning = false;
            anim.SetBool("isWalking", false);
            anim.SetBool("isRunning", false);
            anim.SetBool("isBackingUp", false);
        }
        else
        {
            // WALKING OR RUNNING?
            if (movementSpeed <= baseMovementSpeed + 1f) // walking
            {
                isWalking = true;
                isRunning = false;
                anim.SetBool("isWalking", true);
                anim.SetBool("isRunning", false);
                anim.SetBool("isBackingUp", false);

                anim.SetTrigger("Walk");
            }
            else // running
            {
                isWalking = false;
                isRunning = true;
                anim.SetBool("isWalking", false);
                anim.SetBool("isRunning", true);
                anim.SetBool("isBackingUp", false);

                anim.SetTrigger("Run");
            }

            Vector3 toDestination = agent.destination - transform.position;
            Vector3 localDirection = transform.InverseTransformDirection(toDestination.normalized);
            if (localDirection.z < -0.5f) // back
            {
                anim.SetBool("isBackingUp", true);
                anim.SetTrigger("Back");
            }
        }
        

        // Strafing instead of walking/running?
        if (isStrafing)
        {
            if (!strafingRight)
            {
                anim.SetBool("isStrafingRight", true);
                anim.SetBool("isStrafingLeft", false);
                anim.SetTrigger("StrafeRight");
            }
            else //left
            {
                anim.SetBool("isStrafingRight", false);
                anim.SetBool("isStrafingLeft", true);
                anim.SetTrigger("StrafeLeft");
            }
        }
        else
        {
            anim.SetBool("isStrafingRight", false);
            anim.SetBool("isStrafingLeft", false);
        }
    }

    public bool canEnemyPerform()
    {
        if (isDead || isChoking || isStunned || isBlocking || isAttacking) return false;
        else return true;
    }

    private void FindPlayer()
    {
        //find the player within range
        Collider[] colliders = Physics.OverlapSphere(transform.position, Mathf.Infinity, playerLayer); // Since it's a singleplayer game, it uses Mathf.Inf to find the player!!!

        if (colliders.Length > 0)
        {
            player = colliders[0].transform; //get first player found
            //Debug.Log($"Player found: {player.name}");
        }
        else
        {
            //Debug.Log("No player detected within the specified range.");
        }
    }

    public void EnterCombatMode()
    {
        Debug.Log("Combat ON");
        agent.isStopped = true; //Stop moving
        agent.updateRotation = false;
        detectionScript.isLookingAround = false; //bug fix 

        agent.isStopped = false; //Reenable to reposition
        attackScript.Reposition();

        combatMode = true;
        combatModeTimer = 0f;
    }

    public void DisableCombatMode()
    {
        Debug.Log("Combat OFF");
        combatMode = false;
        agent.updateRotation = true;
    }

    public void SeeAttack()
    {
        attackScript.TryToBlock();
    }

    public void DamangeTaken(int hitPoints, bool penetratingAttack)
    {
        // No Minus if blocking
        if (!isBlocking || penetratingAttack)
        {
            // IMPACT ANIMATION
            anim.SetTrigger("TakeDamage");

            currentHealth -= hitPoints;
        }
        else // success block
        {
            // BLOCK IMPACT
            anim.SetTrigger("BlockHit");
        }

        // Death check
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void HitByMelee(int hitPoints)
    {
        if (hitCauseAlert && currentHealth > 0) detectionScript.InstantAggroMelee(); //pass player location and alert
        DamangeTaken(hitPoints, false);
    }
    public void HitByMelee()
    {
        HitByMelee(1);
    }

    public void HitByHeavyMelee(int hitPoints, float stunDuration) //+ STUNNED
    {
        if (hitCauseAlert && currentHealth > 0) detectionScript.InstantAggroMelee(); //pass player location and alert

        Debug.Log("IM STUNNED"!);

        Stun(stunDuration);
        DamangeTaken(hitPoints, true);
    }
    public void HitByHeavyMelee()
    {
        HitByHeavyMelee(1, 1);
    }

    public void HitByRange(int hitPoints, bool canInstaKill)
    {
        if (canInstaKill)
        {
            if (detectionScript.currentState == DetectionState.Alerted) DamangeTaken(hitPoints, true); //range is penetraing (not affected by block)
            else Die();
        }
        else
        {
            DamangeTaken(hitPoints, true); //range is penetraing (not affected by block)
        }

        //InstantAggroRange alerts enemies but doesn't pass player info
        if (hitCauseAlert && currentHealth > 0) detectionScript.InstantAggroRange(); // AggroRange is different
    }
    public void HitByRange()
    {
        HitByRange(1, false);
    }

    public void Stun(float duration)
    {
        PauseActivity();
        if (agent.enabled) agent.isStopped = true; // fix while moving, they get stunned, stops them from moving

        isStunned = true;
        
        if (duration >= stunnedDuration) stunnedDuration = duration; // Overwrite if its a stronger stun
        
        stunnedTimer = 0f;
    }

    public void Die()
    {
        //ANIMATION
        anim.SetBool("isDead", true);
        anim.SetTrigger("Dieded");

        currentHealth = 0; // Make sure health is ded
        isDead = true;

        agent.isStopped = true;
        isActivityPaused = true;
        isExecutingActivity = false;

        agent.enabled = false;

        //Disable collider
        GetComponent<Collider>().excludeLayers = LayerMask.GetMask("Player");

        Destroy(gameObject, despawnTime);
    }

    public void SpeedMult(float rate)
    {
        movementSpeed = baseMovementSpeed * rate;

        if (agent == null) Debug.LogWarning("No NavMesh Agent found! Can't update Agent Speed");
        else agent.speed = movementSpeed;
    }
    public void RotationMult(float rate)
    {
        rotationSpeed = baseRotationSpeed * rate;

        if (agent == null) Debug.LogWarning("No NavMesh Agent found! Can't update Agent Rotation Speed");
        else agent.angularSpeed = rotationSpeed;
    }

    public void ContinueActivity()
    {
        if (agent.pathPending) agent.isStopped = true;
        agent.isStopped = false;
        isExecutingActivity = false;
        isActivityPaused = false;
    }
    public void PauseActivity()
    {
        isActivityPaused = true;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if ((smokeLayer.value & (1 << collider.gameObject.layer)) != 0) // Code for checking if collider layer is smoke layer
        {
            EnteredSmoke(collider);
            Debug.Log("Enemy entered smoke.");
        }
    }
    public void EnteredSmoke(Collider collider)
    {
        PauseActivity();
        if (agent.enabled) agent.isStopped = true; // fix while moving, they get somked

        inSmoke = true;
        isChoking = true; // start choke in smoke
        chokingTimer = 0f; // Reset timer jic

        // choke until smoke ends + after smoke effects
        SmokeBomb smokeScript = collider.GetComponent<SmokeBomb>();
        if (smokeScript != null)
        {
            chokingDuration = Mathf.Max(chokingDuration, smokeScript.durationRemaining + smokeChokeAfterDisappear); // if a weaker smoke is entered,
                                                                                                                    // won't decrease stronger smoke time
        }
        else
        {
            Debug.LogWarning("SmokeBomb script not found on smoke collider!");
        }

        overlappingSmokes++;
    }
    // Either exit trigger, or smoke ends, calls OnSmokeDisabled()
    private void OnTriggerExit(Collider collider)
    {
        if ((smokeLayer.value & (1 << collider.gameObject.layer)) != 0) // Code for checking if collider layer is smoke layer
        {
            if (agent.enabled) agent.isStopped = false; // fix while moving, they get smoked
            overlappingSmokes--;

            if (overlappingSmokes <= 0)
            {
                inSmoke = false;

                //Fix smoke moved
                if (isChoking && !inSmoke) chokingDuration = smokeChokeAfterDisappear;
            }
        }
    }
    public void OnSmokeDisabled()
    {
        if (agent.enabled) agent.isStopped = false; // fix while moving, they get smoked
        overlappingSmokes--;

        if (overlappingSmokes <= 0)
        {
            inSmoke = false;

            //Fix smoke moved
            if (isChoking && !inSmoke) chokingDuration = smokeChokeAfterDisappear;
        }
    }

    public void HeardNoise(Transform playerTransform, bool isWalking)
    {
        detectionScript.HeardNoise(playerTransform, isWalking);
    }

    public bool HasLineOfSight(Transform target)
    {
        RaycastHit hit;
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, directionToTarget, out hit, detectionScript.GetCurrentDetectionDistance()))
        {
            Debug.DrawRay(transform.position, directionToTarget * detectionScript.GetCurrentDetectionDistance(), Color.red, 0.01f); // DISABLE-ABLE disable disablable
            return hit.transform == target;
        }
        return false;
    }

    public bool HasClearLineOfSightForRepositionOrStrafe(Vector3 targetPosition)
    {
        RaycastHit hit;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized; // Direction to the target position

        // Perform a raycast in the direction the enemy wants to move
        if (Physics.Raycast(transform.position, directionToTarget, out hit, detectionScript.GetCurrentDetectionDistance(), surfacesLayer))
        {
            // Visualize the raycast (optional, for debugging)
            Debug.DrawRay(transform.position, directionToTarget * detectionScript.GetCurrentDetectionDistance(), Color.green, 0.1f);

            // If we hit something, return false (meaning the path is blocked)
            return false;
        }

        // If no obstacles are detected, return true (line-of-sight is clear)
        return true;
    }

    public bool HasReachedDestination()
    {
        if (!agent.pathPending && !isDead) // Ensure the path is ready
        {
            // Check if the agent is close enough to its destination
            float remainingDistance = agent.remainingDistance;
            return remainingDistance != Mathf.Infinity && remainingDistance <= agent.stoppingDistance;
        }
        return false;
    }
}
