using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Debug References")]
    public GameObject lookingAtEnemy; // Enemy player is looking at

    [Header("Swinging with Sword")]
    public float meleeRange = 4f;
    public float meleeAngle = 100f;
    public float meleeCooldown = 1f;

    [Header("Assassinate")]
    public float assRange = 2.75f;
    public GameObject assEnemyTarget; // Enemy within assRange & looking at
    public bool canAssassinate = false; // use this to change UI (add ass UI on screen)
    public float behindEnemyThreshold = -0.4f; // Negative value = behind,
                                               // so -1: Player must be directly behind
                                               // -0.9: 40° Arc // -0.7: 100° Arc  // -0.5: 120° Arc

    [Header("References (Auto)")]
    public float maxRaycastRange = 10f;
    public LayerMask enemyLayer;
    public bool drawSwingGizmo = false; // Flag to draw swing gizmo
    public float meleeTimer = 0f;

    void Start()
    {
        enemyLayer = LayerMask.GetMask("Enemy");
        if (enemyLayer == 0) Debug.LogWarning("Enemy layer reference is missing!");
    }

    // Update is called once per frame
    void Update()
    {
        canAssassinate = isEnemyAssable(); // Check for any assable enemies

        if (meleeTimer < meleeCooldown) meleeTimer += Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && meleeTimer >= meleeCooldown)
        {
            meleeTimer = 0f;
            Attack();
        }
    }

    public bool isFacingEnemy()
    {
        // Perform a raycast in the forward direction
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxRaycastRange, enemyLayer))
        {
            lookingAtEnemy = hit.collider.gameObject;
            return true;
        }
        else
        {
            lookingAtEnemy = null;
            assEnemyTarget = null;
            canAssassinate = false;
            return false;
        }
    }

    private bool IsPlayerBehindEnemy(Transform enemyTransform)
    {
        Vector3 toPlayer = (transform.position - enemyTransform.position).normalized;
        return Vector3.Dot(enemyTransform.forward, toPlayer) < behindEnemyThreshold; // Adjust threshold for "behind"
    }

    public bool isEnemyAssable() // Ass-ass-inatable?
    {
        // Find looking at enemy
        if (isFacingEnemy() && lookingAtEnemy != null)
        {
            Enemy enemyScript = lookingAtEnemy.GetComponent<Enemy>();
            // 1. Need to be BEHIND to give backshots
            // 2. Need to be within Range
            // 3. Enemy can't be dead
            if (enemyScript != null)
            {
                if (!enemyScript.isDead && //3
                    enemyScript.playerInDetectionArea == false //1
                    && Vector3.Distance(transform.position, lookingAtEnemy.transform.position) <= assRange // 2
                    && IsPlayerBehindEnemy(lookingAtEnemy.transform)) //1
                {
                    assEnemyTarget = lookingAtEnemy;
                        
                    Debug.Log("Can Ass");
                    Debug.DrawLine(transform.position, lookingAtEnemy.transform.position, Color.green, 1f); //DISABLE-ABLE disableable disable

                    return true;
                }
                else
                {
                    assEnemyTarget = null;
                    return false;
                }
            }
        }

        assEnemyTarget = null;
        return false;
    }

    private void Attack()
    {
        if (canAssassinate && assEnemyTarget != null)
        {
            Assassinate();
        }
        else
        {
            Swing();
        }

    }

    public void Assassinate()
    {
        Debug.Log("ASS");

        if (assEnemyTarget != null)
        {
            Enemy enemyScript = assEnemyTarget.GetComponent<Enemy>();
            enemyScript.Die();
        }
    }

    public void Swing()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, meleeRange, enemyLayer);
        HashSet<Enemy> hitEnemySet = new HashSet<Enemy>(); // Prevent duplicate hits

        foreach (Collider enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null && !hitEnemySet.Contains(enemyScript))
            {
                Vector3 toEnemy = (enemy.transform.position - transform.position).normalized;
                if (Vector3.Angle(transform.forward, toEnemy) <= meleeAngle / 2)
                {
                    hitEnemySet.Add(enemyScript);
                    enemyScript.HitByMelee(); // HIT
                    Debug.Log($"Hit enemy: {enemyScript.name}");
                }
            }
        }

        drawSwingGizmo = true;
        StartCoroutine(ResetSwingGizmo()); //DISABLE-ABLE disableable disable
    }

    private IEnumerator ResetSwingGizmo() //DISABLE-ABLE disableable disable
    {
        yield return new WaitForSeconds(0.25f);
        drawSwingGizmo = false;
    }

    private void OnDrawGizmos() //DISABLE-ABLE disableable disable
    {
        if (drawSwingGizmo)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, meleeRange);

            // Draw angle cone
            Vector3 leftBoundary = Quaternion.Euler(0, -meleeAngle / 2, 0) * transform.forward * meleeRange;
            Vector3 rightBoundary = Quaternion.Euler(0, meleeAngle / 2, 0) * transform.forward * meleeRange;

            Gizmos.color = Color.gray;
            Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
            Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        }
    }
}
