using UnityEngine;

public class RotateOnStart : MonoBehaviour
{
    public enum RotateDirection
    {
        X, Y, Z
    }

    public float rotation = 90f;
    public RotateDirection rotateDirection = RotateDirection.Y;

    void Start()
    {
        Vector3 rotationAxis = Vector3.zero;
        switch (rotateDirection)
        {
            case RotateDirection.X:
                rotationAxis = Vector3.right;
                break;
            case RotateDirection.Y:
                rotationAxis = Vector3.up;
                break;
            case RotateDirection.Z:
                rotationAxis = Vector3.forward;
                break;
        }

        // Apply the rotation
        transform.Rotate(rotationAxis * rotation);
    }
}
