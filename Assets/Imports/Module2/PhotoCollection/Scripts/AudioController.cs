using System.Collections.Generic;
using UnityEngine;
namespace DynamicPhotoCamera
{
    /// Controls photo sounds
    public class AudioController : MonoBehaviour
    {
        public List<AudioSource> shotSounds;
        public List<AudioSource> dragSounds;
        public List<AudioSource> dropSounds;
        public List<AudioSource> clickSounds;
        public List<AudioSource> buttonSounds;

        // Plays a random sound from list
        public void PlayRandomSound(List<AudioSource> audioSources)
        {
            Debug.Log("Play sound.");
            List<AudioSource> tempAudioSources = new List<AudioSource>(audioSources);
            if (tempAudioSources == null || tempAudioSources.Count == 0)
            {
                return;
            }

            int randomIndex = Random.Range(0, tempAudioSources.Count);

            if (tempAudioSources[randomIndex] != null)
            {
                tempAudioSources[randomIndex].Play();
            }
            else
            {
                Debug.LogWarning($"AudioSource with index {randomIndex} in list is not set!");
            }
        }
    }
}