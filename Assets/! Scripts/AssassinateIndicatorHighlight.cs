using UnityEngine;

public class AssassinateIndicatorHighlight : MonoBehaviour
{
    public GameObject target;
    public float hoverDistance = 2f;

    private void Update()
    {
        if (target != null)
        {
            HoverAboveTarget();
        }
    }

    private void HoverAboveTarget()
    {
        // Set the position of the indicator to the target's position + the hover distance on the Y-axis
        Vector3 targetPosition = target.transform.position;
        transform.position = new Vector3(targetPosition.x, targetPosition.y + hoverDistance, targetPosition.z);
    }

    // Optionally, if you want to set a new target from other parts of the game
    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }
}
