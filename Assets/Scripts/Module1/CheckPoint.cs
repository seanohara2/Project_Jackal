using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private GameObject portalGreenEffect; // Reference to the "Portal Green" effect
    private CheckpointManager checkpointManager; // Reference to the checkpoint manager
    private int checkpointIndex; // Index of this checkpoint in the manager list

    private void Start()
    {
        portalGreenEffect.SetActive(false); // Disable the effect initially for all checkpoints
        checkpointManager = FindObjectOfType<CheckpointManager>(); // Find the CheckpointManager in the scene

        // Find this checkpoint's index in the checkpoint list
        checkpointIndex = checkpointManager.checkpoints.IndexOf(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            checkpointManager.SetCheckpoint(transform, checkpointIndex); // Set this checkpoint as the current checkpoint
            portalGreenEffect.SetActive(false); // Disable the effect for this checkpoint
            checkpointManager.ActivateNextCheckpoint(); // Activate the next checkpoint's effect
        }
    }

    public void ActivatePortalEffect()
    {
        portalGreenEffect.SetActive(true); // Enable the portal effect for this checkpoint
    }

    public void DeactivatePortalEffect()
    {
        portalGreenEffect.SetActive(false); // Disable the portal effect for this checkpoint
    }
}
