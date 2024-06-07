using UnityEngine;

public class FixedRotation : MonoBehaviour
{
    private Quaternion initialRotation;

    private void Start()
    {
        // Store the initial rotation of the GameObject
        initialRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        // Reset the rotation to the initial rotation
        transform.rotation = initialRotation;
    }
}
