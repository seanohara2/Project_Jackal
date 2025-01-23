using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sensitivitySlider;

    private CursorManager cursorManager;

    private void Start()
    {
        cursorManager = FindObjectOfType<CursorManager>(); // Reference to shared cursor manager
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1.0f);
        sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1.0f);

        OnVolumeChanged();
    }

    private void Update()
    {
        if (cursorManager.cursor.activeSelf)
        {
            cursorManager.Update(); // Use shared cursor manager update
        }
    }

    public void ShowCursor()
    {
        cursorManager.ShowCursor();
    }

    public void HideCursor()
    {
        cursorManager.HideCursor();
    }

    public void OnVolumeChanged()
    {
        float volume = Mathf.Round(volumeSlider.value * 100f) / 100f;
        PlayerPrefs.SetFloat("Volume", volume);
        AudioListener.volume = Mathf.Clamp01(volume);
    }

    public void OnSensitivityChanged()
    {
        float sensitivity = sensitivitySlider.value;
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
    }
}
