using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    public Transform player;

    private void Update()
    {
        if (player != null) transform.LookAt(player);
        else
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            if (player == null) Debug.LogWarning("No Player Tagged!");
        }
    }
}
