using UnityEngine;

public class PlayerNoise : MonoBehaviour
{
    [Header("Noise Settings")]
    public float walkNoiseRadius = 5f;
    public float runNoiseRadius = 10f;

    [Header("References (Auto)")]
    public Player playerScript;
    public Transform playerTransform;

    void Start()
    {
        playerScript = GetComponentInParent<Player>();
        if (playerScript == null) Debug.LogWarning("No playerScript reference!");

        if (playerScript != null) playerTransform = playerScript.GetComponent<Transform>();
    }

    public void NotifyEnemies(bool isWalking)
    {
        // Determine noise radius based on walking or running
        float noiseRadius = isWalking ? walkNoiseRadius : runNoiseRadius;

        // Find all colliders within the noise radius on the enemy layer
        Collider[] hitColliders = Physics.OverlapSphere(playerTransform.position, noiseRadius, playerScript.enemyLayer);

        foreach (Collider collider in hitColliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead)
            {
                enemy.HeardNoise(playerTransform, isWalking);
            }
        }
    }

    void OnDrawGizmos() // DISABLE-ABLE disableable
    {
        if (playerScript != null && playerScript.isWalking)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, walkNoiseRadius);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, runNoiseRadius);
        }
    }
}
