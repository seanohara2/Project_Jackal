using System;
using UnityEngine;

public class JackalControllerNoCamera : MonoBehaviour
{
    public Transform car;
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
    }

    void FixedUpdate()
    {
        CheckGroundStatus();
        HandleCarMovementAndRotation();
        UpdateWheelAnimations();
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

        if (groundedWheels > 0)
        {
            isGrounded = true;
            lastGroundedTime = Time.time;
            rb.drag = groundDrag;
        }
        else
        {
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

    void HandleCarMovementAndRotation()
    {
        verticalInput = Input.GetAxis("LeftStickVertical");
        horizontalInput = Input.GetAxis("LeftStickHorizontal");

        if (Input.GetButton("RightBumper") && verticalInput < 0)
        {
            isTurbo = true;
        }
        else
        {
            isTurbo = false;
        }

        float currentSpeed = isTurbo ? moveSpeed * turboMultiplier : moveSpeed;

        if (isGrounded && Input.GetButton("LeftBumper"))
        {
            Vector3 fixedVelocity = transform.forward * (-verticalInput) * currentSpeed;
            rb.velocity = new Vector3(fixedVelocity.x, rb.velocity.y, fixedVelocity.z);

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
        else if (!isGrounded)
        {
            rb.angularVelocity = Vector3.zero;
        }
    }

    void UpdateWheelAnimations()
    {
        float carSpeed = rb.velocity.magnitude + Mathf.Abs(horizontalInput * turnSpeed * Mathf.Deg2Rad);
        float animationSpeed = Mathf.Lerp(minAnimationSpeed, maxAnimationSpeed, carSpeed / (moveSpeed * turboMultiplier));

        if (carSpeed < 0.1f)
        {
            SetWheelAnimationSpeed(0);
            return;
        }

        if (!Input.GetButton("LeftBumper"))
        {
            SetWheelAnimationSpeed(0);
            return;
        }

        bool isReversing = Vector3.Dot(transform.forward, rb.velocity) < 0;

        if (horizontalInput > 0)
        {
            SetWheelAnimationSpeed(animationSpeed, !isReversing, isReversing);
        }
        else if (horizontalInput < 0)
        {
            SetWheelAnimationSpeed(animationSpeed, isReversing, !isReversing);
        }
        else
        {
            SetWheelAnimationSpeed(animationSpeed, isReversing);
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
