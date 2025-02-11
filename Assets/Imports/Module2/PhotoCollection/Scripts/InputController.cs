using UnityEngine;
using System;
namespace DynamicPhotoCamera
{
    // Handles user input processing for mouse and touch controls
    public class InputController : MonoBehaviour
    {
        // Reference to photo controller
        [HideInInspector] public PhotoController photoController;
        // Duration required for long press
        [HideInInspector] public float longClickDuration = 1f;

        // Main camera for raycasting
        public Camera currentCamera;
        // Current device type (Desktop/Handheld)
        private string deviceType;
        // Active click state
        [HideInInspector] public bool IsClicking;
        // Last input position
        [HideInInspector] public Vector3 lastVector;
        // Timer for click duration
        private float clickTimer;
        // Timer for special hold action
        private float specialHeldTimer;

        // Property for camera access
        public Camera CurrentCamera => currentCamera;
        // Property for device type access
        public string DeviceType => deviceType;
        // Property for last vector access
        public Vector3 LastVector => lastVector;

        // Event for input start
        public event Action<Vector3> OnInputDown;
        // Event for continuous input
        public event Action<Vector3> OnInputHeld;
        // Event for input release
        public event Action<Vector3> OnInputUp;

        // Turns off the input if photo was made
        public bool forbiddenInput;

        // Initialize device type and camera
        private void Start()
        {
            InitializeDeviceType();
            SetupCamera();
        }

        // Process input based on device type
        private void Update()
        {
            HandleMouseInput();

            photoController.uiManager.UpdatePhotoSquare(IsClicking);
            HandleMovingCard();
        }

        // Creates ray from camera through screen point
        public Ray GetScreenPointRay(Vector3 screenPoint) => currentCamera.ScreenPointToRay(screenPoint);

        // Processes mouse input for desktop
        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
                StartInput(Input.mousePosition);
            else if (Input.GetMouseButton(0))
                ContinueInput(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0))
            {
                Vector3 touchPos = Input.mousePosition;
                EndInput(touchPos);
            }
            MoveOtherObjects(Input.mousePosition);
        }

        // Handles initial input
        private void StartInput(Vector3 inputPos)
        {
            Debug.Log("start input");
            OnInputDown?.Invoke(inputPos);
            CheckForInteraction(inputPos);
        }

        // Handles continuous input
        private void ContinueInput(Vector3 inputPos)
        {
            if (!forbiddenInput)
            {
                OnInputHeld?.Invoke(inputPos);
                ProcessHeldInput(inputPos);
            }
        }

        // Handles input release
        private void EndInput(Vector3 inputPos)
        {
            Debug.Log("end input");
            OnInputUp?.Invoke(inputPos);
            ResetInputState();
        }

        // Sets device type based on system info
        private void InitializeDeviceType()
        {
            if (SystemInfo.deviceType.ToString() == "Handheld")
            {
                deviceType = "Handheld";
            }
            else
            {
                deviceType = "Desktop";
            }
        }

        // Initializes camera reference
        private void SetupCamera()
        {
            currentCamera = currentCamera ? currentCamera : Camera.main;
        }

        // Processes card movement based on device type
        private void HandleMovingCard()
        {
            if (!photoController.movingCard) return;

            if (deviceType == "Desktop")
            {
                HandleDesktopCardMovement();
            }
        }

        // Handles card movement for desktop
        private void HandleDesktopCardMovement()
        {
            if (Input.GetMouseButton(0))
                photoController.movingCard.MoveIt(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0))
                photoController.movingCard.LetItGo(Input.mousePosition);
        }

        // Checks for interaction with game objects
        private void CheckForInteraction(Vector3 inputPos)
        {
            specialHeldTimer = 0;
            IsClicking = true;
            clickTimer = longClickDuration;
        }

        // Processes held input state
        private void ProcessHeldInput(Vector3 inputPos)
        {
            if (specialHeldTimer > 1 && !IsClicking)
            {
                IsClicking = true;
                clickTimer = longClickDuration;
            }

            if (IsClicking)
            {
                lastVector = inputPos;
                if (clickTimer <= 0)
                {
                    IsClicking = false;
                    photoController.MakeScrenshot(inputPos);
                }
                else
                {
                    clickTimer -= Time.deltaTime;
                }
            }
            else
            {
                specialHeldTimer += Time.deltaTime;
            }
        }

        // Resets input state
        private void ResetInputState()
        {
            photoController.uiManager.ResetPhotos();
            specialHeldTimer = 0;
            forbiddenInput = false;
        }

        // Updates positions of UI elements
        private void MoveOtherObjects(Vector3 inputPos)
        {
            photoController.pointerOnScreen.transform.position = inputPos + photoController.pointerOnScreen.positionOffset;
        }
    }
}