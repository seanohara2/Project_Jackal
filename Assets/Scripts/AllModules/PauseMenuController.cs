using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour, IMenu
{
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject mainGameUI;

    private CursorManager cursorManager;
    private SettingsController settingsController;
    private bool isPaused = false;
    private GameObject lastSelectedButton;

    private void Start()
    {
        cursorManager = FindObjectOfType<CursorManager>(); // Reference to shared cursor manager
        settingsController = FindObjectOfType<SettingsController>();
        InitializeMenus();
    }

    private void Update()
    {
        HandlePauseToggle();
        if (isPaused)
        {
            cursorManager.Update(); // Use shared cursor manager update
        }
    }

    private void InitializeMenus()
    {
        pauseMenu.SetActive(false);
        settingsMenu.SetActive(false);
    }

    private void HandlePauseToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        TogglePauseMenuVisibility(true);
        cursorManager.ShowCursor();
        lastSelectedButton = EventSystem.current.currentSelectedGameObject;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        TogglePauseMenuVisibility(false);
        cursorManager.HideCursor();
        EventSystem.current.SetSelectedGameObject(lastSelectedButton);
    }

    private void TogglePauseMenuVisibility(bool isVisible)
    {
        pauseMenu.SetActive(isVisible);
        settingsMenu.SetActive(false);
        mainGameUI.SetActive(!isVisible);
    }

    public void QuitToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSettingsMenu()
    {
        TogglePauseMenuVisibility(false);
        settingsMenu.SetActive(true);
        cursorManager.ShowCursor();
    }

    public void CloseSettingsMenu()
    {
        settingsMenu.SetActive(false);
        TogglePauseMenuVisibility(true);
        cursorManager.ShowCursor();
    }

    public void Show()
    {
        TogglePauseMenuVisibility(true);
    }

    public void Hide()
    {
        pauseMenu.SetActive(false);
    }
}
