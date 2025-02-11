using UnityEngine;
using UnityEngine.UI;

public class Module2ButtonBindings : MonoBehaviour
{
    public PauseMenuController pauseMenuController;
    public SettingsController settingsController;

    // Buttons for pause menu interactions
    public Button resumeButton;
    public Button quitButton;
    public Button settingsButton;
    public Button backButtonSettings;

    // Sliders for settings
    public Slider volumeSlider;
    public Slider sensitivitySlider;

    private void Start()
    {
        // Bind buttons to their respective methods in PauseMenuController
        resumeButton.onClick.AddListener(() => pauseMenuController.ResumeGame());
        quitButton.onClick.AddListener(() => pauseMenuController.QuitToMainMenu());
        settingsButton.onClick.AddListener(() => pauseMenuController.OpenSettingsMenu());
        backButtonSettings.onClick.AddListener(() => pauseMenuController.CloseSettingsMenu());

        // Bind sliders to settings changes in SettingsController
        volumeSlider.onValueChanged.AddListener(_ => settingsController.OnVolumeChanged());
        sensitivitySlider.onValueChanged.AddListener(_ => settingsController.OnSensitivityChanged());

        // Initialize slider values based on current settings
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1.0f);
        sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
    }
}

