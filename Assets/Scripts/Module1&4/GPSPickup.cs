using UnityEngine;

public class GPSPickup : MonoBehaviour
{
    [SerializeField] private GameObject gpsUIPanel; // Reference to the GPS instruction UI panel
    [SerializeField] private GameObject miniMapUI; // Reference to the mini-map UI
    [SerializeField] private GameObject gpsComponentOnRobot; // GPS component on the robot
    [SerializeField] private GameObject floatingGPSComponent; // Floating GPS component to be disabled on pickup

    private bool isDisplayingGPSInstructions = false; // Flag to check if the GPS instructions are active

    private void Start()
    {
        // Ensure the GPS UI, Mini-map, and GPS component are initially disabled
        gpsUIPanel.SetActive(false);
        miniMapUI.SetActive(false);
        gpsComponentOnRobot.SetActive(false);

        // Ensure the floating GPS is initially active (or set based on your design)
        if (floatingGPSComponent != null)
        {
            floatingGPSComponent.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Disable the floating GPS component
            if (floatingGPSComponent != null)
            {
                floatingGPSComponent.SetActive(false);
                Debug.Log("Floating GPS component disabled.");
            }

            // Enable GPS functionality and Mini-map
            EnableGPS();

            // Display the GPS instruction panel
            DisplayGPSInstructions();
        }
    }

    private void Update()
    {
        // If GPS instructions are being displayed, wait for any button press to continue
        if (isDisplayingGPSInstructions && Input.anyKeyDown)
        {
            CloseGPSInstructions();
        }
    }

    private void DisplayGPSInstructions()
    {
        isDisplayingGPSInstructions = true;

        // Display the GPS instruction UI
        gpsUIPanel.SetActive(true);
    }

    private void CloseGPSInstructions()
    {
        isDisplayingGPSInstructions = false;

        // Hide the GPS instruction UI
        gpsUIPanel.SetActive(false);

        // Destroy the GPS pickup object now that the instructions are dismissed
        Destroy(gameObject);
    }

    private void EnableGPS()
    {
        gpsComponentOnRobot.SetActive(true); // Enable the GPS functionality
        miniMapUI.SetActive(true); // Enable the mini-map UI
        Debug.Log("GPS and Mini-map enabled.");
    }
}
