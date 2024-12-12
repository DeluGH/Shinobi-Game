using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public int damage = 1;
    public bool isInstaKill = false;
    public bool canStickIfHit = true;

    [Header("References (Auto)")]
    public LayerMask enemyLayer;
    public isRotating rotateScript; //isRotating script is attached? Will stop rotating when hit

    private void Start()
    {
        enemyLayer = LayerMask.GetMask("Enemy");
        if (enemyLayer == 0) Debug.LogWarning("Enemy Layer Reference is Missing!");

        rotateScript = GetComponent<isRotating>();
        //no warning if don't have
    }

    private void OnTriggerEnter(Collider collider)
    {
        //ENEMY
        if (((1 << collider.gameObject.layer) & enemyLayer) != 0)
        {
            if (isInstaKill)Debug.Log($"Killed Enemy: {collider.gameObject.name}!");
            else Debug.Log($"Hit Enemy: {collider.gameObject.name} and dealt {damage} damage!");

            //STICK TO ENEMY (Projectile becomes their child)
            if (canStickIfHit)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                // Stick to enemies
                transform.SetParent(collider.transform, true);
                rotateScript.isRotate = false;
            }

            //Damage
            Enemy enemyScript = collider.GetComponent<Enemy>();
            if (enemyScript != null && !enemyScript.isDead)
            {
                if (isInstaKill)
                {
                    enemyScript.Die();
                }
                else
                {
                    enemyScript.HitByRange(damage);
                }
            }
        }
        else // NON ENEMIES
        {
            Debug.Log($"Obstacle Hit: {collider.gameObject.name}");
            // Stick to non enemies, walls etc
            if (canStickIfHit)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                // Stick to Walls, etc
                Vector3 hitPoint = collider.ClosestPoint(transform.position);
                transform.position = hitPoint + (transform.position - hitPoint).normalized * 0.1f;

                rotateScript.isRotate = false;
            }
        }

        transform.GetComponent<Collider>().enabled = false;
    }
}
