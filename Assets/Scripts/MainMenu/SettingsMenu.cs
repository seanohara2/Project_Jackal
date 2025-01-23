using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public Slider sensitivitySlider;
    public Button backButton;
    public Button firstSelectedButton;

    [Header("Audio Settings")]
    public AudioMixer masterMixer; // Reference to the AudioMixer
    private const string masterVolumeParam = "VolumeParameter"; // Parameter name in the AudioMixer
    private const float minVolumeThreshold = 0.001f; // Minimum threshold to consider as "muted"

    private void Start()
    {
        // Initialize slider values
        InitializeVolumeSlider();
        InitializeSensitivitySlider();

        // Add listeners for sliders and buttons
        volumeSlider.onValueChanged.AddListener(SetVolume);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        backButton.onClick.AddListener(BackToSelectionMenu);

        // Set the first selected button for navigation
        if (firstSelectedButton != null)
        {
            firstSelectedButton.Select();
        }
    }

    private void Update()
    {
        // Only process input if the Settings Menu is active
        if (!gameObject.activeInHierarchy)
            return;

        // Handle "B" button for going back
        if (Input.GetButtonDown("BButton"))
        {
            BackToSelectionMenu();
        }
    }

    private void InitializeVolumeSlider()
    {
        // Set volume slider to midpoint and update audio volume
        float midpoint = 0.5f;
        volumeSlider.value = midpoint;
        SetVolume(midpoint);
    }

    private void InitializeSensitivitySlider()
    {
        // Set sensitivity slider to midpoint of range
        sensitivitySlider.minValue = 50f;
        sensitivitySlider.maxValue = 200f;

        float defaultSensitivity = (sensitivitySlider.minValue + sensitivitySlider.maxValue) / 2f;
        sensitivitySlider.value = defaultSensitivity;
    }

    private void SetVolume(float value)
    {
        if (value <= minVolumeThreshold)
        {
            masterMixer.SetFloat(masterVolumeParam, -80f); // Mute audio
        }
        else
        {
            masterMixer.SetFloat(masterVolumeParam, Mathf.Log10(value) * 20);
        }
    }

    private void SetSensitivity(float value)
    {
        // Update sensitivity in your game settings
        Debug.Log($"Sensitivity set to: {value}");
    }

    private void BackToSelectionMenu()
    {
        MenuController menuController = FindObjectOfType<MenuController>();
        if (menuController != null)
        {
            menuController.BackToSelectionMenu();
        }
    }
}
