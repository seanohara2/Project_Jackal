using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreenController : MonoBehaviour
{
    public string mainMenuSceneName = "MainMenu"; // Name of the main menu scene

    public void RestartLevel()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMainMenu()
    {
        // Load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
