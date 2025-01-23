using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PlayerStatsManager2 : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI inGameScoreText; // Displays score during gameplay
    [SerializeField] private TextMeshProUGUI restartsText;    // Displays restart count during gameplay
    [SerializeField] private TextMeshProUGUI winPanelScoreText; // Displays score on the win panel
    [SerializeField] private TextMeshProUGUI winPanelRestartsText; // Displays restart count on the win panel
    [SerializeField] private GameObject winScreenPanel;
    [SerializeField] private RawImage[] stars;
    [SerializeField] private Button defaultButton;
    [SerializeField] private Button quitButton; // Second button for horizontal navigation

    [Header("Game References")]
    [SerializeField] private JackalController jackalController;
    [SerializeField] private List<GameObject> pressurePlates; // Assign all pressure plates in the inspector
    [SerializeField] private List<GameObject> oreObjects;     // Assign all ore objects in the inspector

    [Header("Star System Thresholds")]
    [SerializeField] private float threeStarThreshold = 90f; // Score >=90 for 3 stars
    [SerializeField] private float twoStarThreshold = 60f;  // Score >=60 for 2 stars
    [SerializeField] private float restartPenalty = 5f;     // Points deducted per restart

    private HashSet<GameObject> activatedPlates = new HashSet<GameObject>();
    private HashSet<GameObject> capturedPhotos = new HashSet<GameObject>();
    private int totalRestarts = 0;
    private bool gameEnded = false;

    private void Start()
    {
        UpdateInGameScoreText();
        UpdateRestartText();
    }

    public void ActivatePressurePlate(GameObject plate)
    {
        if (gameEnded || activatedPlates.Contains(plate)) return;

        if (pressurePlates.Contains(plate))
        {
            activatedPlates.Add(plate);
            UpdateInGameScoreText();
        }
    }

    public void CaptureOrePhoto(GameObject ore)
    {
        if (gameEnded || capturedPhotos.Contains(ore)) return;

        if (oreObjects.Contains(ore))
        {
            capturedPhotos.Add(ore);
            UpdateInGameScoreText();
        }
    }

    public void IncrementRestarts()
    {
        if (!gameEnded)
        {
            totalRestarts++;
            UpdateRestartText();
        }
    }

    private void UpdateInGameScoreText()
    {
        inGameScoreText.text = $"Plates: {activatedPlates.Count}/{pressurePlates.Count} | Photos: {capturedPhotos.Count}/{oreObjects.Count}";
    }

    private void UpdateRestartText()
    {
        if (restartsText != null)
        {
            restartsText.text = $"Restarts: {totalRestarts}";
        }
    }

    public void EndGame()
    {
        if (gameEnded) return;

        gameEnded = true;

        if (jackalController != null)
        {
            jackalController.enabled = false;
        }

        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);
        }

        float finalScore = CalculatePerformanceScore();

        if (winPanelScoreText != null)
        {
            winPanelScoreText.text = $"{finalScore:F2}";
        }

        if (winPanelRestartsText != null)
        {
            winPanelRestartsText.text = $"{totalRestarts}";
        }

        CalculateAndDisplayStars(finalScore);
        SelectDefaultButton();
    }

    private float CalculatePerformanceScore()
    {
        float plateScore = (float)activatedPlates.Count / pressurePlates.Count * 100f;
        float photoScore = (float)capturedPhotos.Count / oreObjects.Count * 100f;
        float baseScore = (plateScore + photoScore) / 2f;
        return Mathf.Max(0, baseScore - (totalRestarts * restartPenalty)); // Deduct points for restarts
    }

    private void CalculateAndDisplayStars(float finalScore)
    {
        int starCount = 1;

        if (finalScore >= threeStarThreshold)
        {
            starCount = 3;
        }
        else if (finalScore >= twoStarThreshold)
        {
            starCount = 2;
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
            Debug.LogWarning("Default button is not assigned in PlayerStatsManager.");
        }
    }

    public void RestartLevel()
    {
        // Only restart the level, do not increment restarts here
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