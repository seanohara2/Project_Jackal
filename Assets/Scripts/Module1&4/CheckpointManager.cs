using TMPro; // Add this for TextMeshPro support
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CheckpointManager : MonoBehaviour
{
    public List<Checkpoint> checkpoints; // List of all checkpoints in order
    public List<string> checkpointMessages; // List of messages for each checkpoint
    public TextMeshProUGUI checkpointText; // Reference to the TMP text component
    private int lastReachedCheckpointIndex = 0; // Index of the last checkpoint the player collided with
    public int LastReachedCheckpointIndex
    {
        get { return lastReachedCheckpointIndex; }
    }
    [SerializeField] private Transform player; // Reference to the player's transform

    [SerializeField] private List<Transform> objectsToReset; // Objects to reset with their initial states
    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Quaternion> originalRotations = new List<Quaternion>();

    [SerializeField] private GameStatsManager gameStatsManager; // Reference to GameStatsManager
    [SerializeField] private PlayerStatsManager2 playerStatsManager2; // New reference to PlayerStatsManager

    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;

    // Music and audio
    public List<AudioClip> checkpointMusic; // Music for each checkpoint
    public CheckpointMusicManager musicManager; // Reference to the music manager
    private HashSet<int> reachedCheckpoints = new HashSet<int>(); // Track reached checkpoints

    private void Start()
    {
        if (checkpoints.Count > 0)
        {
            checkpoints[lastReachedCheckpointIndex].ActivatePortalEffect(); // Activate the effect on the first checkpoint only
        }

        // Save initial positions and rotations of objects to reset
        foreach (Transform obj in objectsToReset)
        {
            originalPositions.Add(obj.position);
            originalRotations.Add(obj.rotation);
        }

        // Display the first checkpoint's message if available
        UpdateCheckpointMessage(lastReachedCheckpointIndex);
    }

    private void Update()
    {
        // Check if the Y button on the controller is pressed
        if (Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.R))
        {
            ResetToCheckpoint();
        }
    }

    public void SetCheckpoint(Transform checkpoint, int checkpointIndex)
    {
        // Skip sound for the first checkpoint
        if (checkpointIndex != 0 && !reachedCheckpoints.Contains(checkpointIndex))
        {
            AudioManager.Instance.PlayCheckpointSound(); // Play checkpoint sound
        }

        // Mark the checkpoint as reached
        reachedCheckpoints.Add(checkpointIndex);

        // Update the checkpoint position and rotation
        lastCheckpointPosition = checkpoint.position;
        lastCheckpointRotation = checkpoint.rotation;
        lastReachedCheckpointIndex = checkpointIndex; // Update the index of the last checkpoint reached
        Debug.Log($"Checkpoint set to position {lastCheckpointPosition} at index {lastReachedCheckpointIndex}");

        // Update the displayed message for the current checkpoint
        UpdateCheckpointMessage(checkpointIndex);

        // Notify the music manager to play the appropriate music
        if (musicManager != null && checkpointIndex < checkpointMusic.Count)
        {
            musicManager.ChangeMusic(checkpointMusic[checkpointIndex]);
        }
    }

    public void ActivateNextCheckpoint()
    {
        checkpoints[lastReachedCheckpointIndex].DeactivatePortalEffect();

        int nextCheckpointIndex = lastReachedCheckpointIndex + 1;
        if (nextCheckpointIndex < checkpoints.Count)
        {
            checkpoints[nextCheckpointIndex].ActivatePortalEffect();
        }
    }

    private void UpdateCheckpointMessage(int checkpointIndex)
    {
        if (checkpointIndex < checkpointMessages.Count)
        {
            checkpointText.text = checkpointMessages[checkpointIndex];
        }
        else
        {
            Debug.LogWarning($"No message set for checkpoint index {checkpointIndex}");
        }
    }

    public void ResetToCheckpoint()
    {
        if (gameStatsManager != null)
        {
            gameStatsManager.IncrementRestarts();
        }

        if (playerStatsManager2 != null)
        {
            playerStatsManager2.IncrementRestarts();
        }

        if (lastCheckpointPosition != Vector3.zero) // Ensure a checkpoint was set
        {
            Debug.Log($"Resetting player to checkpoint at position {lastCheckpointPosition}, rotation {lastCheckpointRotation}");

            // Stop any ongoing player motion
            StartCoroutine(ResetPositionAfterFixedUpdate());

            // Reset the flag or other objects as needed
            ResetAdditionalObjects();
        }
        else
        {
            Debug.LogWarning("No checkpoint set!");
        }
    }

    private IEnumerator ResetPositionAfterFixedUpdate()
    {
        yield return new WaitForFixedUpdate(); // Wait for the next physics frame

        // Move player to last checkpoint position and rotation
        player.position = lastCheckpointPosition;
        player.rotation = lastCheckpointRotation;

        Debug.Log("Player moved to checkpoint position with forced update.");

        // Reset velocity if the player has a Rigidbody
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log("Player Rigidbody velocity reset.");
        }
    }

    // Method to reset additional objects to their original positions and rotations
    private void ResetAdditionalObjects()
    {
        for (int i = 0; i < objectsToReset.Count; i++)
        {
            objectsToReset[i].position = originalPositions[i];
            objectsToReset[i].rotation = originalRotations[i];

            // Reset velocity if objects have Rigidbodies
            Rigidbody objRb = objectsToReset[i].GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.velocity = Vector3.zero;
                objRb.angularVelocity = Vector3.zero;
            }
        }
    }
}



//main
/*using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CheckpointManager : MonoBehaviour
{
    public List<Checkpoint> checkpoints; // List of all checkpoints in order
    private int lastReachedCheckpointIndex = 0; // Index of the last checkpoint the player collided with
    [SerializeField] private Transform player; // Reference to the player's transform

    // Objects to reset with their initial states
    [SerializeField] private List<Transform> objectsToReset;
    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Quaternion> originalRotations = new List<Quaternion>();

    private Vector3 lastCheckpointPosition;
    private Quaternion lastCheckpointRotation;

    private void Start()
    {
        if (checkpoints.Count > 0)
        {
            checkpoints[lastReachedCheckpointIndex].ActivatePortalEffect(); // Activate the effect on the first checkpoint only
        }

        // Save initial positions and rotations of objects to reset
        foreach (Transform obj in objectsToReset)
        {
            originalPositions.Add(obj.position);
            originalRotations.Add(obj.rotation);
        }
    }

    private void Update()
    {
        // Check if the Y button on the controller is pressed
        if (Input.GetKeyDown(KeyCode.JoystickButton3))
        {
            ResetToCheckpoint();
        }
    }

    public void SetCheckpoint(Transform checkpoint, int checkpointIndex)
    {
        lastCheckpointPosition = checkpoint.position;
        lastCheckpointRotation = checkpoint.rotation;
        lastReachedCheckpointIndex = checkpointIndex; // Update the index of the last checkpoint reached
        Debug.Log($"Checkpoint set to position {lastCheckpointPosition} at index {lastReachedCheckpointIndex}");
    }

    public void ActivateNextCheckpoint()
    {
        checkpoints[lastReachedCheckpointIndex].DeactivatePortalEffect();

        int nextCheckpointIndex = lastReachedCheckpointIndex + 1;
        if (nextCheckpointIndex < checkpoints.Count)
        {
            checkpoints[nextCheckpointIndex].ActivatePortalEffect();
        }
    }

    public void ResetToCheckpoint()
    {
        if (lastCheckpointPosition != Vector3.zero) // Ensure a checkpoint was set
        {
            Debug.Log($"Resetting player to checkpoint at position {lastCheckpointPosition}, rotation {lastCheckpointRotation}");

            // Stop any ongoing player motion
            StartCoroutine(ResetPositionAfterFixedUpdate());

            // Reset the flag or other objects as needed
            ResetAdditionalObjects();
        }
        else
        {
            Debug.LogWarning("No checkpoint set!");
        }
    }

    private IEnumerator ResetPositionAfterFixedUpdate()
    {
        yield return new WaitForFixedUpdate(); // Wait for the next physics frame

        // Move player to last checkpoint position and rotation
        player.position = lastCheckpointPosition;
        player.rotation = lastCheckpointRotation;

        Debug.Log("Player moved to checkpoint position with forced update.");

        // Reset velocity if the player has a Rigidbody
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            Debug.Log("Player Rigidbody velocity reset.");
        }
    }

    // Method to reset additional objects to their original positions and rotations
    private void ResetAdditionalObjects()
    {
        for (int i = 0; i < objectsToReset.Count; i++)
        {
            objectsToReset[i].position = originalPositions[i];
            objectsToReset[i].rotation = originalRotations[i];

            // Reset velocity if objects have Rigidbodies
            Rigidbody objRb = objectsToReset[i].GetComponent<Rigidbody>();
            if (objRb != null)
            {
                objRb.velocity = Vector3.zero;
                objRb.angularVelocity = Vector3.zero;
            }
        }
    }
}*/