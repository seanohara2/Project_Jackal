using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public Slider volumeSlider;
    public Slider sensitivitySlider;
    public Button quitButton;
    public Button firstSelectedButton;
    public JackalController jackalController;

    [Header("Audio Settings")]
    public AudioMixer masterMixer; // Reference to the AudioMixer
    private const string masterVolumeParam = "VolumeParameter"; // Parameter name in the AudioMixer
    private const float minVolumeThreshold = 0.001f; // Threshold for muting audio

    private bool isPaused = false;
    private float originalTimeScale = 1f;

    // Cooldown for D-pad input
    private float navigationCooldown = 0.25f; // 0.25 seconds between inputs
    private float lastNavigationTime;

    void Start()
    {
        // Set initial slider values
        InitializeVolumeSlider();
        InitializeSensitivitySlider();

        // Add listeners for sliders and buttons
        volumeSlider.onValueChanged.AddListener(SetVolume);
        sensitivitySlider.onValueChanged.AddListener(SetSensitivity);
        quitButton.onClick.AddListener(QuitToMainMenu);

        if (firstSelectedButton != null)
        {
            firstSelectedButton.Select();
        }

        pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Start"))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        if (isPaused && Input.GetButtonDown("BButton"))
        {
            ResumeGame();
        }

        if (isPaused)
        {
            NavigateMenu();
        }
    }

    private void InitializeVolumeSlider()
    {
        float midpoint = 0.5f; // Midpoint of slider (range 0 to 1)
        volumeSlider.value = midpoint;
        SetVolume(midpoint); // Apply midpoint volume to the AudioMixer
    }

    private void InitializeSensitivitySlider()
    {
        sensitivitySlider.minValue = 50f;
        sensitivitySlider.maxValue = 200f;

        float defaultSensitivity = (sensitivitySlider.minValue + sensitivitySlider.maxValue) / 2f;
        sensitivitySlider.value = defaultSensitivity;
        jackalController.cameraRotationSpeed = defaultSensitivity;
    }

    public void PauseGame()
    {
        isPaused = true;
        originalTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);

        if (firstSelectedButton != null)
        {
            firstSelectedButton.Select();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = originalTimeScale;
        pauseMenuUI.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetVolume(float value)
    {
        // Mute audio if the slider value is close to zero
        if (value <= minVolumeThreshold)
        {
            masterMixer.SetFloat(masterVolumeParam, -80f); // Mute audio
        }
        else
        {
            // Convert slider value (0 to 1) to decibels and set volume
            masterMixer.SetFloat(masterVolumeParam, Mathf.Log10(value) * 20);
        }
    }

    private void SetSensitivity(float value)
    {
        jackalController.cameraRotationSpeed = value;
    }

    private void QuitToMainMenu()
    {
        Time.timeScale = originalTimeScale;
        SceneManager.LoadScene("MainMenu");
    }

    private void NavigateMenu()
    {
        // Cooldown check for navigation
        if (Time.unscaledTime - lastNavigationTime < navigationCooldown)
        {
            return;
        }

        // Read vertical and horizontal inputs from D-pad or left stick
        float vertical = (Input.GetAxis("DPadVertical") + Input.GetAxis("Vertical") * 0.5f); // Scale down left stick input
        float horizontal = Input.GetAxis("DPadHorizontal") + Input.GetAxis("Horizontal");

        // Get the current selected GameObject
        var currentSelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;

        if (currentSelected == null)
        {
            if (firstSelectedButton != null)
            {
                firstSelectedButton.Select();
            }
            return;
        }

        // Vertical navigation
        if (Mathf.Abs(vertical) > 0.5f)
        {
            UnityEngine.UI.Selectable current = currentSelected.GetComponent<UnityEngine.UI.Selectable>();
            if (current != null)
            {
                // Determine the next selectable UI element
                UnityEngine.UI.Selectable next = vertical > 0 ? current.FindSelectableOnUp() : current.FindSelectableOnDown();

                if (next != null)
                {
                    next.Select();
                    lastNavigationTime = Time.unscaledTime; // Update cooldown timer
                }
            }
        }

        // Horizontal navigation for sliders (Left/Right)
        if (Mathf.Abs(horizontal) > 0.5f)
        {
            if (currentSelected.TryGetComponent(out Slider slider))
            {
                float step = slider == volumeSlider ? 0.1f : 5f; // Step size: 0.1 for volume, 5 for sensitivity
                slider.value = Mathf.Clamp(slider.value + (horizontal > 0 ? step : -step), slider.minValue, slider.maxValue);
                lastNavigationTime = Time.unscaledTime; // Update cooldown timer
            }
        }
    }
}
