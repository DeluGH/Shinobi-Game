using UnityEngine;
using System.Collections;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;
    private const float updateInterval = 0.1f; // 10 times per second

    private void Start()
    {
        StartCoroutine(LookAtPlayerRoutine());
    }

    private IEnumerator LookAtPlayerRoutine()
    {
        while (true)
        {
            if (player != null)
            {
                transform.LookAt(player);
            }
            else
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject.transform;
                }
                else
                {
                    Debug.LogWarning("No Player Tagged!");
                }
            }

            yield return new WaitForSeconds(updateInterval);
        }
    }
}
