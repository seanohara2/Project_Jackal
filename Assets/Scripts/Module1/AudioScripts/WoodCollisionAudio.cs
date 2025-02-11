using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WoodCollisionAudio : MonoBehaviour
{
    public AudioClip woodCollisionSound; // Assign the wood collision sound in the Inspector
    public float collisionVolume = 1f; // Adjust the volume of the sound
    public float minImpactForce = 1f; // Minimum force required to play the sound

    private AudioSource audioSource;

    void Start()
    {
        // Get the AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource component is missing from this GameObject.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Calculate the impact force from the collision
        float impactForce = collision.relativeVelocity.magnitude;

        // Check if the impact force exceeds the minimum threshold
        if (impactForce >= minImpactForce)
        {
            // Play the wood collision sound
            PlayCollisionSound();
        }
    }

    private void PlayCollisionSound()
    {
        if (woodCollisionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(woodCollisionSound, collisionVolume);
        }
        else
        {
            Debug.LogWarning("Wood collision sound or AudioSource is not set.");
        }
    }
}
