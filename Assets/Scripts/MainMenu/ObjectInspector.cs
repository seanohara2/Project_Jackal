using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ObjectInspector : MonoBehaviour
{
    [SerializeField] private Transform inspectPosition;  // Target position near the camera for inspection
    [SerializeField] private float moveSpeed = 2f;       // Speed at which the robot moves to the inspection position
    [SerializeField] private float controllerRotationSpeed = 100f; // Rotation speed for the controller
    [SerializeField] private float mouseRotationSpeed = 200f;      // Separate rotation speed for the mouse

    [Header("Robot and Components")]
    [SerializeField] private GameObject robotObject;                // The robot GameObject to inspect
    [SerializeField] private InspectableComponent[] inspectableComponents; // List of components to highlight
    [SerializeField] private GameObject motherBoard;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI descriptionText;       // Text element for component descriptions
    [SerializeField] private TextMeshProUGUI componentTitleText;    // Text element for component titles
    [SerializeField] private Button leftButton;                     // Button to cycle components left
    [SerializeField] private Button rightButton;                    // Button to cycle components right
    [SerializeField] private Button backButton;                     // Back button to return to the selection menu

    private int currentComponentIndex = 0;           // Index of the currently selected component
    private bool isInspecting = false;               // Inspection mode flag
    private Vector3 currentRotation = Vector3.zero;  // Tracks rotation for all axes

    private Vector3 initialPosition;                 // Initial position of the robot
    private Quaternion initialRotation;              // Initial rotation of the robot

    void Start()
    {
        // Cache initial position and rotation of the robot
        if (robotObject != null)
        {
            initialPosition = robotObject.transform.position;
            initialRotation = robotObject.transform.rotation;
        }

        // Bind button events for UI navigation
        leftButton.onClick.AddListener(() => CycleComponent(-1));
        rightButton.onClick.AddListener(() => CycleComponent(1));
        backButton.onClick.AddListener(BackToSelectionMenu);

        // Ensure components are not highlighted initially
        foreach (var component in inspectableComponents)
        {
            EnableHighlight(component, false);
        }

        // Initialize the component view
        UpdateComponentView();
    }

    void Update()
    {
        if (!isInspecting)
            return;

        // Handle controller input for bumpers
        HandleControllerInput();

        // Allow robot rotation via mouse or controller
        RotateRobot();

        // Move the robot towards the inspection position
        MoveRobotToInspectPosition();
    }

    public void StartInspection()
    {
        if (robotObject == null)
            return;

        // Always start from the top of the order of inspectable components
        currentComponentIndex = 0;

        // Make the robot kinematic before positioning and rotating
        Rigidbody rb = robotObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            //rb.velocity = Vector3.zero;
            //rb.angularVelocity = Vector3.zero;
        }

        // Immediately set the robot to the inspection position
        robotObject.transform.position = inspectPosition.position;

        // Get the focus rotation of the first component
        Vector3 focusRotation = inspectableComponents[currentComponentIndex].focusRotation;
        robotObject.transform.rotation = Quaternion.Euler(focusRotation);
        currentRotation = focusRotation;

        // Disable highlight on all components before enabling the first one
        foreach (var component in inspectableComponents)
        {
            EnableHighlight(component, false);
        }

        // Highlight the current (first) component
        EnableHighlight(currentComponentIndex, true);

        // Disable objects specific to this component, if any
        UpdateDisabledObjects();

        // Update component view (description, rotation, etc.)
        UpdateComponentView();

        // Wait one frame before enabling inspection mode to ensure transforms are stable
        StartCoroutine(EnableInspectionAfterFrame());
    }

    private IEnumerator EnableInspectionAfterFrame()
    {
        yield return null; // Wait one frame
        isInspecting = true; // Now user can rotate and interact
    }

    public void StopInspection()
    {
        if (robotObject == null)
            return;

        isInspecting = false;

        // Make robot kinematic before resetting position/rotation
        Rigidbody rb = robotObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            //rb.velocity = Vector3.zero;
            //rb.angularVelocity = Vector3.zero;
        }

        // Reset robot position and rotation
        ResetRobotPositionAndRotation();

        // Disable highlighting on all components
        foreach (var component in inspectableComponents)
        {
            EnableHighlight(component, false);
        }

        // Re-enable all previously disabled objects
        ReenableAllDisabledObjects();
        motherBoard.SetActive(false);

        // Wait a frame to let transforms settle, then re-enable physics
        StartCoroutine(ReenablePhysicsAfterReset());
    }

    private IEnumerator ReenablePhysicsAfterReset()
    {
        yield return null; // Wait one frame so the robot doesn't "drop"

        Rigidbody rb = robotObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false; // Allow physics again now that position is stable
        }
    }

    private void RotateRobot()
    {
        // Check if user is providing rotation input
        bool userIsRotating = Input.GetMouseButton(0) ||
                              Mathf.Abs(Input.GetAxis("RightStickHorizontal")) > 0.1f ||
                              Mathf.Abs(Input.GetAxis("RightStickVertical")) > 0.1f;

        if (!userIsRotating)
        {
            // If no input, don't override the current rotation
            return;
        }

        float rotationX = 0f; // Vertical rotation (X-axis)
        float rotationY = 0f; // Horizontal rotation (Y-axis)

        // Mouse input for rotation
        if (Input.GetMouseButton(0))
        {
            rotationY = -Input.GetAxis("Mouse X") * mouseRotationSpeed * Time.deltaTime; // Horizontal rotation
            rotationX = Input.GetAxis("Mouse Y") * mouseRotationSpeed * Time.deltaTime;  // Vertical rotation
        }

        // Controller input for rotation (Right Stick)
        rotationY += Input.GetAxis("RightStickHorizontal") * controllerRotationSpeed * Time.deltaTime; // Horizontal rotation
        rotationX += -Input.GetAxis("RightStickVertical") * controllerRotationSpeed * Time.deltaTime;  // Vertical rotation

        // Update currentRotation with user input
        currentRotation.x = Mathf.Clamp(currentRotation.x + rotationX, -20f, 20f);
        currentRotation.y += rotationY; // Increment Y based on input (no longer from localEulerAngles)
        currentRotation.z = 0f; // Keep Z fixed at zero

        // Apply the updated rotation
        robotObject.transform.localRotation = Quaternion.Euler(currentRotation);
    }

    private void CycleComponent(int direction)
    {
        // Disable highlight on current component
        EnableHighlight(currentComponentIndex, false);

        // Re-enable objects disabled by the current component
        ReenableDisabledObjects(inspectableComponents[currentComponentIndex]);

        currentComponentIndex = (currentComponentIndex + direction) % inspectableComponents.Length;

        if (currentComponentIndex < 0)
        {
            currentComponentIndex = inspectableComponents.Length - 1;
        }

        // Do NOT reset currentRotation here. Let RotateToFocusOnComponent set it.
        // currentRotation = Vector3.zero; // Remove this line

        // Highlight the new component
        EnableHighlight(currentComponentIndex, true);

        // Disable objects specific to the new component
        UpdateDisabledObjects();

        // Update the component view (applies focusRotation and sets currentRotation)
        UpdateComponentView();
    }


    private void HandleControllerInput()
    {
        if (Input.GetButtonDown("LeftBumper"))
        {
            CycleComponent(-1); // Cycle left
        }
        else if (Input.GetButtonDown("RightBumper"))
        {
            CycleComponent(1);  // Cycle right
        }
    }

    private void UpdateComponentView()
    {
        // Update the component title and description in the UI
        componentTitleText.text = inspectableComponents[currentComponentIndex].componentName;
        descriptionText.text = inspectableComponents[currentComponentIndex].description;

        // Rotate the robot to focus on its predefined rotation, if provided
        RotateToFocusOnComponent(inspectableComponents[currentComponentIndex].focusRotation);
    }

    private void RotateToFocusOnComponent(Vector3 focusRotation)
    {
        // Convert the preset rotation into a global Quaternion
        Quaternion targetRotation = Quaternion.Euler(focusRotation.x, focusRotation.y, focusRotation.z);

        // Apply the rotation directly
        robotObject.transform.rotation = targetRotation;

        // Update currentRotation to match the preset focusRotation
        currentRotation = focusRotation;
    }

    private void MoveRobotToInspectPosition()
    {
        // Lerp towards the inspection position if isInspecting is true
        robotObject.transform.position = Vector3.Lerp(robotObject.transform.position, inspectPosition.position, Time.deltaTime * moveSpeed);
    }

    private void ResetRobotPositionAndRotation()
    {
        robotObject.transform.position = initialPosition;
        robotObject.transform.rotation = initialRotation;
        currentRotation = Vector3.zero;
    }

    private void EnableHighlight(int index, bool enable)
    {
        var component = inspectableComponents[index];
        EnableHighlight(component, enable);
    }

    private void EnableHighlight(InspectableComponent component, bool enable)
    {
        if (component.componentObject == null)
            return;

        var outline = component.componentObject.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = enable;
        }
    }

    private void UpdateDisabledObjects()
    {
        var currentComponent = inspectableComponents[currentComponentIndex];
        foreach (var obj in currentComponent.objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    private void ReenableDisabledObjects(InspectableComponent component)
    {
        foreach (var obj in component.objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    private void ReenableAllDisabledObjects()
    {
        foreach (var component in inspectableComponents)
        {
            ReenableDisabledObjects(component);
        }
    }

    private void BackToSelectionMenu()
    {
        // Stop inspection before returning
        StopInspection();

        // Call the MenuController to switch back
        MenuController menuController = FindObjectOfType<MenuController>();
        if (menuController != null)
        {
            menuController.BackToSelectionMenuFromInspect();
        }
    }
}


/*using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ObjectInspector : MonoBehaviour
{
    [SerializeField] private Transform inspectPosition;  // Target position near the camera for inspection
    [SerializeField] private float moveSpeed = 2f;       // Speed at which the robot moves to the inspection position
    [SerializeField] private float controllerRotationSpeed = 100f; // Rotation speed for the controller
    [SerializeField] private float mouseRotationSpeed = 200f;      // Separate rotation speed for the mouse

    [Header("Robot and Components")]
    [SerializeField] private GameObject robotObject;                // The robot GameObject to inspect
    [SerializeField] private InspectableComponent[] inspectableComponents; // List of components to highlight

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI descriptionText;       // Text element for component descriptions
    [SerializeField] private TextMeshProUGUI componentTitleText;    // Text element for component titles
    [SerializeField] private Button leftButton;                     // Button to cycle components left
    [SerializeField] private Button rightButton;                    // Button to cycle components right
    [SerializeField] private Button backButton;                     // Back button to return to the selection menu

    private int currentComponentIndex = 0;           // Index of the currently selected component
    private bool isInspecting = false;               // Inspection mode flag
    private float currentVerticalRotation = 0f;      // Vertical rotation (X-axis) control

    private Vector3 initialPosition;                 // Initial position of the robot
    private Quaternion initialRotation;              // Initial rotation of the robot

    void Start()
    {
        // Cache initial position and rotation of the robot
        if (robotObject != null)
        {
            initialPosition = robotObject.transform.position;
            initialRotation = robotObject.transform.rotation;
        }

        // Bind button events for UI navigation
        leftButton.onClick.AddListener(() => CycleComponent(-1));
        rightButton.onClick.AddListener(() => CycleComponent(1));
        backButton.onClick.AddListener(BackToSelectionMenu);

        // Ensure components are not highlighted initially
        foreach (var component in inspectableComponents)
        {
            EnableHighlight(component, false);
        }

        // Initialize the component view
        UpdateComponentView();
    }

    void Update()
    {
        if (!isInspecting)
            return;

        // Handle controller input for bumpers
        HandleControllerInput();

        // Allow robot rotation via mouse or controller
        RotateRobot();

        // Move the robot towards the inspection position
        MoveRobotToInspectPosition();
    }

    public void StartInspection()
    {
        if (robotObject == null)
            return;

        isInspecting = true;

        // Start moving robot to inspect position (handled in Update)
        // Highlight the current component
        EnableHighlight(currentComponentIndex, true);

        // Update component view (description, title)
        UpdateComponentView();
    }

    public void StopInspection()
    {
        if (robotObject == null)
            return;

        isInspecting = false;

        // Reset robot position and rotation
        ResetRobotPositionAndRotation();

        // Disable highlighting on all components
        foreach (var component in inspectableComponents)
        {
            EnableHighlight(component, false);
        }
    }

    private void RotateRobot()
    {
        float rotationX = 0f;
        float rotationY = 0f;

        // Mouse input for rotation
        if (Input.GetMouseButton(0))
        {
            rotationX = -Input.GetAxis("Mouse X") * mouseRotationSpeed * Time.deltaTime;
            rotationY = Input.GetAxis("Mouse Y") * mouseRotationSpeed * Time.deltaTime;
        }

        // Controller input for rotation (Right Stick)
        rotationX += Input.GetAxis("RightStickHorizontal") * controllerRotationSpeed * Time.deltaTime;
        rotationY += -Input.GetAxis("RightStickVertical") * controllerRotationSpeed * Time.deltaTime;

        // Apply horizontal rotation
        robotObject.transform.Rotate(Vector3.up, rotationX, Space.World);

        // Clamp and apply vertical rotation between -20 and 20 degrees
        currentVerticalRotation = Mathf.Clamp(currentVerticalRotation + rotationY, -20f, 20f);
        robotObject.transform.localRotation = Quaternion.Euler(currentVerticalRotation, robotObject.transform.localRotation.eulerAngles.y, 0);
    }

    private void CycleComponent(int direction)
    {
        // Disable highlight on current component
        EnableHighlight(currentComponentIndex, false);

        currentComponentIndex = (currentComponentIndex + direction) % inspectableComponents.Length;

        // Ensure proper cycling when reaching the first or last component
        if (currentComponentIndex < 0)
        {
            currentComponentIndex = inspectableComponents.Length - 1;
        }

        // Reset vertical rotation when switching to a new component
        currentVerticalRotation = 0f;

        // Highlight new component
        EnableHighlight(currentComponentIndex, true);

        // Update the component view
        UpdateComponentView();
    }

    private void HandleControllerInput()
    {
        if (Input.GetButtonDown("LeftBumper"))
        {
            CycleComponent(-1); // Cycle left
        }
        else if (Input.GetButtonDown("RightBumper"))
        {
            CycleComponent(1);  // Cycle right
        }
    }

    private void UpdateComponentView()
    {
        // Update the component title and description in the UI
        componentTitleText.text = inspectableComponents[currentComponentIndex].componentName;
        descriptionText.text = inspectableComponents[currentComponentIndex].description;

        // Rotate the robot to focus on its predefined rotation, if provided
        RotateToFocusOnComponent(inspectableComponents[currentComponentIndex].focusRotation);
    }

    private void RotateToFocusOnComponent(Vector3 rotation)
    {
        robotObject.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z);
    }

    private void MoveRobotToInspectPosition()
    {
        robotObject.transform.position = Vector3.Lerp(robotObject.transform.position, inspectPosition.position, Time.deltaTime * moveSpeed);
    }

    private void ResetRobotPositionAndRotation()
    {
        robotObject.transform.position = initialPosition;
        robotObject.transform.rotation = initialRotation;
        currentVerticalRotation = 0f;
    }

    private void EnableHighlight(int index, bool enable)
    {
        var component = inspectableComponents[index];
        EnableHighlight(component, enable);
    }

    private void EnableHighlight(InspectableComponent component, bool enable)
    {
        if (component.componentObject == null)
            return;

        var outline = component.componentObject.GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = enable;
        }
    }

    private void BackToSelectionMenu()
    {
        // Stop inspection before returning
        StopInspection();

        // Call the MenuController to switch back
        MenuController menuController = FindObjectOfType<MenuController>();
        if (menuController != null)
        {
            menuController.BackToSelectionMenuFromInspect();
        }
    }
}

*/