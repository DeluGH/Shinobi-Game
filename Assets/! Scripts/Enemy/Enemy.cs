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

    public float combatModeProximity = 5.5f; // Prefered distance from player
    public float combatModeOutDecayTime = 0.5f; // If player steps out, wait for this amount of time before continuing chase
    public bool combatMode = false;

    [Header("Smart Position Settings")]
    public bool smartReposition = true;
    public float enemySpacing = 3f;
    [Space(5f)]
    public bool canStrafe = true;
    public bool strafingRight = false;
    public float strafeSpeed = 4f;

    [Header("Smoked Settings")]
    public bool inSmoke = false;
    public float smokeChokeAfterDisappear = 2f; // Extra choking time after smoke disappears

    [Header("Debug Stats (Auto)")]
    public int currentHealth = 0;
    public float movementSpeed = 5f;
    public float rotationSpeed = 120f;
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

    public DetectionAI detectionScript;
    public ActivityAI activityScript;
    public EnemyAttack attackScript;

    public LayerMask playerLayer;       // Layer mask for detecting players
    public LayerMask enemyLayer;       // Layer mask for detecting enemies
    public LayerMask smokeLayer;

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
        // Find Player w Layer
        if (player == null) FindPlayer();
        if (player == null) Debug.LogWarning("Player reference is missing! Unable to FindPlayer()");
        // Others
        if (detectionScript == null) detectionScript = GetComponentInChildren<DetectionAI>();
        if (detectionScript == null) Debug.LogError("This Enemy is Missing detectionScript!");
        if (activityScript == null) activityScript = GetComponentInChildren<ActivityAI>();
        if (activityScript == null) Debug.LogError("This Enemy is Missing activityScript!");
        if (attackScript == null) attackScript = GetComponentInChildren<EnemyAttack>();
        if (attackScript == null) Debug.LogError("This Enemy is Missing attackScript!");
        if (agent == null) agent = GetComponentInChildren<NavMeshAgent>();
        if (agent == null) Debug.LogError("This Enemy is Missing NavMeshAgent! Other AI Scripts will not be able to run!");
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

        detectionScript.enabled = true;
        activityScript.enabled = true;
    }

    private void Update()
    {
        if (player == null) { FindPlayer(); return; }

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

        agent.isStopped = false; //Reenable to reposition
        Reposition();

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
            currentHealth -= hitPoints;
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

    public void HitByRange(int hitPoints)
    {
        if (hitCauseAlert && currentHealth > 0) detectionScript.InstantAggroRange(); // AggroRange is different
        //InstantAggroRange alerts enemies but doesn't pass player info
        DamangeTaken(hitPoints, true);
    }
    public void HitByRange()
    {
        HitByRange(1);
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
        currentHealth = 0; // Make sure health is ded
        isDead = true;

        agent.isStopped = true;
        isActivityPaused = true;
        isExecutingActivity = false;

        agent.enabled = false;

        // Death anim
        transform.position = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        transform.Rotate(0, 0, 90f);

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
                Debug.Log("Enemy exited all smoke zones.");
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
            Debug.Log("Smoke disabled, inSmoke reset.");
        }
    }

    public void HeardNoise(Transform playerTransform, bool isWalking)
    {
        detectionScript.HeardNoise(playerTransform, isWalking);
    }

    // COMBAT REPOSITIONING ====================================================================
    public void Reposition()
    {
        // Find nearby enemies
        List<Vector3> enemyPositions = GetNearbyEnemyPositions();

        // Find a free position around the player
        Vector3 freePosition = FindFreePosition(enemyPositions);

        // If a valid free position is found, move there
        if (freePosition != Vector3.zero)
        {
            Debug.Log("Repositioning!");
            agent.SetDestination(freePosition);
        }
        else
        {
            Debug.Log("Failed to find position!");
        }
    }

    private List<Vector3> GetNearbyEnemyPositions()
    {
        List<Vector3> positions = new List<Vector3>();

        // Use OverlapSphere to find nearby enemies
        Collider[] colliders = Physics.OverlapSphere(player.position, combatModeProximity, enemyLayer);
        foreach (var collider in colliders)
        {
            // Exclude this enemy
            if (collider.gameObject != gameObject)
            {
                positions.Add(collider.transform.position);
            }
        }

        return positions;
    }

    private Vector3 FindFreePosition(List<Vector3> enemyPositions)
    {
        // Try multiple positions around the player
        int numSamples = 36; // Divide the circle into 36 segments
        for (int i = 0; i < numSamples; i++)
        {
            float angle = (360f / numSamples) * i;
            Vector3 candidatePosition = CalculatePositionOnCircle(player.position, combatModeProximity, angle);

            // Check if this position is far enough from other enemies
            if (IsPositionFree(candidatePosition, enemyPositions))
            {
                return candidatePosition;
            }
        }

        // No valid position found
        return Vector3.zero;
    }

    private Vector3 CalculatePositionOnCircle(Vector3 center, float radius, float angle)
    {
        // Convert the angle from degrees to radians
        float radians = angle * Mathf.Deg2Rad;

        // Calculate the position on the circle
        float x = center.x + radius * Mathf.Cos(radians);
        float z = center.z + radius * Mathf.Sin(radians);
        return new Vector3(x, center.y, z); // Assume y-coordinate remains the same
    }

    private bool IsPositionFree(Vector3 position, List<Vector3> enemyPositions)
    {
        foreach (var enemyPos in enemyPositions)
        {
            if (Vector3.Distance(position, enemyPos) < enemySpacing)
            {
                return false;
            }
        }
        return true;
    }

    // STRAFING
    public void Strafe()
    {
        Debug.Log("Strafe!");
        // Calculate the direction perpendicular to the player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        Vector3 strafeDirection = Vector3.Cross(directionToPlayer, Vector3.up).normalized;

        // Decide whether to strafe left or right
        if (!strafingRight)
        {
            strafeDirection = -strafeDirection;
        }

        // Calculate the target position
        Vector3 targetPosition = transform.position + strafeDirection * strafeSpeed;

        // Use NavMeshAgent to move to the target position
        agent.SetDestination(targetPosition);
    }

    public void ToggleStrafeDirection()
    {
        // Toggle strafing direction
        strafingRight = !strafingRight;
    }
}
