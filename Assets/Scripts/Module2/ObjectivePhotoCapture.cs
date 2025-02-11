using UnityEngine;
using UnityEngine.UI;

public class ObjectivePhotoCapture : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayer; // Layer for target objects
    [SerializeField] private float maxCaptureDistance = 1000f; // Max distance for valid capture
    [SerializeField] private Color readyToCaptureColor = Color.green; // Color when ready to capture
    [SerializeField] private Color defaultCursorColor = Color.white; // Default cursor color
    [SerializeField] private Transform debugCursor; // Optional: Cursor object to visualize hits

    public Image captureCursor;
    private bool isCaptureReady;
    private FLIRCameraController cameraController;

    [SerializeField] private Text captureNotificationText; // Text UI for notification
    private float notificationDisplayTime = 3f; // Time for notification to show
    private float notificationTimer;

    private void Start()
    {
        cameraController = FindObjectOfType<FLIRCameraController>();

        if (cameraController == null || cameraController.flirCamera == null)
        {
            Debug.LogError("FLIR camera not found. Please assign a Camera component in FLIRCameraController.");
            return;
        }

        if (captureCursor != null)
        {
            captureCursor.color = defaultCursorColor; // Default cursor color
        }

        if (captureNotificationText != null)
        {
            captureNotificationText.gameObject.SetActive(false); // Hide notification text initially
        }

        if (debugCursor != null)
        {
            debugCursor.gameObject.SetActive(false); // Hide debug cursor initially
        }
    }

    private void Update()
    {
        if (cameraController != null && cameraController.IsCameraActive())
        {
            UpdateCursorAndRaycast();

            // Capture photo when ready and A button is pressed
            if (isCaptureReady && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton0))) // Left-click or A button
            {
                CapturePhoto();
            }

            // Display notification for a limited time
            if (notificationTimer > 0f)
            {
                notificationTimer -= Time.deltaTime;
                if (notificationTimer <= 0f)
                {
                    HideNotification();
                }
            }
        }
    }

    private void UpdateCursorAndRaycast()
    {
        if (cameraController.flirCamera == null) return;

        isCaptureReady = false;
        captureCursor.color = defaultCursorColor; // Reset cursor color

        // Cast a ray from the camera to where the cursor is pointing
        Ray ray = cameraController.flirCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * maxCaptureDistance, Color.yellow); // Debug: Visualize raycast in Scene view

        // Perform the raycast, but check if we're hitting the correct object and layer
        if (Physics.Raycast(ray, out hit, maxCaptureDistance, targetLayer))
        {
            if (hit.collider != null)
            {
                isCaptureReady = true;
                captureCursor.color = readyToCaptureColor;

                // Move the debug cursor to the hit point
                if (debugCursor != null)
                {
                    debugCursor.gameObject.SetActive(true);
                    debugCursor.position = hit.point;
                }

                Debug.Log($"Cursor is over target: {hit.collider.gameObject.name} at distance {hit.distance}.");
            }
        }
        else
        {
            // Raycast hit nothing or an invalid object
            Debug.Log("Raycast did not hit any valid object.");
            if (debugCursor != null)
            {
                debugCursor.gameObject.SetActive(false);
            }
        }
    }

    private void CapturePhoto()
    {
        if (isCaptureReady)
        {
            Debug.Log("Photo captured of objective.");

            // Show capture notification
            ShowNotification("Photo taken of " + GetTargetName());

            // Additional logic for tracking objectives can go here
        }
    }

    private void ShowNotification(string message)
    {
        if (captureNotificationText != null)
        {
            captureNotificationText.gameObject.SetActive(true);
            captureNotificationText.text = message;
            notificationTimer = notificationDisplayTime; // Set timer to hide notification
        }
    }

    private void HideNotification()
    {
        if (captureNotificationText != null)
        {
            captureNotificationText.gameObject.SetActive(false);
        }
    }

    private string GetTargetName()
    {
        Ray ray = cameraController.flirCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxCaptureDistance, targetLayer))
        {
            return hit.collider.gameObject.name; // Return the name of the object hit
        }
        return "Unknown Object";
    }
}
