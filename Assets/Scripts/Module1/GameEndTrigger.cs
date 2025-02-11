using UnityEngine;

public class GameEndTrigger : MonoBehaviour
{
    [SerializeField] private GameStatsManager gameStatsManager; // Reference to GameStatsManager

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameStatsManager != null)
            {
                gameStatsManager.EndGame();
                AudioManager.Instance.PlayWinSound();//play audio
            }
            else
            {
                Debug.LogError("GameStatsManager reference is missing in GameEndTrigger.");
            }
        }
    }
}
