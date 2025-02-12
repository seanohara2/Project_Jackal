using UnityEngine;

public class GameEndTrigger2 : MonoBehaviour
{
    [SerializeField] private PlayerStatsManager2 playerStatsManager2; // Reference to PlayerActivityManager

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerStatsManager2 != null)
            {
                playerStatsManager2.EndGame();
                AudioManager.Instance.PlayWinSound(); // Play audio
            }
            else
            {
                Debug.LogError("PlayerActivityManager reference is missing in GameEndTrigger.");
            }
        }
    }
}