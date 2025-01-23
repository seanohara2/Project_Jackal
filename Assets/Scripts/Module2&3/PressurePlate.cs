using UnityEngine;
using TMPro;
using System.Collections;

public class PressurePlate : MonoBehaviour
{
    public int level; // Specify the level this plate belongs to
    public int groupId; // Specify the group within the level
    public GameObject door; // Assign the door GameObject
    public GameObject[] objectsToDisable; // Array of objects to disable
    public GameObject[] objectsToEnable; // Array of objects to enable
    public TextMeshProUGUI notificationText; // Optional: Text to show a message
    public Animator plateAnimator; // Optional: Animation for plate
    public AudioClip activationSound; // Sound to play when the plate is activated
    public AudioSource audioSource; // AudioSource to play the sound
    public bool isMinecartActivator = false; // If this plate is triggered by a minecart

    private bool isActivated = false;

    private void Start()
    {
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);

        PressurePlateManager.Instance.RegisterPlate(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((isMinecartActivator && other.CompareTag("Minecart")) || (!isMinecartActivator && other.transform.root.CompareTag("Player")))
        {
            ActivatePlate();
            FindObjectOfType<PlayerStatsManager2>()?.ActivatePressurePlate(gameObject);
        }
    }

    private void ActivatePlate()
    {
        if (!isActivated)
        {
            isActivated = true;

            if (door != null)
                door.SetActive(false); // Open door

            if (objectsToDisable != null)
            {
                foreach (GameObject obj in objectsToDisable)
                {
                    if (obj != null)
                        obj.SetActive(false); // Disable the object
                }
            }

            if (objectsToEnable != null)
            {
                foreach (GameObject obj in objectsToEnable)
                {
                    if (obj != null)
                        obj.SetActive(true); // Enable the object
                }
            }

            if (notificationText != null)
            {
                notificationText.gameObject.SetActive(true);
                StartCoroutine(HideTextAfterDelay(10f));
            }

            if (plateAnimator != null)
                plateAnimator.SetTrigger("PressurePlate");

            if (audioSource != null && activationSound != null)
            {
                audioSource.PlayOneShot(activationSound); // Play sound effect
            }

            PressurePlateManager.Instance.PlateActivated(this);
        }
    }

    private IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notificationText != null)
            notificationText.gameObject.SetActive(false);
    }

    public bool IsActivated() => isActivated;
}
