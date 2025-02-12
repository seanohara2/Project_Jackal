using UnityEngine;
using System.Collections;

public class BoulderCollision : MonoBehaviour
{
    private CheckpointManager checkpointManager;
    [SerializeField] private ParticleSystem collisionEffect;
    [SerializeField] private float effectDuration = 0.25f;
    private bool isResetting = false;

    private void Start()
    {
        checkpointManager = FindObjectOfType<CheckpointManager>();
        if (checkpointManager == null)
        {
            Debug.LogError("CheckpointManager not found in the scene.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Player") && checkpointManager != null && !isResetting)
        {
            isResetting = true;

            if (collisionEffect != null && !collisionEffect.isPlaying)
            {
                collisionEffect.gameObject.SetActive(true);
                collisionEffect.Play();

                AudioManager.Instance.PlayBoulderCollisionSound();//plays audio
            }

            StartCoroutine(ResetAfterEffect());
        }
    }

    private IEnumerator ResetAfterEffect()
    {
        yield return new WaitForSeconds(effectDuration);

        if (collisionEffect != null)
        {
            collisionEffect.Stop();
            collisionEffect.gameObject.SetActive(false);
        }

        // Call ResetToCheckpoint directly
        checkpointManager.ResetToCheckpoint();

        isResetting = false;
    }
}
