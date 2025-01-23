using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BoulderCollisionAudio : MonoBehaviour
{
    public AudioClip collisionSound; // Assign the collision sound in the Inspector
    public float collisionVolume = 1f; // Adjust the volume of the sound
    public float minImpactForce = 2f; // Minimum force required to play the sound

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
            // Play the collision sound
            PlayCollisionSound();
        }
    }

    private void PlayCollisionSound()
    {
        if (collisionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collisionSound, collisionVolume);
        }
        else
        {
            Debug.LogWarning("Collision sound or AudioSource is not set.");
        }
    }
}
