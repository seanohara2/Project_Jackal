using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class JackalAudioController : MonoBehaviour
{
    public JackalController jackalController; // Reference to the JackalController
    public float normalMaxPitch = 2f; // Max pitch during normal driving
    public float turboMaxPitch = 3f; // Max pitch during turbo
    public float pitchLerpSpeed = 5f; // Speed at which pitch adjusts
    private AudioSource engineAudio; // AudioSource component
    private float targetPitch = 0f; // Desired pitch value

    void Start()
    {
        engineAudio = GetComponent<AudioSource>();

        if (jackalController == null)
        {
            Debug.LogError("JackalController reference not assigned!");
        }
    }

    void Update()
    {
        if (jackalController == null || engineAudio == null)
            return;

        float carSpeed = jackalController.rb.velocity.magnitude;

        // Determine the target pitch based on speed and turbo status
        if (jackalController.isTurbo)
        {
            targetPitch = Mathf.Lerp(2f, turboMaxPitch, carSpeed / (jackalController.moveSpeed * jackalController.turboMultiplier));
        }
        else
        {
            targetPitch = Mathf.Lerp(0f, normalMaxPitch, carSpeed / jackalController.moveSpeed);
        }

        // Smoothly interpolate the pitch
        engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, targetPitch, pitchLerpSpeed * Time.deltaTime);

        // Play the audio if the car is moving and audio is not already playing
        if (carSpeed > 0.1f && !engineAudio.isPlaying)
        {
            engineAudio.Play();
        }
        else if (carSpeed <= 0.1f && engineAudio.isPlaying)
        {
            engineAudio.Stop();
        }
    }
}
