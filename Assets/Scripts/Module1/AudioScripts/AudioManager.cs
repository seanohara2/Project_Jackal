using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public GameObject OceanResetSource; // Audio source for ocean reset sound
    public GameObject BoulderCollisionSource; // Audio source for boulder collision sound
    public GameObject checkpointSource;
    public GameObject winSource;

    private void Awake()
    {
        // Ensure there is only one instance of AudioManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayOceanResetSound()
    {
        PlaySound(OceanResetSource);
    }

    public void PlayBoulderCollisionSound()
    {
        PlaySound(BoulderCollisionSource);
    }

    public void PlayCheckpointSound()
    {
        PlaySound(checkpointSource);
    }

    public void PlayWinSound()
    {
        PlaySound(winSource);
    }



    private void PlaySound(GameObject source)
    {
        if (source != null)
        {
            AudioSource audioSource = source.GetComponent<AudioSource>();
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.PlayOneShot(audioSource.clip);
            }
            else
            {
                Debug.LogWarning("AudioSource or AudioClip is missing on the provided source.");
            }
        }
        else
        {
            Debug.LogWarning("Source GameObject is not assigned in AudioManager.");
        }
    }
}
