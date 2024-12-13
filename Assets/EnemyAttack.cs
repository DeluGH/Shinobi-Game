using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAttack : MonoBehaviour
{
    [Header("Combat Settings")]
    public float attackRange = 3f;
    public float attackAngle = 80f;
    public float attackAnimTime = 0.3f; // Wind Up before the swing time
    public float minAttackInterval = 3f; // Random time between attacks
    public float maxAttackInterval = 8f; // Random time between attacks
    public float blockTime = 2f; // Block duration, how long the player's attack is deflected

    public float minRepositionInterval = 10f; // How often should enemies attempt to reposition?
    public float maxRepositionInterval = 20f; // How often should enemies attempt to reposition?

    public float minStrafeInterval = 2f; // How often should enemies attempt to reposition?
    public float maxStrafeInterval = 5f; // How often should enemies attempt to reposition?

    [Header("Debug Reference")]
    public bool attackAvailable = false; // True: attack cooldown is complete
    public float attackWaitTime = 0f;
    public float attackWaitTimer = 0f;

    public bool canReposition = false; // True: reposition cooldown is complete
    public float repositionTime = 0f;
    public float repositionTimer = 0f;

    public bool canStrafe = false; // True: strafe cooldown is complete
    public float strafeTime = 0f;
    public float strafeTimer = 0f;

    [Header("References (Auto)")]
    public Enemy enemyScript;

    private void Start()
    {
        enemyScript = GetComponentInParent<Enemy>();
        if (enemyScript == null) Debug.LogWarning("No enemyScript detected!");

        attackWaitTime = GetAttackWait();
        repositionTime = GetRepositionWait();

    }

    private void Update()
    {
        if (enemyScript.combatMode && enemyScript.canEnemyPerform()) // not dead, not stunned, not choking, not attacking, not blocking
        {
            //ATTACK TIMER MANAGER
            if (!attackAvailable)
            {
                if (attackWaitTimer < attackWaitTime) attackWaitTimer += Time.deltaTime;
                else attackAvailable = true;
            }

            //REPOSITION TIMER MANAGER
            if (enemyScript.smartReposition && !canReposition)
            {
                if (repositionTimer < repositionTime) repositionTimer += Time.deltaTime;
                else canReposition = true;
            }

            //STRAFE TIMER MANAGER
            if (enemyScript.canStrafe && !canStrafe)
            {
                if (strafeTimer < strafeTime) strafeTimer += Time.deltaTime;
                else canStrafe = true;
            }

            if (attackAvailable)
            {
                //ATTACK
                enemyScript.isAttacking = true;
                attackWaitTimer = 0f;

                // Feature: moves a little forward to attack
                if (Vector3.Distance(enemyScript.player.transform.position, transform.position) > attackRange)
                {
                    Vector3 directionToPlayer = (enemyScript.player.transform.position - transform.position).normalized;
                    Vector3 destination = transform.position + directionToPlayer * attackRange;
                    enemyScript.agent.SetDestination(destination);
                }

                StartCoroutine(PerformAttack());
            }
            else if (canReposition)
            {
                //POSITIONING AMONGST OTHER ENEMIES
                // If can't attack, try to move positions
                enemyScript.Reposition();

                canReposition = false;
                repositionTimer = 0f;
                repositionTime = GetRepositionWait();
            }
            else if (canStrafe)
            {
                //STRAFE L/R
                enemyScript.Strafe();

                canStrafe = false;
                strafeTimer = 0f;
                strafeTime = GetStrafeWait();
            }
        }
    }

    private IEnumerator PerformAttack()
    {
        // Wait for the attack animation time
        yield return new WaitForSeconds(attackAnimTime);

        // Check for players within range and angle
        Collider[] hitPlayers = Physics.OverlapSphere(transform.position, attackRange, enemyScript.playerLayer);
        foreach (Collider player in hitPlayers)
        {
            Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

            if (angleToPlayer <= attackAngle / 2)
            {
                Debug.Log($"Hit player: {player.name}");
            }
        }

        enemyScript.isAttacking = false;
        attackAvailable = false;
        attackWaitTime = GetAttackWait(); // Get Random Wait Time
    }

    public float GetAttackWait()
    {
        return Random.Range(minAttackInterval, maxAttackInterval);
    }
    public float GetRepositionWait()
    {
        return Random.Range(minRepositionInterval, maxRepositionInterval);
    }
    public float GetStrafeWait()
    {
        return Random.Range(minStrafeInterval, maxStrafeInterval);
    }

    public void TryToBlock()
    {
        if (enemyScript.canBlock && enemyScript.canEnemyPerform())
        {
            StartCoroutine(PerformBlock());
        }
    }

    private IEnumerator PerformBlock()
    {
        enemyScript.isBlocking = true;

        //BLOCKING ANIMATION

        // Block for the specified duration
        Debug.Log("Enemy is blocking");

        yield return new WaitForSeconds(blockTime);

        enemyScript.isBlocking = false;
        Debug.Log("Enemy stopped blocking");
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the attack range sphere in the editor for visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw the attack angle arc
        Gizmos.color = Color.yellow;
        Vector3 forward = transform.forward * attackRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2, 0) * forward;
        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);
    }
}
