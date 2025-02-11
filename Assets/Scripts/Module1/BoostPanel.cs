using UnityEngine;

public class BoostPanel : MonoBehaviour
{
    [SerializeField] private float boostMultiplier = 2f; // Multiplier for the boost speed
    [SerializeField] private float boostDuration = 3f;   // Duration of the boost in seconds
    [SerializeField] private float cooldownDuration = 5f; // Cooldown period to prevent abuse
    [SerializeField] private ParticleSystem boostEffect; // Optional: Particle effect for the boost panel

    private bool isOnCooldown = false; // Tracks whether the boost pad is on cooldown

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player (robot) triggers the panel and it's not on cooldown
        if (other.CompareTag("Player") && !isOnCooldown)
        {
            JackalController jackalController = other.GetComponent<JackalController>();
            if (jackalController != null)
            {
                // Apply the boost
                StartCoroutine(ApplyBoost(jackalController));
            }

            // Trigger boost effect if available
            if (boostEffect != null)
            {
                boostEffect.Play();
            }

            // Start cooldown
            StartCoroutine(StartCooldown());
        }
    }

    private System.Collections.IEnumerator ApplyBoost(JackalController jackalController)
    {
        // Save the original move speed
        float originalSpeed = jackalController.moveSpeed;

        // Apply the boost multiplier
        jackalController.moveSpeed *= boostMultiplier;

        // Wait for the boost duration
        yield return new WaitForSeconds(boostDuration);

        // Reset the move speed
        jackalController.moveSpeed = originalSpeed;
    }

    private System.Collections.IEnumerator StartCooldown()
    {
        // Set the boost pad to cooldown
        isOnCooldown = true;

        // Wait for the cooldown duration
        yield return new WaitForSeconds(cooldownDuration);

        // Reset the cooldown
        isOnCooldown = false;
    }
}
