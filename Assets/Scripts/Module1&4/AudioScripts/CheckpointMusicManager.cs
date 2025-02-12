using UnityEngine;
using System.Collections;

public class CheckpointMusicManager : MonoBehaviour
{
    [SerializeField] private AudioSource backgroundAudioSource; // The AudioSource for background music
    [SerializeField] private float fadeDuration = 2f; // Duration for fade transitions
    [SerializeField] private AudioClip defaultMusic; // Music to play if no specific checkpoint music is set

    private AudioClip currentClip;

    private void Start()
    {
        // Ensure the initial volume is set to 0.05
        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.volume = 0.1f;
        }
    }

    /// <summary>
    /// Changes the background music to the specified clip, with a fade transition.
    /// </summary>
    /// <param name="newClip">The new audio clip to play.</param>
    public void ChangeMusic(AudioClip newClip)
    {
        // Skip music change if the new clip is null
        if (newClip == null)
        {
            Debug.Log("No music change for this checkpoint.");
            return;
        }

        // Avoid redundant transitions
        if (newClip == currentClip) return;

        currentClip = newClip;
        StartCoroutine(FadeToNewMusic(newClip));
    }

    private IEnumerator FadeToNewMusic(AudioClip newClip)
    {
        // Fade out current music
        if (backgroundAudioSource.isPlaying)
        {
            float elapsedTime = 0f;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                backgroundAudioSource.volume = Mathf.Lerp(0.1f, 0f, elapsedTime / fadeDuration);
                yield return null;
            }
        }

        // Stop current music and play the new clip
        backgroundAudioSource.Stop();
        backgroundAudioSource.clip = newClip ?? defaultMusic; // Fallback to default music
        backgroundAudioSource.Play();

        // Fade in the new music
        float fadeInTime = 0f;
        while (fadeInTime < fadeDuration)
        {
            fadeInTime += Time.deltaTime;
            backgroundAudioSource.volume = Mathf.Lerp(0f, 0.1f, fadeInTime / fadeDuration);
            yield return null;
        }
    }
}
