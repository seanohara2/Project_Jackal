using UnityEngine;

public class FLIRCameraPickup : MonoBehaviour
{
    [SerializeField] private GameObject flirUIPanel;             // UI Panel for FLIR instructions
    [SerializeField] private GameObject floatingFlirComponent;   // Floating FLIR Camera pickup visual
    [SerializeField] private GameObject flirComponentOnRobot; // GPS component on the robot
    [SerializeField] private FLIRCameraController flirCameraController; // Reference to FLIRCameraController script

    private bool isDisplayingFLIRInstructions = false;          // Flag for displaying instructions
    private bool flirActivated = false;                         // Tracks if FLIR is activated

    private void Start()
    {
        // Ensure FLIR UI and floating pickup are correctly initialized
        if (flirUIPanel != null) flirUIPanel.SetActive(false);
        if (floatingFlirComponent != null) floatingFlirComponent.SetActive(true);
        if (flirComponentOnRobot != null) flirComponentOnRobot.SetActive(false);

        // Disable the FLIR Camera Controller script until pickup is collected
        if (flirCameraController != null)
        {
            flirCameraController.enabled = false;
            Debug.Log("FLIR Camera Controller script disabled at start.");
        }
        else
        {
            Debug.LogError("FLIRCameraController is not assigned in the Inspector.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Disable the floating pickup visual
            if (floatingFlirComponent != null)
            {
                floatingFlirComponent.SetActive(false);
                Debug.Log("Floating FLIR Camera component disabled.");
            }

            // Display FLIR instructions
            DisplayFLIRInstructions();
        }
    }

    private void Update()
    {
        // Allow the player to dismiss instructions
        if (isDisplayingFLIRInstructions && Input.anyKeyDown)
        {
            CloseFLIRInstructions();
        }
    }

    private void DisplayFLIRInstructions()
    {
        isDisplayingFLIRInstructions = true;
        if (flirUIPanel != null)
        {
            flirUIPanel.SetActive(true);
            Debug.Log("FLIR Camera instructions displayed.");
        }
    }

    private void CloseFLIRInstructions()
    {
        isDisplayingFLIRInstructions = false;

        if (flirUIPanel != null)
        {
            flirUIPanel.SetActive(false);
            Debug.Log("FLIR Camera instructions closed.");
        }

        ActivateFLIRController();
    }

    private void ActivateFLIRController()
    {
        // Enable the FLIR Camera Controller
        if (flirCameraController != null && !flirActivated)
        {
            flirCameraController.enabled = true;
            flirActivated = true;
            Debug.Log("FLIR Camera Controller script activated.");
        }

        if (flirComponentOnRobot != null)
        {
            flirComponentOnRobot.SetActive(true);
            Debug.Log("LiDAR activated on the robot.");
        }

        // Destroy the pickup object
        Destroy(gameObject);
    }
}
