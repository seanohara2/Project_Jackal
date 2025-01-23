//main with keyboar support
using System;
using UnityEngine;

public class JackalController : MonoBehaviour
{
    // Existing variables
    public Transform car;
    public Transform cameraTransform;
    public float cameraDistance = 8f;
    public float cameraHeight = 3f;
    public float cameraRotationSpeed = 100f;
    public float cameraFollowSpeed = 5f;
    public float cameraAutoFollowSpeed = 2f;
    public float moveSpeed = 10f;
    public float turboMultiplier = 2f;
    public float turnSpeed = 50f;

    public CapsuleCollider[] wheelColliders;
    public float groundCheckDistance = 1.0f;
    public float groundedBufferTime = 0.2f;
    public float groundDrag = 1.0f;
    public float airDrag = 0.5f;

    private bool isGrounded;
    public bool isTurbo = false;
    private float horizontalInput;
    private float verticalInput;
    private float cameraYaw;
    private float cameraPitch;
    private bool isRightStickUsed = false;
    public Rigidbody rb;
    private float lastGroundedTime;

    // Wheel animation variables
    public Animator[] leftWheels;  // Assign left wheels Animator components in Inspector
    public Animator[] rightWheels; // Assign right wheels Animator components in Inspector
    public float minAnimationSpeed = 0f;
    public float maxAnimationSpeed = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraYaw = car.eulerAngles.y;
        cameraPitch = 0f;
    }

    void FixedUpdate()
    {
        CheckGroundStatus();
        HandleCarMovementAndRotation();
        UpdateWheelAnimations();
    }

    void Update()
    {
        HandleCameraControl();
    }

    private void CheckGroundStatus()
    {
        int groundedWheels = 0;

        foreach (var wheel in wheelColliders)
        {
            Vector3 wheelCenter = wheel.bounds.center;
            if (Physics.Raycast(wheelCenter, -wheel.transform.up, out RaycastHit hit, groundCheckDistance))
            {
                groundedWheels++;
            }
            Debug.DrawRay(wheelCenter, -wheel.transform.up * groundCheckDistance, Color.red);
        }

        // Car is considered grounded if at least one wheel is in contact
        if (groundedWheels > 0)
        {
            isGrounded = true;
            lastGroundedTime = Time.time;
            rb.drag = groundDrag;
        }
        else
        {
            // Use buffer time to prevent immediate loss of ground status on bumps
            if (Time.time - lastGroundedTime < groundedBufferTime)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
                rb.drag = airDrag;
            }
        }
    }

    void HandleCameraControl()
    {
        // Determine input method
        bool usingKeyboard = (InputUIManager.currentInputMethod == InputUIManager.InputMethod.KeyboardMouse);

        float inputHorizontal = 0f;
        float inputVertical = 0f;

        if (usingKeyboard)
        {
            // Mouse movement for camera
            float mouseX = Input.GetAxis("Mouse X") * cameraRotationSpeed * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * cameraRotationSpeed * Time.deltaTime;

            inputHorizontal = mouseX;
            inputVertical = mouseY;
        }
        else
        {
            // Controller right stick for camera
            float rightStickHorizontal = Input.GetAxis("RightStickHorizontal") * cameraRotationSpeed * Time.deltaTime;
            float rightStickVertical = Input.GetAxis("RightStickVertical") * cameraRotationSpeed * Time.deltaTime;
            inputHorizontal = rightStickHorizontal;
            inputVertical = rightStickVertical;
        }

        if (Mathf.Abs(inputHorizontal) > 0.01f || Mathf.Abs(inputVertical) > 0.01f)
        {
            cameraYaw += inputHorizontal;
            cameraPitch -= inputVertical;
            isRightStickUsed = true;
        }
        else
        {
            if (!isRightStickUsed)
            {
                float targetYaw = car.eulerAngles.y;
                cameraYaw = Mathf.LerpAngle(cameraYaw, targetYaw, cameraAutoFollowSpeed * Time.deltaTime);
                cameraPitch = Mathf.Lerp(cameraPitch, 0f, cameraAutoFollowSpeed * Time.deltaTime);
            }
            else
            {
                isRightStickUsed = false;
            }
        }

        cameraPitch = Mathf.Clamp(cameraPitch, -20f, 45f);

        Vector3 cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        Vector3 targetPosition = car.position + rotation * cameraOffset;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);
        cameraTransform.LookAt(car.position + Vector3.up * cameraHeight * 0.5f);
    }

    void HandleCarMovementAndRotation()
    {
        // Determine input method
        bool usingKeyboard = (InputUIManager.currentInputMethod == InputUIManager.InputMethod.KeyboardMouse);

        float inputForward = 0f;
        float inputTurn = 0f;
        bool enableDrive = false;
        bool boost = false;

        if (usingKeyboard)
        {
            // Keyboard movement:
            // W = forward (+1), S = backward (-1)
            if (Input.GetKey(KeyCode.W)) inputForward += 1f;
            if (Input.GetKey(KeyCode.S)) inputForward -= 1f;

            // A = turn left (-1), D = turn right (+1)
            if (Input.GetKey(KeyCode.A)) inputTurn -= 1f;
            if (Input.GetKey(KeyCode.D)) inputTurn += 1f;

            // Enable drive = Space, Boost = LeftShift
            enableDrive = Input.GetKey(KeyCode.Space);
            boost = Input.GetKey(KeyCode.LeftShift) && inputForward > 0;
        }
        else
        {
            // Controller movement
            verticalInput = Input.GetAxis("LeftStickVertical");
            horizontalInput = Input.GetAxis("LeftStickHorizontal");
            enableDrive = Input.GetButton("LeftBumper");
            boost = Input.GetButton("RightBumper") && verticalInput < 0;

            // Translate these to our internal variables for consistency
            inputForward = -verticalInput; // Note: In original code negative vertical was forward
            inputTurn = horizontalInput;
        }

        // Determine if turbo applies
        isTurbo = boost;

        // Apply current speed based on turbo status
        float currentSpeed = isTurbo ? moveSpeed * turboMultiplier : moveSpeed;

        if (isGrounded && enableDrive)
        {
            Vector3 fixedVelocity = transform.forward * (inputForward) * currentSpeed;
            rb.velocity = new Vector3(fixedVelocity.x, rb.velocity.y, fixedVelocity.z);

            if (inputTurn != 0)
            {
                float turnRate = inputTurn * turnSpeed * Mathf.Deg2Rad;
                rb.angularVelocity = new Vector3(0, turnRate, 0);
            }
            else
            {
                rb.angularVelocity = Vector3.zero;
            }
        }
        else if (!isGrounded) // In air, retain momentum
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    void UpdateWheelAnimations()
    {
        // Calculate the car's speed including forward speed and turn speed
        float carSpeed = rb.velocity.magnitude + Mathf.Abs(horizontalInput * turnSpeed * Mathf.Deg2Rad);
        float animationSpeed = Mathf.Lerp(minAnimationSpeed, maxAnimationSpeed, carSpeed / (moveSpeed * turboMultiplier));

        // Stop animations if the car is idle
        if (carSpeed < 0.1f)
        {
            SetWheelAnimationSpeed(0); // Stop all wheels when idle
            return;
        }

        // Determine input method for turning direction (only matters if we want to differentiate)
        // Here horizontalInput was set in controller mode.
        // In keyboard mode, we didn't store horizontalInput globally.
        // Let's just rely on current inputMethod. If using keyboard, read again:
        bool usingKeyboard = (InputUIManager.currentInputMethod == InputUIManager.InputMethod.KeyboardMouse);
        float currentTurn = usingKeyboard ? (Input.GetKey(KeyCode.A) ? -1 : (Input.GetKey(KeyCode.D) ? 1 : 0)) : horizontalInput;

        bool enableDrive = usingKeyboard ? Input.GetKey(KeyCode.Space) : Input.GetButton("LeftBumper");

        if (!enableDrive)
        {
            SetWheelAnimationSpeed(0); // Stop animations when not driving
            return;
        }

        // Determine direction based on car movement
        bool isReversing = Vector3.Dot(transform.forward, rb.velocity) < 0;

        if (currentTurn > 0) // Right turn
        {
            SetWheelAnimationSpeed(animationSpeed, !isReversing, isReversing); // Left wheels reverse, right wheels forward
        }
        else if (currentTurn < 0) // Left turn
        {
            SetWheelAnimationSpeed(animationSpeed, isReversing, !isReversing); // Left wheels forward, right wheels reverse
        }
        else // Forward or backward movement
        {
            SetWheelAnimationSpeed(animationSpeed, isReversing); // All wheels forward or reverse
        }
    }

    private void SetWheelAnimationSpeed(float speed, bool reverseAll = false, bool reverseLeft = false, bool reverseRight = false)
    {
        foreach (var leftWheel in leftWheels)
        {
            if (leftWheel != null)
            {
                leftWheel.speed = speed;
                leftWheel.SetBool("IsReversing", reverseAll || reverseLeft);
            }
        }
        foreach (var rightWheel in rightWheels)
        {
            if (rightWheel != null)
            {
                rightWheel.speed = speed;
                rightWheel.SetBool("IsReversing", reverseAll || reverseRight);
            }
        }
    }
}




//main (fixed issue)
/*
//main with animations
using System;
using UnityEngine;

public class JackalController : MonoBehaviour
{
    // Existing variables
    public Transform car;
    public Transform cameraTransform;
    public float cameraDistance = 8f;
    public float cameraHeight = 3f;
    public float cameraRotationSpeed = 100f;
    public float cameraFollowSpeed = 5f;
    public float cameraAutoFollowSpeed = 2f;
    public float moveSpeed = 10f;
    public float turboMultiplier = 2f;
    public float turnSpeed = 50f;

    public CapsuleCollider[] wheelColliders;
    public float groundCheckDistance = 1.0f;
    public float groundedBufferTime = 0.2f;
    public float groundDrag = 1.0f;
    public float airDrag = 0.5f;

    private bool isGrounded;
    public bool isTurbo = false;
    private float horizontalInput;
    private float verticalInput;
    private float cameraYaw;
    private float cameraPitch;
    private bool isRightStickUsed = false;
    public Rigidbody rb;
    private float lastGroundedTime;

    // Wheel animation variables
    public Animator[] leftWheels;  // Assign left wheels Animator components in Inspector
    public Animator[] rightWheels; // Assign right wheels Animator components in Inspector
    public float minAnimationSpeed = 0f;
    public float maxAnimationSpeed = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraYaw = car.eulerAngles.y;
        cameraPitch = 0f;
    }

    void FixedUpdate()
    {
        CheckGroundStatus();
        HandleCarMovementAndRotation();
        UpdateWheelAnimations();
    }

    void Update()
    {
        HandleCameraControl();
    }

    private void CheckGroundStatus()
    {
        int groundedWheels = 0;

        foreach (var wheel in wheelColliders)
        {
            Vector3 wheelCenter = wheel.bounds.center;
            if (Physics.Raycast(wheelCenter, -wheel.transform.up, out RaycastHit hit, groundCheckDistance))
            {
                groundedWheels++;
            }
            Debug.DrawRay(wheelCenter, -wheel.transform.up * groundCheckDistance, Color.red);
        }

        // Car is considered grounded if at least one wheel is in contact
        if (groundedWheels > 0)
        {
            isGrounded = true;
            lastGroundedTime = Time.time;
            rb.drag = groundDrag;
        }
        else
        {
            // Use buffer time to prevent immediate loss of ground status on bumps
            if (Time.time - lastGroundedTime < groundedBufferTime)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
                rb.drag = airDrag;
            }
        }
    }

    void HandleCameraControl()
    {
        float rightStickHorizontal = Input.GetAxis("RightStickHorizontal") * cameraRotationSpeed * Time.deltaTime;
        float rightStickVertical = Input.GetAxis("RightStickVertical") * cameraRotationSpeed * Time.deltaTime;

        if (rightStickHorizontal != 0 || rightStickVertical != 0)
        {
            cameraYaw += rightStickHorizontal;
            cameraPitch -= rightStickVertical;
            isRightStickUsed = true;
        }
        else
        {
            if (!isRightStickUsed)
            {
                float targetYaw = car.eulerAngles.y;
                cameraYaw = Mathf.LerpAngle(cameraYaw, targetYaw, cameraAutoFollowSpeed * Time.deltaTime);
                cameraPitch = Mathf.Lerp(cameraPitch, 0f, cameraAutoFollowSpeed * Time.deltaTime);
            }
            else
            {
                isRightStickUsed = false;
            }
        }

        cameraPitch = Mathf.Clamp(cameraPitch, -20f, 45f);

        Vector3 cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        Vector3 targetPosition = car.position + rotation * cameraOffset;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);
        cameraTransform.LookAt(car.position + Vector3.up * cameraHeight * 0.5f);
    }

    void HandleCarMovementAndRotation()
    {
        verticalInput = Input.GetAxis("LeftStickVertical");
        horizontalInput = Input.GetAxis("LeftStickHorizontal");

        // Turbo functionality based on Right Bumper
        if (Input.GetButton("RightBumper") && verticalInput < 0)
        {
            isTurbo = true;
        }
        else
        {
            isTurbo = false;
        }

        // Apply current speed based on turbo status
        float currentSpeed = isTurbo ? moveSpeed * turboMultiplier : moveSpeed;

        if (isGrounded && Input.GetButton("LeftBumper"))
        {
            // Set velocity directly for consistent movement speed
            Vector3 fixedVelocity = transform.forward * (-verticalInput) * currentSpeed;
            rb.velocity = new Vector3(fixedVelocity.x, rb.velocity.y, fixedVelocity.z);

            // Apply smooth rotation using angular velocity
            if (horizontalInput != 0)
            {
                float turnRate = horizontalInput * turnSpeed * Mathf.Deg2Rad;
                rb.angularVelocity = new Vector3(0, turnRate, 0);
            }
            else
            {
                rb.angularVelocity = Vector3.zero;
            }
        }
        else if (!isGrounded) // In air, retain momentum
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    void UpdateWheelAnimations()
    {
        // Calculate the car's speed including forward speed and turn speed
        float carSpeed = rb.velocity.magnitude + Mathf.Abs(horizontalInput * turnSpeed * Mathf.Deg2Rad);
        float animationSpeed = Mathf.Lerp(minAnimationSpeed, maxAnimationSpeed, carSpeed / (moveSpeed * turboMultiplier));

        // Stop animations if the car is idle
        if (carSpeed < 0.1f)
        {
            SetWheelAnimationSpeed(0); // Stop all wheels when idle
            return;
        }

        if (!Input.GetButton("LeftBumper"))
        {
            SetWheelAnimationSpeed(0); // Stop animations when the left bumper is not held
            return;
        }

        // Determine animation direction based on car movement
        bool isReversing = Vector3.Dot(transform.forward, rb.velocity) < 0;

        if (horizontalInput > 0) // Right turn
        {
            SetWheelAnimationSpeed(animationSpeed, !isReversing, isReversing); // Left wheels reverse, right wheels forward
        }
        else if (horizontalInput < 0) // Left turn
        {
            SetWheelAnimationSpeed(animationSpeed, isReversing, !isReversing); // Left wheels forward, right wheels reverse
        }
        else // Forward or backward movement
        {
            SetWheelAnimationSpeed(animationSpeed, isReversing); // All wheels forward or reverse
        }
    }

    // Helper function to set animation speed and reversing status
    private void SetWheelAnimationSpeed(float speed, bool reverseAll = false, bool reverseLeft = false, bool reverseRight = false)
    {
        foreach (var leftWheel in leftWheels)
        {
            if (leftWheel != null)
            {
                leftWheel.speed = speed;
                leftWheel.SetBool("IsReversing", reverseAll || reverseLeft);
            }
        }
        foreach (var rightWheel in rightWheels)
        {
            if (rightWheel != null)
            {
                rightWheel.speed = speed;
                rightWheel.SetBool("IsReversing", reverseAll || reverseRight);
            }
        }
    }
}
}*/




//main (With Ground & Air Momentum) has update window size issue.
/*using UnityEngine;

public class JackalController : MonoBehaviour
{
    public Transform car;
    public Transform cameraTransform;
    public float cameraDistance = 8f;
    public float cameraHeight = 3f;
    public float cameraRotationSpeed = 100f;
    public float cameraFollowSpeed = 5f;
    public float cameraAutoFollowSpeed = 2f;
    public float moveSpeed = 10f;
    public float turboMultiplier = 2f;
    public float turnSpeed = 50f;

    public CapsuleCollider[] wheelColliders; // Assign capsule colliders for each wheel
    public float groundCheckDistance = 1.0f; // Forgiving distance for ground check
    public float groundedBufferTime = 0.2f; // Buffer time to maintain grounded state briefly after losing ground contact
    public float groundDrag = 1.0f; // Normal drag when grounded
    public float airDrag = 0.5f; // Reduced drag for smooth air movement

    private bool isGrounded;
    private bool isTurbo = false;
    private float horizontalInput;
    private float verticalInput;
    private float cameraYaw;
    private float cameraPitch;
    private bool isRightStickUsed = false;
    private Rigidbody rb;
    private float lastGroundedTime; // Tracks the last time the car was grounded

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cameraYaw = car.eulerAngles.y;
        cameraPitch = 0f;
    }

    void Update()
    {
        CheckGroundStatus();
        HandleCarControl();
        HandleCameraControl();
    }

    private void CheckGroundStatus()
    {
        int groundedWheels = 0;

        // Check each wheel's collider with a forgiving ground check distance
        foreach (var wheel in wheelColliders)
        {
            Vector3 wheelCenter = wheel.bounds.center; // Start raycast from the center of the wheel collider
            if (Physics.Raycast(wheelCenter, -wheel.transform.up, out RaycastHit hit, groundCheckDistance))
            {
                groundedWheels++;
            }
            Debug.DrawRay(wheelCenter, -wheel.transform.up * groundCheckDistance, Color.red); // Visualize raycast
        }

        // Car is considered grounded if at least one wheel is in contact
        if (groundedWheels > 0)
        {
            isGrounded = true;
            lastGroundedTime = Time.time; // Update the last grounded time
            rb.drag = groundDrag; // Apply normal ground drag
        }
        else
        {
            // Use grounded buffer to prevent immediate loss of control
            if (Time.time - lastGroundedTime < groundedBufferTime)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
                rb.drag = airDrag; // Reduced drag for smoother air movement
            }
        }
    }


    void HandleCameraControl()
    {
        float rightStickHorizontal = Input.GetAxis("RightStickHorizontal") * cameraRotationSpeed * Time.deltaTime;
        float rightStickVertical = Input.GetAxis("RightStickVertical") * cameraRotationSpeed * Time.deltaTime;

        if (rightStickHorizontal != 0 || rightStickVertical != 0)
        {
            cameraYaw += rightStickHorizontal;
            cameraPitch -= rightStickVertical;
            isRightStickUsed = true;
        }
        else
        {
            if (!isRightStickUsed)
            {
                float targetYaw = car.eulerAngles.y;
                cameraYaw = Mathf.LerpAngle(cameraYaw, targetYaw, cameraAutoFollowSpeed * Time.deltaTime);
                cameraPitch = Mathf.Lerp(cameraPitch, 0f, cameraAutoFollowSpeed * Time.deltaTime);
            }
            else
            {
                isRightStickUsed = false;
            }
        }

        cameraPitch = Mathf.Clamp(cameraPitch, -20f, 45f);

        Vector3 cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);
        Vector3 targetPosition = car.position + rotation * cameraOffset;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);
        cameraTransform.LookAt(car.position + Vector3.up * cameraHeight * 0.5f);
    }

    void HandleCarControl()
    {
        if (isGrounded && Input.GetButton("LeftBumper"))
        {
            horizontalInput = Input.GetAxis("LeftStickHorizontal");
            verticalInput = Input.GetAxis("LeftStickVertical");
            float currentSpeed = moveSpeed;

            // Turbo boost logic
            if (Input.GetButton("RightBumper") && verticalInput < 0)
            {
                isTurbo = true;
                currentSpeed *= turboMultiplier;
            }
            else
            {
                isTurbo = false;
            }

            // Apply forward/backward movement using AddForce to carry momentum naturally
            if (verticalInput != 0)
            {
                Vector3 movementForce = transform.forward * (-verticalInput) * currentSpeed;
                rb.AddForce(movementForce, ForceMode.Acceleration); // Use Acceleration for smoother control
            }

            // Apply rotation (left/right) with Rigidbody physics
            if (horizontalInput != 0)
            {
                float rotation = horizontalInput * turnSpeed * Time.deltaTime;
                Quaternion turnOffset = Quaternion.Euler(0, rotation, 0);
                rb.MoveRotation(rb.rotation * turnOffset);
            }
        }
        else if (!isGrounded) // In air, retain existing momentum without adding new force
        {
            rb.angularVelocity = Vector3.zero; // Stabilize rotation in the air
        }
    }
}

*/



//main (Without Ground & No Air Momentum)
/*using UnityEngine;

public class JackalController : MonoBehaviour
{
    public Transform car; // Assign the car transform in the Inspector
    public Transform cameraTransform; // Assign the camera transform
    public float cameraDistance = 8f; // Distance from the car
    public float cameraHeight = 3f; // Height of the camera
    public float cameraRotationSpeed = 100f; // Speed of manual camera rotation
    public float cameraFollowSpeed = 5f; // Speed at which the camera follows the car's movement
    public float cameraAutoFollowSpeed = 2f; // Speed of camera auto-follow when the car turns
    public float moveSpeed = 10f; // Normal movement speed
    public float turboMultiplier = 2f; // Turbo boost multiplier
    public float turnSpeed = 50f; // Speed of turning the car

    private bool isTurbo = false; // Whether turbo is active or not
    private float horizontalInput;
    private float verticalInput;
    private float cameraYaw; // Horizontal rotation of the camera
    private float cameraPitch; // Vertical rotation of the camera
    private bool isRightStickUsed = false; // Flag to check if the right stick is in use

    void Start()
    {
        // Set initial camera angles
        cameraYaw = car.eulerAngles.y;
        cameraPitch = 0f; // Initialize camera pitch to default
    }

    void Update()
    {
        HandleCarControl();
        HandleCameraControl();
    }

    void HandleCameraControl()
    {
        // Get camera rotation input from the right stick
        float rightStickHorizontal = Input.GetAxis("RightStickHorizontal") * cameraRotationSpeed * Time.deltaTime; // Right stick horizontal axis
        float rightStickVertical = Input.GetAxis("RightStickVertical") * cameraRotationSpeed * Time.deltaTime; // Right stick vertical axis

        // If right stick is being used, manually rotate the camera around the car
        if (rightStickHorizontal != 0 || rightStickVertical != 0)
        {
            cameraYaw += rightStickHorizontal;  // Control horizontal (yaw) rotation with the right stick
            cameraPitch -= rightStickVertical;  // Control vertical (pitch) rotation with the right stick
            isRightStickUsed = true;
        }
        else
        {
            // Smoothly follow the car's rotation when not using the right stick
            if (!isRightStickUsed)
            {
                float targetYaw = car.eulerAngles.y;
                cameraYaw = Mathf.LerpAngle(cameraYaw, targetYaw, cameraAutoFollowSpeed * Time.deltaTime);
                // Keep the camera pitch fixed or return it to a default value (if needed)
                cameraPitch = Mathf.Lerp(cameraPitch, 0f, cameraAutoFollowSpeed * Time.deltaTime);
            }
            else
            {
                // Reset the override flag after some time
                isRightStickUsed = false;
            }
        }

        // Clamp the vertical angle to prevent the camera from flipping too far up/down
        cameraPitch = Mathf.Clamp(cameraPitch, -20f, 45f);

        // Calculate the new position of the camera relative to the car
        Vector3 cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
        Quaternion rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0);

        // Update the camera's position
        Vector3 targetPosition = car.position + rotation * cameraOffset;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);

        // Always look at the car, overriding any other behaviors when the right stick is used
        cameraTransform.LookAt(car.position + Vector3.up * cameraHeight * 0.5f);  // Slightly above the car for better focus
    }

    void HandleCarControl()
    {
        // Check if the left bumper (deadman switch) is held down
        if (Input.GetButton("LeftBumper"))
        {
            // Get movement input from the left stick
            horizontalInput = Input.GetAxis("LeftStickHorizontal"); // Left stick horizontal axis
            verticalInput = Input.GetAxis("LeftStickVertical"); // Left stick vertical axis

            // Apply movement (forward or reverse)
            float currentSpeed = moveSpeed;

            // Turbo boost check (right bumper for turbo while moving forward)
            if (Input.GetButton("RightBumper") && verticalInput < 0)
            {
                isTurbo = true;
                currentSpeed *= turboMultiplier;
            }
            else
            {
                isTurbo = false;
            }

            // Move the car based on left stick input
            if (verticalInput != 0)
            {
                Vector3 movement = transform.forward * (-verticalInput) * currentSpeed * Time.deltaTime; // Inverted verticalInput for correct direction
                transform.Translate(movement, Space.World);
            }

            // Apply rotation (left/right)
            if (horizontalInput != 0)
            {
                float rotation = horizontalInput * turnSpeed * Time.deltaTime;
                transform.Rotate(0, rotation, 0);
            }
        }
    }
}
*/


