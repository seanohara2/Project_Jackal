using UnityEngine;

public class SimpleRotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 100, 0); // Rotation speed in degrees per second

    void Update()
    {
        // Rotate the object based on the rotation speed
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
