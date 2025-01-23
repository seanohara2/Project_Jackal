using System.Collections;
using UnityEngine;

public class InspectMenuManager : MonoBehaviour
{
    public GameObject inspectMenu; // Reference to the Inspect Menu GameObject
    public JackalControllerNoCamera jackalControllerNoCamera; // Reference to the JackalControllerNoCamera script
    public Rigidbody jackalRigidbody; // Reference to the Rigidbody of the robot
    public ResetPositionOnTrigger resetPosition; // Reference to the ResetPositionOnTrigger script

    private bool wasInspectMenuActive = false; // Tracks if the inspect menu was active in the previous frame

    void Update()
    {
        if (inspectMenu != null && jackalControllerNoCamera != null && jackalRigidbody != null && resetPosition != null)
        {
            if (inspectMenu.activeSelf)
            {
                if (!wasInspectMenuActive)
                {
                    EnterInspectMenu();
                }
            }
            else
            {
                if (wasInspectMenuActive)
                {
                    StartCoroutine(ExitInspectMenu());
                }
            }
        }
    }

    private void EnterInspectMenu()
    {
        // Disable robot controls and physics
        jackalControllerNoCamera.enabled = false; // Disable the controller script
        jackalRigidbody.isKinematic = true; // Disable Rigidbody physics
        //jackalRigidbody.velocity = Vector3.zero; // Stop any current velocity
        //jackalRigidbody.angularVelocity = Vector3.zero; // Stop any angular velocity
        wasInspectMenuActive = true;
    }

    private IEnumerator ExitInspectMenu()
    {
        // Reset the robot's position and rotation
        resetPosition.ResetRobotPosition();

        // Ensure the position is reset before re-enabling
        yield return new WaitForEndOfFrame();

        // Re-enable robot controls and physics
        // Make sure physics is enabled first
        jackalRigidbody.isKinematic = false;

        // Zero out velocities while dynamic
        jackalRigidbody.velocity = Vector3.zero;
        jackalRigidbody.angularVelocity = Vector3.zero;

        // Now make it kinematic
        jackalRigidbody.isKinematic = true;

        jackalControllerNoCamera.enabled = true; // Enable the controller script

        wasInspectMenuActive = false;

        Debug.Log("Robot controls re-enabled after reset.");
    }
}
