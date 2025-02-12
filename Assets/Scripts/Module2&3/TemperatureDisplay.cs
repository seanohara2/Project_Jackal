using TMPro;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class CameraBehavior : MonoBehaviour
{
    [SerializeField] private GameObject thermalCameraUI; // UI for displaying thermal information
    [SerializeField] private TextMeshProUGUI temperatureText; // UI text element for displaying temperature
    [SerializeField] private TextMeshProUGUI RGBText; // UI text element for displaying RGB value
    [SerializeField] private TextMeshProUGUI photoNotificationText; // UI text element for photo notification
    [SerializeField] private float notificationDuration = 3f; // Duration for the photo notification
    [SerializeField] private float updateInterval = 0.5f; // Interval for updating temperature display
    [SerializeField] private LayerMask targetLayer; // Layer for target objects
    [SerializeField] private Color readyToCaptureColor = Color.green; // Color when ready to capture
    [SerializeField] private Color defaultCursorColor = Color.white; // Default cursor color
    [SerializeField] private Transform debugCursor; // Optional: Cursor object to visualize hits
    [SerializeField] private AudioClip captureSound; // Sound effect for photo capture
    [SerializeField] private AudioSource audioSource; // AudioSource to play the capture sound
    [SerializeField] private float audioPlaybackDuration = 1f; // Duration to play the sound effect
    public Image captureCursor; // Reference to the cursor UI image

    private ThermalCameraScript thermalCameraScript;
    private FLIRCameraController flirCameraController;
    private float timer;
    private bool isReadyToCapture;
    private float notificationTimer;

    private void Start()
    {
        // Find the FLIR camera controller and thermal camera script
        flirCameraController = FindObjectOfType<FLIRCameraController>();
        if (flirCameraController == null || flirCameraController.flirCamera == null)
        {
            Debug.LogError("FLIRCameraController or its camera is not assigned.");
            return;
        }

        thermalCameraScript = flirCameraController.flirCamera.GetComponent<ThermalCameraScript>();
        if (thermalCameraScript == null)
        {
            Debug.LogError("ThermalCameraScript is not attached to the FLIR camera.");
            return;
        }

        // Ensure the UI is initially inactive
        if (thermalCameraUI != null)
        {
            thermalCameraUI.SetActive(false);
        }

        // Set initial cursor color
        if (captureCursor != null)
        {
            captureCursor.color = defaultCursorColor;
        }

        // Hide photo notification initially
        if (photoNotificationText != null)
        {
            photoNotificationText.gameObject.SetActive(false);
        }

        // Ensure AudioSource is assigned
        if (audioSource == null)
        {
            Debug.LogError("AudioSource is not assigned. Please assign it in the inspector.");
        }
    }

    private void Update()
    {
        if (flirCameraController != null && flirCameraController.IsCameraActive())
        {
            // Activate the thermal camera UI when the FLIR camera is active
            if (thermalCameraUI != null)
            {
                thermalCameraUI.SetActive(true);
            }

            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                UpdateTemperatureDisplay();
                timer = 0f;
            }

            // Handle photo capture
            if (isReadyToCapture && Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0)) // A button on the controller
            {
                CapturePhoto();
            }
        }
        else
        {
            // Deactivate the thermal camera UI when the FLIR camera is not active
            if (thermalCameraUI != null)
            {
                thermalCameraUI.SetActive(false);
            }
        }

        // Update photo notification visibility
        if (photoNotificationText.gameObject.activeSelf)
        {
            notificationTimer += Time.deltaTime;
            if (notificationTimer >= notificationDuration)
            {
                photoNotificationText.gameObject.SetActive(false);
            }
        }
    }

    private void UpdateTemperatureDisplay()
    {
        // Raycast from the center of the screen to detect a temperature-controlled object
        Ray ray = flirCameraController.flirCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            var tempController = hit.collider.GetComponent<TemperatureController>();
            if (tempController != null)
            {
                // Display the object's temperature and RGB values relative to the environment
                DisplayTemperature(tempController.temperature);
                DisplayRGB(hit);

                // Change cursor color to indicate object is ready for capture
                if (captureCursor != null)
                {
                    captureCursor.color = readyToCaptureColor;
                }

                isReadyToCapture = true;
            }
            else
            {
                ClearTemperatureDisplay();

                if (captureCursor != null)
                {
                    captureCursor.color = defaultCursorColor;
                }

                isReadyToCapture = false;
            }
        }
        else
        {
            ClearTemperatureDisplay();

            if (captureCursor != null)
            {
                captureCursor.color = defaultCursorColor;
            }

            isReadyToCapture = false;
        }

        // Optionally, visualize the raycast for debugging
        if (debugCursor != null)
        {
            if (hit.collider != null)
            {
                debugCursor.gameObject.SetActive(true);
                debugCursor.position = hit.point;
            }
            else
            {
                debugCursor.gameObject.SetActive(false);
            }
        }
    }

    private void CapturePhoto()
    {
        // Notify the PlayerActivityManager of the photo capture
        var playerActivityManager = FindObjectOfType<PlayerStatsManager2>();
        if (isReadyToCapture && playerActivityManager != null)
        {
            Ray ray = flirCameraController.flirCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetLayer))
            {
                var oreObject = hit.collider.gameObject; // Get the object hit by the ray
                playerActivityManager.CaptureOrePhoto(oreObject); // Pass the ore GameObject
            }
        }

        // Play the capture sound effect
        if (audioSource != null && captureSound != null)
        {
            StartCoroutine(PlaySoundForDuration(captureSound, audioPlaybackDuration));
        }

        // Display photo notification
        if (photoNotificationText != null)
        {
            photoNotificationText.text = "Photo taken of " + GetTargetName()!;
            photoNotificationText.gameObject.SetActive(true);
        }

        notificationTimer = 0f;
    }

    private IEnumerator PlaySoundForDuration(AudioClip clip, float duration)
    {
        audioSource.clip = clip;
        audioSource.Play();

        yield return new WaitForSeconds(duration);

        audioSource.Stop();
    }

    private string GetTargetName()
    {
        // Use the same raycast logic as UpdateTemperatureDisplay
        Ray ray = flirCameraController.flirCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            return hit.collider.gameObject.name; // Return the name of the object hit
        }

        return "Unknown Object"; // Fallback when no object is hit
    }

    private void DisplayTemperature(float temperature)
    {
        if (temperatureText != null)
        {
            temperatureText.text = $"Temperature: {temperature:F2}°F";
        }
    }

    private void DisplayRGB(RaycastHit hit)
    {
        if (RGBText != null)
        {
            // Extract the RGB color of the object at the hit point
            Color objectColor = hit.collider.GetComponent<Renderer>().material.color;
            RGBText.text = $"RGB: ({objectColor.r:F2}, {objectColor.g:F2}, {objectColor.b:F2})";
        }
    }

    private void ClearTemperatureDisplay()
    {
        if (temperatureText != null)
        {
            temperatureText.text = "No temperature data.";
        }

        if (RGBText != null)
        {
            RGBText.text = "No RGB data.";
        }
    }
}
