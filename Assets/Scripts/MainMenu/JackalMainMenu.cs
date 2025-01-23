using UnityEngine;

public class JackalMainMenu : MonoBehaviour
{
    public Transform centerPoint;
    public float speed = 5f;
    public float radius = 10f;
    private bool isMovingInCircle = true;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private bool isReturningToCenter = false;
    public float returnSpeed = 3f; // Speed at which the Jackal returns to the center

    void Start()
    {
        // Store the initial position and rotation for later resetting
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (isMovingInCircle && !isReturningToCenter)
        {
            MoveInCircle();
        }
        else if (isReturningToCenter)
        {
            MoveToCenter();
        }
    }

    // Method to move the Jackal in a circular path while looking forward
    void MoveInCircle()
    {
        float angle = Time.time * speed;
        Vector3 offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * radius;
        Vector3 newPosition = centerPoint.position + offset;

        // Calculate the direction to move forward (tangential to the circle)
        Vector3 direction = (newPosition - transform.position).normalized;

        // Update the position
        transform.position = newPosition;

        // Rotate to face the direction of movement (forward)
        transform.forward = direction;
    }

    // Method to stop circular movement and return to center
    void MoveToCenter()
    {
        transform.position = Vector3.MoveTowards(transform.position, centerPoint.position, Time.deltaTime * returnSpeed);
        if (Vector3.Distance(transform.position, centerPoint.position) < 0.1f)
        {
            isReturningToCenter = false;
            // After reaching the center, start the inspection
            //FindObjectOfType<ObjectInspector>().ToggleInspection();
        }
    }

    public void StopAndReturnToCenter()
    {
        isMovingInCircle = false;
        isReturningToCenter = true;
    }

    public void StartMovingInCircle()
    {
        isMovingInCircle = true;
        isReturningToCenter = false;
    }
}


