using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIButtonAudioHandler : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip click1Sound; // Sound for "Submit" actions (A button or equivalent)
    public AudioClip click2Sound; // Sound for "Cancel" actions (B button or equivalent)

    [Header("Audio Source")]
    public AudioSource audioSource; // The AudioSource used to play sounds

    private void Update()
    {
        // Check for controller input
        if (Input.GetButtonDown("Submit"))
        {
            PlaySound(click1Sound); // Play click1 sound
        }

        if (Input.GetButtonDown("Cancel"))
        {
            PlaySound(click2Sound); // Play click2 sound
        }

        // Check for mouse input
        if (Input.GetMouseButtonDown(0))
        {
            // Get the currently selected GameObject from the EventSystem (the button being clicked)
            GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

            if (selectedObject != null && selectedObject.GetComponent<Button>() != null)
            {
                // Check if the button has the "Back" tag to determine the sound
                if (selectedObject.CompareTag("Back"))
                {
                    PlaySound(click2Sound); // Play back sound
                }
                else
                {
                    PlaySound(click1Sound); // Play general button click sound
                }
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
