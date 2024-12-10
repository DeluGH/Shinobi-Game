using System.Collections;
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
    public bool canBlock = false; //allow enemy to block
    public float blockPeriod = 1f; //block duration, how long the player's attack is deflected
    public float blockInterval = 7.5f; //block cooldown, how often the enemy can block
    [Header("Smoked Settings")]
    public bool inSmoke = false;
    public float smokeChokeAfterDisappear = 2f; // Extra choking time after smoke disappears

    [Header("Debug Stats (Auto)")]
    public int currentHealth = 0;
    public float movementSpeed = 5f;
    public float rotationSpeed = 120f;
    [Space(5f)]
    public bool isChoking = false; // Stunned
    public float chokingDuration = 0f;
    public float chokingTimer = 0f;
    [Space(5f)]
    public bool isDead = false; //Should stop ALL functions (KILL SWITCH)
    public bool isBlocking = false; // Flag for if enemy is blocking or not
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

    public LayerMask playerLayer;       // Layer mask for detecting players
    public LayerMask enemyLayer;       // Layer mask for detecting enemies
    public LayerMask smokeLayer;

    public int overlappingSmokes = 0; // Fix bug with overlapping smokes,
                                       // if 1 ends, shouldn't "unsmoke" the enemy
    

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
        if (detectionScript == null) detectionScript = GetComponent<DetectionAI>();
        if (detectionScript == null) Debug.LogError("This Enemy is Missing detectionScript!");
        if (activityScript == null) activityScript = GetComponent<ActivityAI>();
        if (activityScript == null) Debug.LogError("This Enemy is Missing activityScript!");
        if (agent == null) agent = GetComponent<NavMeshAgent>();
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
    }

    public bool canEnemyPerform()
    {
        if (isDead || isChoking) return false;
        else return true;
    }

    private void FindPlayer()
    {
        //find the player within range
        Collider[] colliders = Physics.OverlapSphere(transform.position, Mathf.Infinity, playerLayer); // Since it's a singleplayer game, it uses Mathf.Inf to find the player!!!

        if (colliders.Length > 0)
        {
            player = colliders[0].transform; //get first player found
            Debug.Log($"Player found: {player.name}");
        }
        else
        {
            Debug.Log("No player detected within the specified range.");
        }
    }

    public void DamangeTaken(int hitPoints)
    {
        // No Minus if blocking
        if (!isBlocking)
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
        // Getting hit causes aggro (make sure not dead)
        if (hitCauseAlert && currentHealth > 0) detectionScript.InstantAggroMelee();

        DamangeTaken(hitPoints);
    }
    public void HitByMelee()
    {
        HitByMelee(1);
    }

    public void HitByRange(int hitPoints)
    {
        // Getting hit causes aggro (make sure not dead)
        if (hitCauseAlert && currentHealth > 0) detectionScript.InstantAggroRange();

        DamangeTaken(hitPoints);
    }
    public void HitByRange()
    {
        HitByRange(1);
    }

    public void Die()
    {
        currentHealth = 0; // Make sure health is ded
        isDead = true;

        agent.isStopped = true;
        isActivityPaused = true;
        isExecutingActivity = false;

        agent.enabled = false;
        transform.position = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);
        transform.Rotate(0, 0, 90f);

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
}
