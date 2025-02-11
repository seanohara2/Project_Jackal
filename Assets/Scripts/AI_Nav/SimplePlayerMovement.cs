using UnityEngine;

public class SimplePlayerMovement : MonoBehaviour
{
    public float speed = 5f;  // Movement speed

    void Update()
    {
        // Get input from WASD or arrow keys
        float moveX = Input.GetAxis("Horizontal");  // Left and right (A/D or arrow keys)
        float moveZ = Input.GetAxis("Vertical");    // Forward and backward (W/S or arrow keys)

        // Create a movement vector
        Vector3 movement = new Vector3(moveX, 0, moveZ) * speed * Time.deltaTime;

        // Move the player
        transform.Translate(movement);
    }
}
