using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Attach the Camera GameObject here.")]
    public Transform cameraTransform;
    [Tooltip("The target that the camera will follow (e.g., your car).")]
    public Transform car;

    [Header("Camera Settings")]
    [Tooltip("Distance behind the target.")]
    public float cameraDistance = 8f;
    [Tooltip("Height offset from the target.")]
    public float cameraHeight = 3f;
    [Tooltip("Speed at which the camera rotates based on input.")]
    public float cameraRotationSpeed = 100f;
    [Tooltip("Speed at which the camera follows the target.")]
    public float cameraFollowSpeed = 5f;
    [Tooltip("Speed at which the camera auto-aligns with the target when there is no input.")]
    public float cameraAutoFollowSpeed = 2f;

    [Header("Input Settings")]
    [Tooltip("Use controller input (right stick) if checked; otherwise, mouse input is used.")]
    public bool useControllerInput = false;

    // Internal variables to store the camera's current rotation angles
    private float cameraYaw;
    private float cameraPitch;
    private bool isRightStickUsed = false;

    void Start()
    {
        // Check to ensure required references are assigned.
        if (car == null)
        {
            Debug.LogError("CameraController: No target (car) assigned!");
        }
        if (cameraTransform == null)
        {
            Debug.LogError("CameraController: No camera transform assigned!");
        }

        // Initialize the camera's yaw based on the target's current rotation; pitch starts at 0.
        cameraYaw = car.eulerAngles.y;
        cameraPitch = 0f;
    }

    void Update()
    {
        HandleCameraControl();
    }

    /// <summary>
    /// Handles the camera's rotation and following behavior.
    /// </summary>
    private void HandleCameraControl()
    {
        float deltaTime = Time.deltaTime;
        float inputHorizontal = 0f;
        float inputVertical = 0f;

        // Retrieve input based on the selected input method
        if (!useControllerInput)
        {
            // Mouse input
            inputHorizontal = Input.GetAxis("Mouse X") * cameraRotationSpeed * deltaTime;
            inputVertical = Input.GetAxis("Mouse Y") * cameraRotationSpeed * deltaTime;
        }
        else
        {
            // Controller right stick input
            inputHorizontal = Input.GetAxis("RightStickHorizontal") * cameraRotationSpeed * deltaTime;
            inputVertical = Input.GetAxis("RightStickVertical") * cameraRotationSpeed * deltaTime;
        }

        // Update camera rotation based on input
        if (Mathf.Abs(inputHorizontal) > 0.01f || Mathf.Abs(inputVertical) > 0.01f)
        {
            cameraYaw += inputHorizontal;
            cameraPitch -= inputVertical;
            isRightStickUsed = true;
        }
        else
        {
            // If no input is detected, auto-align the camera with the target's forward direction
            if (!isRightStickUsed)
            {
                float targetYaw = car.eulerAngles.y;
                cameraYaw = Mathf.LerpAngle(cameraYaw, targetYaw, cameraAutoFollowSpeed * deltaTime);
                cameraPitch = Mathf.Lerp(cameraPitch, 0f, cameraAutoFollowSpeed * deltaTime);
            }
            else
            {
                isRightStickUsed = false;
            }
        }

        // Clamp the camera's pitch angle to prevent flipping
        cameraPitch = Mathf.Clamp(cameraPitch, -20f, 45f);

        // Calculate the desired position of the camera relative to the target
        Vector3 cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        Vector3 desiredPosition = car.position + rotation * cameraOffset;

        // Smoothly move the camera towards the desired position
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, desiredPosition, cameraFollowSpeed * deltaTime);

        // Have the camera look at the target (with a slight upward offset)
        cameraTransform.LookAt(car.position + Vector3.up * (cameraHeight * 0.5f));
    }
}
