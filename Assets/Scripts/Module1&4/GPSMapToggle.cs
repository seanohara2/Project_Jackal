using UnityEngine;

public class GPSMapToggle : MonoBehaviour
{
    [SerializeField] private GameObject miniMapUI; // Reference to the regular mini-map UI
    [SerializeField] private GameObject fullMapUI; // Reference to the full map UI (camera output)
    private bool isFullMapActive = false; // Tracks if the full map is currently active

    private void Start()
    {
        // Ensure only the mini-map is active at the start
        miniMapUI.SetActive(true);
        fullMapUI.SetActive(false);
    }

    private void Update()
    {
        // Check for the toggle button (Square on PS4 / X on Xbox)
        if (Input.GetButtonDown("MapToggle") || Input.GetKeyDown(KeyCode.E))
        {
            ToggleMap();
        }
    }

    private void ToggleMap()
    {
        if (isFullMapActive)
        {
            // Close the full map and enable the mini-map
            fullMapUI.SetActive(false);
            miniMapUI.SetActive(true);
        }
        else
        {
            // Open the full map and disable the mini-map
            fullMapUI.SetActive(true);
            miniMapUI.SetActive(false);
        }

        // Toggle the map state
        isFullMapActive = !isFullMapActive;
    }
}
