using UnityEngine;
using UnityEngine.AI;

public class CheckpointNavigationScript : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [Tooltip("Reference to the CheckpointManager that holds the list of checkpoints.")]
    public CheckpointManager checkpointManager;

    [Header("Navigation Settings")]
    [Tooltip("NavMeshAgent component that will move toward the next checkpoint.")]
    private NavMeshAgent agent;

    void Start()
    {
        // Get the NavMeshAgent component attached to this GameObject
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("CheckpointNavigationScript: No NavMeshAgent component found on this GameObject!");
        }
        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointNavigationScript: No CheckpointManager assigned!");
        }
    }

    void Update()
    {
        if (agent != null && agent.isOnNavMesh)
        {
            Transform nextCheckpoint = GetNextCheckpoint();
            if (nextCheckpoint != null)
            {
                agent.SetDestination(nextCheckpoint.position);
            }
            else
            {
                Debug.LogWarning("No next checkpoint available!");
            }
        }
        else
        {
            Debug.LogWarning("NavMeshAgent is not on a NavMesh!");
        }
    }

    /// <summary>
    /// Returns the transform of the next checkpoint based on the checkpoint order.
    /// Assumes that the next checkpoint is the one immediately after the last reached checkpoint.
    /// </summary>
    /// <returns>The transform of the next checkpoint, or null if none exist.</returns>
    Transform GetNextCheckpoint()
    {
        if (checkpointManager == null)
            return null;

        // Get the next checkpoint index based on the last reached checkpoint.
        int nextIndex = checkpointManager.LastReachedCheckpointIndex + 1;

        // Option 1: Loop back to the start if we reach the end.
        if (nextIndex >= checkpointManager.checkpoints.Count)
        {
            nextIndex = 0;  // or return null if you want the agent to stop at the last checkpoint
        }

        return checkpointManager.checkpoints[nextIndex].transform;
    }
}
