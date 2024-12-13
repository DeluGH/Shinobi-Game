using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Processors;
using static UnityEngine.GraphicsBuffer;

public class ActivityAI : MonoBehaviour
{
    [Header("AI Behavior")]
    public List<Activity> Activities; // List of activities
    public int currentActivityIndex = 0; // Tracks which activity is being executed
    public int currentPathIndex = 0; // Tracks current pathpoint for GoTo activities

    [Header("References (Auto Assign)")]
    public Enemy enemyScript;

    private void Start()
    {
        enemyScript = GetComponentInParent<Enemy>();
        if (enemyScript == null) Debug.LogWarning("No Enemy Script found!");
    }

    private void Update()
    {
        if (enemyScript.canEnemyPerform() && !enemyScript.isActivityPaused) // Only run activities if not paused
        {
            RunActivity();
        }
    }
    

    private void RunActivity()
    {
        if (enemyScript.isExecutingActivity || Activities.Count == 0) return; // Do nothing if already executing or no activities defined

        Activity currentActivity = Activities[currentActivityIndex];
        if (currentActivity != null)
        {
            switch (currentActivity.Type)
            {
                case Activity.ActivityType.GoTo:
                    StartCoroutine(GoTo(currentActivity));
                    break;

                case Activity.ActivityType.Idle:
                    StartCoroutine(Idle(currentActivity));
                    break;
            }
        }
        else Debug.LogWarning("No Activity Given to This Enemy!\nIf intentional, remove ActivityAI script.");
    }

    private IEnumerator GoTo(Activity activity)
    {
        enemyScript.isExecutingActivity = true;

        // Check if path points are defined
        if (activity.pathPoints == null || activity.pathPoints.Count == 0)
        {
            Debug.LogWarning("No path points defined for GoTo activity.");
            NextActivity();  // Skip to the next activity
            enemyScript.isExecutingActivity = false;
            yield break;  // Exit the coroutine early
        }

        while (currentPathIndex < activity.pathPoints.Count)
        {
            Transform target = activity.pathPoints[currentPathIndex].transform;
            enemyScript.agent.SetDestination(target.position);

            // Wait until the enemy reaches the current pathpoint
            while (!enemyScript.isActivityPaused && Vector3.Distance(transform.position, target.position) > enemyScript.maxDistanceFromNodes)
                yield return null;

            if (enemyScript.isActivityPaused) yield break; // Stop activity if paused

            currentPathIndex++;
        }

        currentPathIndex = 0; // Reset path index for next activity
        NextActivity();
        enemyScript.isExecutingActivity = false;
    }

    private IEnumerator Idle(Activity activity)
    {
        enemyScript.isExecutingActivity = true;

        // Check if idle point is defined
        if (activity.idlePoint == null)
        {
            Debug.LogWarning("No idle point defined for Idle activity.");
            NextActivity();  // Skip to the next activity
            enemyScript.isExecutingActivity = false;
            yield break;  // Exit the coroutine early
        }

        // Move to the idle point if specified and not already there
        if (activity.idlePoint != null && Vector3.Distance(transform.position, activity.idlePoint.transform.position) > enemyScript.maxDistanceFromNodes)
        {
            enemyScript.agent.SetDestination(activity.idlePoint.transform.position);

            while (!enemyScript.isActivityPaused && Vector3.Distance(transform.position, activity.idlePoint.transform.position) > enemyScript.maxDistanceFromNodes)
                yield return null;

            if (enemyScript.isActivityPaused) yield break; // Stop activity if paused
        }

        // If idleLookAt is specified, rotate to face it on the Y-axis
        if (activity.idleLookAt != null)
        {
            Vector3 targetPosition = new Vector3(activity.idleLookAt.transform.position.x, transform.position.y, activity.idleLookAt.transform.position.z); // Keep current Y position
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - transform.position);

            // Smoothly rotate towards the idleLookAt GameObject on the Y-axis
            while (!enemyScript.isActivityPaused && Quaternion.Angle(transform.rotation, targetRotation) > 1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * enemyScript.rotationSpeed);
                yield return null;
            }

            if (enemyScript.isActivityPaused) yield break; // Stop activity if paused
        }

        // Wait for the specified duration
        float timer = activity.idleTime;
        while (!enemyScript.isActivityPaused && timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        if (!enemyScript.isActivityPaused) NextActivity(); // Only proceed if not paused
        enemyScript.isExecutingActivity = false;
    }

    private void NextActivity()
    {
        currentActivityIndex = (currentActivityIndex + 1) % Activities.Count; // Loop back to the start after the last activity
    }

    private void OnDrawGizmosSelected() // DISABLE-ABLE disable disablable
    {
        // Visualize pathpoints in the editor for debugging
        Gizmos.color = Color.green;
        foreach (var activity in Activities)
        {
            if (activity.Type == Activity.ActivityType.GoTo && activity.pathPoints != null)
            {
                for (int i = 0; i < activity.pathPoints.Count - 1; i++)
                {
                    Gizmos.DrawLine(activity.pathPoints[i].transform.position, activity.pathPoints[i + 1].transform.position);
                }
            }
        }

        // Visualize AI's facing direction with an arrow
        Gizmos.color = Color.magenta; // Set color for direction
        Vector3 forwardDirection = transform.forward * 4; // 2 units in front of the AI
        Gizmos.DrawRay(transform.position, forwardDirection); // Draw the arrow
    }
}
