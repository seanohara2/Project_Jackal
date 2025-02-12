using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameStatsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI restartsText;
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private RawImage[] stars;
    [SerializeField] private Button defaultButton;
    [SerializeField] private Button quitButton; // Second button for horizontal navigation

    [Header("Game References")]
    [SerializeField] private JackalController jackalController;

    [Header("Star System Thresholds")]
    [SerializeField] private float threeStarTime = 210f;
    [SerializeField] private float twoStarTime = 300f;
    [SerializeField] private float resetPenalty = 5f;

    private float gameTime = 0f;
    private int totalRestarts = 0;
    private bool gameEnded = false;

    private void Update()
    {
        if (!gameEnded)
        {
            gameTime += Time.deltaTime;
        }
        else
        {
            NavigateMenu();
        }
    }

    public void IncrementRestarts()
    {
        if (!gameEnded)
        {
            totalRestarts++;
        }
    }

    public void EndGame()
    {
        gameEnded = true;

        if (jackalController != null)
        {
            jackalController.enabled = false;
        }

        float effectiveTime = gameTime + (totalRestarts * resetPenalty);

        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);

            if (timeText != null)
            {
                timeText.text = $"Time: {gameTime:F2} seconds";
            }
            if (restartsText != null)
            {
                restartsText.text = $"Restarts: {totalRestarts}";
            }

            CalculateAndDisplayStars(effectiveTime);
            SelectDefaultButton();
        }
    }

    private void CalculateAndDisplayStars(float effectiveTime)
    {
        int starCount;

        if (effectiveTime <= threeStarTime)
        {
            starCount = 3;
        }
        else if (effectiveTime <= twoStarTime)
        {
            starCount = 2;
        }
        else
        {
            starCount = 1;
        }

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].enabled = i < starCount;
        }
    }

    private void SelectDefaultButton()
    {
        if (defaultButton != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
        }
        else
        {
            Debug.LogWarning("Default button is not assigned in GameStatsManager.");
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void NavigateMenu()
    {
        // Read horizontal D-pad input
        float horizontal = Input.GetAxis("DPadHorizontal");

        // Get the currently selected GameObject
        var currentSelected = EventSystem.current.currentSelectedGameObject;

        if (Mathf.Abs(horizontal) > 0.5f && currentSelected != null)
        {
            // Get the current selectable UI element
            Selectable current = currentSelected.GetComponent<Selectable>();

            if (current != null)
            {
                // Navigate left or right based on horizontal input
                Selectable next = horizontal > 0 ? current.FindSelectableOnRight() : current.FindSelectableOnLeft();

                if (next != null)
                {
                    next.Select();
                }
            }
        }
    }
}



//main
/*using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameStatsManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timeText; // Text for total time
    [SerializeField] private TextMeshProUGUI restartsText; // Text for total restarts
    [SerializeField] private GameObject winScreenPanel; // Win screen panel

    [Header("Game References")]
    [SerializeField] private JackalController jackalController; // Reference to the JackalController script

    private float gameTime = 0f; // Total time spent in the game
    private int totalRestarts = 0; // Total restarts
    private bool gameEnded = false; // Whether the game has ended

    private void Update()
    {
        // Increment game time if the game hasn't ended
        if (!gameEnded)
        {
            gameTime += Time.deltaTime;
        }
    }

    // Increment the restart count (called from CheckpointManager)
    public void IncrementRestarts()
    {
        if (!gameEnded)
        {
            totalRestarts++;
        }
    }

    // Method to call when the player reaches the end of the game
    public void EndGame()
    {
        // Stop incrementing stats
        gameEnded = true;

        // Disable JackalController
        if (jackalController != null)
        {
            jackalController.enabled = false;
        }

        // Show the win screen
        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);

            // Update the UI with stats
            if (timeText != null)
            {
                timeText.text = $"Time: {gameTime:F2} seconds";
            }
            if (restartsText != null)
            {
                restartsText.text = $"Restarts: {totalRestarts}";
            }
        }
    }

    // Restart the level (Retry button functionality)
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Reload the current scene
    }

    // Quit to the main menu (Quit button functionality)
    public void QuitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Replace with your main menu scene name
    }
}
*/