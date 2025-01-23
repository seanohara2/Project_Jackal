using UnityEngine;

public class ResetPositionOnTrigger : MonoBehaviour
{
    public Transform robot; // Reference to the robot's Transform
    private Vector3 initialPosition; // Store the robot's initial position
    private Quaternion initialRotation; // Store the robot's initial rotation

    private void Start()
    {
        // Save the robot's initial position and rotation
        if (robot != null)
        {
            initialPosition = robot.position;
            initialRotation = robot.rotation;
        }
        else
        {
            Debug.LogError("Robot Transform not assigned!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the robot entered the trigger
        if (other.transform == robot)
        {
            ResetRobotPosition();
        }
    }

    public void ResetRobotPosition()
    {
        if (robot == null)
        {
            Debug.LogError("Robot Transform is not assigned!");
            return;
        }

        // Reset position and rotation
        robot.position = initialPosition;
        robot.rotation = initialRotation;

        // Stop any motion
        Rigidbody rb = robot.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Robot successfully reset to its initial position.");
    }

}
