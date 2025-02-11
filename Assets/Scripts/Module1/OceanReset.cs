using UnityEngine;

public class OceanReset : MonoBehaviour
{
    private CheckpointManager checkpointManager;
    [SerializeField] private ParticleSystem electricHitEffect; // Reference to the electric hit particle effect attached to the player
    [SerializeField] private float effectDuration = 0.5f; // Duration to wait before resetting (adjust as needed)
    private bool isResetting = false;

    private void Start()
    {
        // Find the CheckpointManager in the scene
        checkpointManager = FindObjectOfType<CheckpointManager>();
        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager not found in the scene. Please add it to use the ocean reset functionality.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player collided with the ocean and we're not already in the process of resetting
        if (other.CompareTag("Player") && checkpointManager != null && !isResetting)
        {
            isResetting = true; // Prevent multiple triggers

            // Play the electric hit particle effect if it's assigned and disabled
            if (electricHitEffect != null && !electricHitEffect.isPlaying)
            {
                electricHitEffect.gameObject.SetActive(true); // Enable the effect if it was disabled
                electricHitEffect.Play();

                AudioManager.Instance.PlayOceanResetSound();//play audio
            }

            // Start coroutine to wait briefly, then reset the player
            StartCoroutine(ResetAfterEffect());
        }
    }

    private System.Collections.IEnumerator ResetAfterEffect()
    {
        // Wait for the effect duration before resetting
        yield return new WaitForSeconds(effectDuration);

        // Disable the particle effect
        if (electricHitEffect != null)
        {
            electricHitEffect.Stop();
            electricHitEffect.gameObject.SetActive(false); // Deactivate the effect after it plays
        }

        // Call the ResetToCheckpoint method
        checkpointManager.SendMessage("ResetToCheckpoint");

        // Reset the flag to allow future resets
        isResetting = false;
    }
}
