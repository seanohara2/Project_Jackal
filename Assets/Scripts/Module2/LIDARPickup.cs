using UnityEngine;

public class LiDARPickup : MonoBehaviour
{
    [SerializeField] private GameObject lidarUIPanel;            // UI Panel for LiDAR instructions
    [SerializeField] private GameObject floatingLidarComponent; // Floating LiDAR pickup visual
    [SerializeField] private GameObject lidarComponentOnRobot;  // LiDAR component on the robot
    [SerializeField] private LiDARController lidarController;   // Reference to LiDARController script

    private bool isDisplayingLiDARInstructions = false;         // Flag for displaying instructions
    private bool lidarActivated = false;                       // Tracks if LiDAR is activated

    private void Start()
    {
        // Ensure UI and components are correctly initialized
        if (lidarUIPanel != null) lidarUIPanel.SetActive(false);
        if (floatingLidarComponent != null) floatingLidarComponent.SetActive(true);
        if (lidarComponentOnRobot != null) lidarComponentOnRobot.SetActive(false);

        // Disable the LiDAR Controller script until pickup is collected
        if (lidarController != null)
        {
            lidarController.enabled = false;
            Debug.Log("LiDAR Controller script disabled at start.");
        }
        else
        {
            Debug.LogError("LiDARController is not assigned in the Inspector.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Disable the floating pickup visual
            if (floatingLidarComponent != null)
            {
                floatingLidarComponent.SetActive(false);
                Debug.Log("Floating LiDAR component disabled.");
            }

            // Display LiDAR instructions
            DisplayLiDARInstructions();
        }
    }

    private void Update()
    {
        // Allow the player to dismiss instructions
        if (isDisplayingLiDARInstructions && Input.anyKeyDown)
        {
            CloseLiDARInstructions();
        }
    }

    private void DisplayLiDARInstructions()
    {
        isDisplayingLiDARInstructions = true;
        if (lidarUIPanel != null)
        {
            lidarUIPanel.SetActive(true);
            Debug.Log("LiDAR instructions displayed.");
        }
    }

    private void CloseLiDARInstructions()
    {
        isDisplayingLiDARInstructions = false;

        if (lidarUIPanel != null)
        {
            lidarUIPanel.SetActive(false);
            Debug.Log("LiDAR instructions closed.");
        }

        ActivateLiDARController();
    }

    private void ActivateLiDARController()
    {
        // Enable the LiDAR Controller
        if (lidarController != null && !lidarActivated)
        {
            lidarController.enabled = true;
            lidarActivated = true;
            Debug.Log("LiDAR Controller script activated.");
        }

        if (lidarComponentOnRobot != null)
        {
            lidarComponentOnRobot.SetActive(true);
            Debug.Log("LiDAR activated on the robot.");
        }

        // Destroy the pickup object
        Destroy(gameObject);
    }
}
