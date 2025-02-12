using DynamicPhotoCamera;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FLIRCameraController : MonoBehaviour
{
    [SerializeField] public Camera flirCamera;
    [SerializeField] private GameObject flirCameraUI;
    [SerializeField] private GameObject mainGameUI;
    [SerializeField] private CameraBehavior cameraBehavior;   // <-- Script that has captureCursor
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoom = 15f;
    [SerializeField] private float maxZoom = 60f;
    [SerializeField] private float sensitivity = 5f;
    [SerializeField] private TextMeshProUGUI zoomText;
    [SerializeField] private ThermalCameraScript thermalCameraScript;
    [SerializeField] private LiDARController lidarController; // Reference to the LiDAR controller

    [SerializeField] private InputController inputController;

    public bool isCameraActive = false;
    private bool isThermalViewActive = false;
    private bool isCursorUnlocked = false; // NEW: Tracks if the cursor is unlocked in camera mode
    private Vector3 smoothVelocity;
    private float zoomLevel;

    private void Start()
    {
        // Set an initial zoom level within bounds
        zoomLevel = (minZoom + maxZoom) / 2;

        // Set up initial camera states
        if (flirCamera != null)
        {
            flirCamera.gameObject.SetActive(false);
            flirCamera.fieldOfView = zoomLevel;
        }
        else
        {
            Debug.LogError("FLIR Camera not assigned in the inspector.");
        }

        // Initially hide both UIs
        if (flirCameraUI != null) flirCameraUI.SetActive(false);
        if (mainGameUI != null) mainGameUI.SetActive(true);
    }

    private void Update()
    {
        // Toggle FLIR camera with the C key or B button on the controller
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            ToggleFLIRCamera();
        }

        // If we're in FLIR camera mode, handle input
        if (isCameraActive)
        {
            // 1) Allow toggling the cursor with Tab (or another key/button)
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                isCursorUnlocked = !isCursorUnlocked;
                ApplyCursorLockState();
            }

            // 2) Handle camera controls & zoom
            HandleCameraControls();
            HandleZoomControls();
        }
    }

    private void ToggleFLIRCamera()
    {
        isCameraActive = !isCameraActive;
        flirCamera.gameObject.SetActive(isCameraActive);
        flirCameraUI.SetActive(isCameraActive);

        if (isCameraActive)
        {
            mainGameUI.SetActive(false);
            flirCamera.fieldOfView = zoomLevel; // Ensure zoom level is correctly applied

            // Disable the LiDAR controller
            if (lidarController != null)
            {
                lidarController.enabled = false;
            }

            // When we first enter camera mode, lock the cursor by default
            isCursorUnlocked = false;
            ApplyCursorLockState();
        }
        else
        {
            mainGameUI.SetActive(true);

            // Disable the thermal effect if we were using it
            if (thermalCameraScript != null)
            {
                thermalCameraScript.DisableThermalView();
            }

            // Re-enable the LiDAR controller
            if (lidarController != null)
            {
                lidarController.enabled = true;
            }

            // Unlock the cursor since we're leaving camera mode
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (inputController != null)
            {
                inputController.enabled = true;
            }
        }
    }

    private void ApplyCursorLockState()
    {
        // Only applies if camera is active
        if (isCameraActive)
        {
            if (isCursorUnlocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                // Disable the InputController so player can't move with controller
                if (inputController != null)
                {
                    inputController.enabled = false;
                }
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Re-enable the InputController for movement
                if (inputController != null)
                {
                    inputController.enabled = true;
                }
            }
        }
        else
        {
            // Outside camera mode, everything is free/unlocked by default
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (inputController != null)
            {
                inputController.enabled = true;
            }
        }
    }

    private void ToggleThermalMode()
    {
        isThermalViewActive = !isThermalViewActive;

        if (thermalCameraScript != null)
        {
            if (isThermalViewActive)
            {
                thermalCameraScript.EnableThermalView();  // Enable the thermal effect
            }
            else
            {
                thermalCameraScript.DisableThermalView();  // Disable the thermal effect
            }
        }
    }

    private void HandleCameraControls()
    {
        // Right joystick for rotation
        float joystickX = Input.GetAxis("RightStickHorizontal") * sensitivity;
        float joystickY = Input.GetAxis("RightStickVertical") * sensitivity; // Inverted movement for the joystick Y-axis

        // Mouse rotation
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = -Input.GetAxis("Mouse Y") * sensitivity; // Invert mouse Y movement

        Vector3 rotationInput;

        // Use joystick input if present, otherwise mouse input
        if (Mathf.Abs(joystickX) > 0.1f || Mathf.Abs(joystickY) > 0.1f)
        {
            rotationInput = new Vector3(joystickY, joystickX, 0);
        }
        else
        {
            rotationInput = new Vector3(mouseY, mouseX, 0);
        }

        // Smoothly update camera rotation
        Vector3 targetRotation = flirCamera.transform.eulerAngles + rotationInput;
        flirCamera.transform.eulerAngles = Vector3.SmoothDamp(
            flirCamera.transform.eulerAngles,
            targetRotation,
            ref smoothVelocity,
            0.05f
        );

        // Manually position capture cursor at screen center (if desired)
        if (!isCursorUnlocked && cameraBehavior != null && cameraBehavior.captureCursor != null)
        {
            Vector2 centerPos = new Vector2(Screen.width / 2f, Screen.height / 2f);
            cameraBehavior.captureCursor.transform.position = centerPos;
        }
    }

    private void HandleZoomControls()
    {
        // Use Right Bumper RB/R1 for zooming in, Left Bumper LB/L1 for zooming out
        float zoomInInput = Input.GetButton("RightBumper") ? 1f : 0f;
        float zoomOutInput = Input.GetButton("LeftBumper") ? 1f : 0f;

        // Zoom using scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // Zoom in
        if ((zoomInInput > 0f || scrollInput > 0f) && flirCamera.fieldOfView > minZoom)
        {
            zoomLevel = Mathf.Clamp(
                zoomLevel - zoomSpeed * Time.deltaTime * (zoomInInput > 0 ? 1 : scrollInput * 100),
                minZoom,
                maxZoom
            );
        }
        // Zoom out
        else if ((zoomOutInput > 0f || scrollInput < 0f) && flirCamera.fieldOfView < maxZoom)
        {
            zoomLevel = Mathf.Clamp(
                zoomLevel + zoomSpeed * Time.deltaTime * (zoomOutInput > 0 ? 1 : -scrollInput * 100),
                minZoom,
                maxZoom
            );
        }

        // Smoothly apply the zoom level
        flirCamera.fieldOfView = Mathf.Lerp(flirCamera.fieldOfView, zoomLevel, 0.3f);
        UpdateZoomUI();
    }

    private void UpdateZoomUI()
    {
        if (zoomText != null)
        {
            float zoomPercentage = 100f * (1f - (flirCamera.fieldOfView - minZoom) / (maxZoom - minZoom));
            zoomText.text = $"Zoom: {zoomPercentage:F1}%";
        }
    }

    public bool IsCameraActive()
    {
        return isCameraActive;
    }
}
