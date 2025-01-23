using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]
public class LevelInfo
{
    public GameObject description; // Description GameObject for the level
    public GameObject title;       // Title GameObject for the level
}

public class MenuController : MonoBehaviour
{
    [Header("References")]
    public SettingsMenu settingsMenuScript; // Reference to the SettingsMenu script


    [Header("Menus")]
    public GameObject startMenu;
    public GameObject selectionMenu;
    public GameObject settingsMenu;
    public GameObject inspectScreen;

    [Header("Levels")]
    public LevelInfo[] levels; // Array of levels with descriptions and titles

    [Header("Buttons")]
    public Button playButton;
    public Button quitButton;
    public Button settingsButton;
    public Button inspectJackalButton;
    public Button[] levelButtons; // Level buttons 1, 2, 3, 4
    public Button levelPlayButtonComponent;
    public Button backButtonSelection;
    public Button backButtonSettings;
    public Button backButtonInspect;

    private int selectedLevelIndex = 0; // Default to level 0 (Module 1)
    private bool isMenuActive = true;

    // Reference to the ObjectInspector
    public ObjectInspector objectInspector;

    // Navigation cooldown variables
    private float navigationCooldown = 0.25f; // 0.25 seconds between inputs
    private float lastNavigationTime;

    // Track the current menu
    private GameObject currentMenu;

    // Controller detection flag
    private bool isControllerConnected;

    private void Start()
    {
        DetectController();
        ShowStartMenu();
        StartCoroutine(SelectPlayButtonNextFrame());
    }

    private IEnumerator SelectPlayButtonNextFrame()
    {
        yield return null; // Wait for the next frame
        if (isControllerConnected)
        {
            SelectButton(playButton);
        }
    }

    private void Update()
    {
        HandleControllerBackInput();
        NavigateMenu();
        DetectController();
    }

    private void DetectController()
    {
        // Check for controller input
        bool controllerDetected = Input.GetJoystickNames().Length > 0 && !string.IsNullOrEmpty(Input.GetJoystickNames()[0]);

        if (controllerDetected != isControllerConnected)
        {
            isControllerConnected = controllerDetected;

            if (isControllerConnected)
            {
                EnableDefaultButtonHighlights();
            }
            else
            {
                DisableDefaultButtonHighlights();
            }
        }
    }

    private void EnableDefaultButtonHighlights()
    {
        // Only set button selection if a controller is connected
        if (isControllerConnected && EventSystem.current.currentSelectedGameObject == null && playButton != null)
        {
            SelectButton(playButton);
        }
    }

    private void DisableDefaultButtonHighlights()
    {
        // Prevent any UI navigation or selection when no controller is connected
        EventSystem.current.SetSelectedGameObject(null);
    }

    // Start Menu actions
    public void PlayButtonClicked()
    {
        ShowSelectionMenu();
        LevelButtonClicked(selectedLevelIndex); // Show default level description
    }

    public void QuitButtonClicked()
    {
        Application.Quit();
    }

    // Selection Menu actions
    public void BackToStartMenu()
    {
        Debug.Log("step 2");
        ShowStartMenu();
        SelectButton(playButton);
    }

    public void LevelButtonClicked(int levelIndex)
    {
        selectedLevelIndex = levelIndex;
        ShowLevelDescription(levelIndex);
        SelectButton(levelButtons[levelIndex]);
    }

    public void StartLevel()
    {
        if (selectedLevelIndex >= 0)
        {
            string sceneName = "Module_" + (selectedLevelIndex + 1);
            SceneManager.LoadScene(sceneName); // Load the correct scene based on the selected level
        }
    }

    public void OpenSettingsMenu()
    {
        ShowSettingsMenu();
        SelectFirstButtonInMenu(settingsMenu);
    }

    public void OpenInspectScreen()
    {
        ShowInspectScreen();

        // Start the inspection
        if (objectInspector != null)
        {
            objectInspector.StartInspection();
        }
    }

    public void BackToSelectionMenuFromInspect()
    {
        ShowSelectionMenu();
        SelectButton(levelButtons[0]); // Select the first level button (default)

        // Stop the inspection
        if (objectInspector != null)
        {
            objectInspector.StopInspection();
        }
    }

    public void BackToSelectionMenu()
    {
        ShowSelectionMenu();
        SelectButton(levelButtons[0]); // Select the first level button (default)
    }

    // UI transitions
    private void ShowStartMenu()
    {
        Debug.Log("step 3");
        startMenu.SetActive(true);
        selectionMenu.SetActive(false);
        settingsMenu.SetActive(false);
        inspectScreen.SetActive(false);
        HideAllLevelDescriptions();

        currentMenu = startMenu; // Track the current menu

        if (isControllerConnected)
        {
            SelectButton(playButton);
        }
    }

    private void ShowSelectionMenu()
    {
        startMenu.SetActive(false);
        selectionMenu.SetActive(true);
        settingsMenu.SetActive(false);
        inspectScreen.SetActive(false);
        HideAllLevelDescriptions();
        ShowLevelDescription(selectedLevelIndex);

        currentMenu = selectionMenu; // Track the current menu

        if (isControllerConnected)
        {
            SelectButton(levelButtons[0]);
        }
    }

    private void ShowSettingsMenu()
    {
        startMenu.SetActive(false);
        selectionMenu.SetActive(false);
        settingsMenu.SetActive(true);
        inspectScreen.SetActive(false);

        currentMenu = settingsMenu; // Track the current menu

        if (isControllerConnected)
        {
            SelectFirstButtonInMenu(settingsMenu);
        }
    }

    private void ShowInspectScreen()
    {
        startMenu.SetActive(false);
        selectionMenu.SetActive(false);
        settingsMenu.SetActive(false);
        inspectScreen.SetActive(true);

        currentMenu = inspectScreen; // Track the current menu

        if (isControllerConnected)
        {
            SelectFirstButtonInMenu(inspectScreen);
        }
    }

    private void ShowLevelDescription(int levelIndex)
    {
        HideAllLevelDescriptions();
        selectedLevelIndex = levelIndex;

        if (levelIndex >= 0 && levelIndex < levels.Length)
        {
            levels[levelIndex].description.SetActive(true);
            levels[levelIndex].title.SetActive(true);
        }

        levelPlayButtonComponent.gameObject.SetActive(true); // Show play button when a level is selected
    }

    private void HideAllLevelDescriptions()
    {
        foreach (LevelInfo level in levels)
        {
            level.description.SetActive(false);
            level.title.SetActive(false);
        }
        levelPlayButtonComponent.gameObject.SetActive(false); // Hide play button when no level is selected
    }

    private void HandleControllerBackInput()
    {
        if (!isMenuActive)
            return;

        // Check for "B" (or Circle) input
        if (Input.GetButtonDown("BButton"))
        {
            // Determine the correct menu to go back to based on the current menu
            if (currentMenu == selectionMenu)
            {
                Debug.Log("step 1");
                BackToStartMenu(); // Go back to the Start Menu
            }
            else if (currentMenu == inspectScreen)
            {
                BackToSelectionMenuFromInspect(); // Go back to the Selection Menu from the Inspect Menu
            }
            else if (currentMenu == settingsMenu)
            {
                BackToSelectionMenu(); // Go back to the Selection Menu from the Settings Menu
            }
            else if (currentMenu == startMenu)
            {
                // Do nothing, as Start Menu is the root menu
                Debug.Log("Already at the Start Menu. No further navigation possible.");
            }
        }
    }


    private void SelectButton(Button button)
    {
        if (button != null && isControllerConnected) // Only select if a controller is connected
        {
            EventSystem.current.SetSelectedGameObject(null); // Deselect any currently selected object
            EventSystem.current.SetSelectedGameObject(button.gameObject);
            button.OnSelect(null); // Optionally trigger OnSelect event
        }
        else
        {
            EventSystem.current.SetSelectedGameObject(null); // Ensure nothing is selected
        }
    }

    private void SelectFirstButtonInMenu(GameObject menu)
    {
        if (!isControllerConnected) return; // Do nothing if no controller is connected

        Button firstButton = menu.GetComponentInChildren<Button>();
        if (firstButton != null)
        {
            SelectButton(firstButton);
        }
    }

    private void NavigateMenu()
    {
        // Cooldown check for navigation
        if (Time.unscaledTime - lastNavigationTime < navigationCooldown)
            return;

        // Read vertical and horizontal inputs from D-pad or left stick
        float vertical = (Input.GetAxis("DPadVertical") + Input.GetAxis("Vertical") * 0.4f); // Less sensitive
        float horizontal = (Input.GetAxis("DPadHorizontal") + Input.GetAxis("Horizontal") * 0.4f); // Less sensitive

        var currentSelected = EventSystem.current.currentSelectedGameObject;

        // Check if we're in the SettingsMenu and dealing with sliders
        if (currentMenu == settingsMenu.gameObject)
        {
            // Handle horizontal input for sliders
            if (Mathf.Abs(horizontal) > 0.5f && currentSelected != null)
            {
                if (currentSelected.TryGetComponent(out Slider slider))
                {
                    // Adjust step size for sliders
                    float step = slider == settingsMenuScript.sensitivitySlider ? 5f : 0.1f; // Larger step for sensitivity
                    slider.value = Mathf.Clamp(slider.value + (horizontal > 0 ? step : -step), slider.minValue, slider.maxValue);
                    lastNavigationTime = Time.unscaledTime; // Update cooldown timer
                    return;
                }
            }
        }

        // Vertical navigation
        if (Mathf.Abs(vertical) > 0.5f)
        {
            Selectable current = currentSelected.GetComponent<Selectable>();
            if (current != null)
            {
                Selectable next = vertical > 0 ? current.FindSelectableOnUp() : current.FindSelectableOnDown();

                if (next != null)
                {
                    next.Select();
                    lastNavigationTime = Time.unscaledTime; // Update cooldown timer
                }
            }
        }

        // Horizontal navigation (non-slider logic)
        if (Mathf.Abs(horizontal) > 0.5f && currentSelected.TryGetComponent<Button>(out var button))
        {
            Selectable current = button.GetComponent<Selectable>();
            if (current != null)
            {
                Selectable next = horizontal > 0 ? current.FindSelectableOnRight() : current.FindSelectableOnLeft();

                if (next != null)
                {
                    next.Select();
                    lastNavigationTime = Time.unscaledTime; // Update cooldown timer
                }
            }
        }
    }
    


}




/*using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]
public class LevelInfo
{
    public GameObject description; // Description GameObject for the level
    public GameObject title;       // Title GameObject for the level
}

public class MenuController : MonoBehaviour
{
    [Header("Menus")]
    public GameObject startMenu;
    public GameObject selectionMenu;
    public GameObject settingsMenu;
    public GameObject inspectScreen;

    [Header("Levels")]
    public LevelInfo[] levels; // Array of levels with descriptions and titles

    [Header("Buttons")]
    public Button playButton;
    public Button quitButton;
    public Button settingsButton;
    public Button inspectJackalButton;
    public Button[] levelButtons; // Level buttons 1, 2, 3, 4
    public Button levelPlayButtonComponent;
    public Button backButtonSelection;
    public Button backButtonSettings;
    public Button backButtonInspect;

    private int selectedLevelIndex = 0; // Default to level 0 (Module 1)
    private bool isMenuActive = true;

    // Reference to the ObjectInspector
    public ObjectInspector objectInspector;

    // Navigation cooldown variables
    private float navigationCooldown = 0.25f; // 0.25 seconds between inputs
    private float lastNavigationTime;

    // Track the current menu
    private GameObject currentMenu;

    private void Start()
    {
        ShowStartMenu();
        StartCoroutine(SelectPlayButtonNextFrame());
    }

    private IEnumerator SelectPlayButtonNextFrame()
    {
        yield return null; // Wait for the next frame
        SelectButton(playButton);
    }


    private void Update()
    {
        HandleControllerBackInput();
        NavigateMenu();
    }

    // Start Menu actions
    public void PlayButtonClicked()
    {
        ShowSelectionMenu();
        LevelButtonClicked(selectedLevelIndex); // Show default level description
    }

    public void QuitButtonClicked()
    {
        Application.Quit();
    }

    // Selection Menu actions
    public void BackToStartMenu()
    {
        ShowStartMenu();
        SelectButton(playButton);
    }

    public void LevelButtonClicked(int levelIndex)
    {
        selectedLevelIndex = levelIndex;
        ShowLevelDescription(levelIndex);
        SelectButton(levelButtons[levelIndex]);
    }

    public void StartLevel()
    {
        if (selectedLevelIndex >= 0)
        {
            string sceneName = "Module_" + (selectedLevelIndex + 1);
            SceneManager.LoadScene(sceneName); // Load the correct scene based on the selected level
        }
    }

    public void OpenSettingsMenu()
    {
        ShowSettingsMenu();
        SelectFirstButtonInMenu(settingsMenu);
    }

    public void OpenInspectScreen()
    {
        ShowInspectScreen();

        // Start the inspection
        if (objectInspector != null)
        {
            objectInspector.StartInspection();
        }
    }

    public void BackToSelectionMenuFromInspect()
    {
        ShowSelectionMenu();
        SelectButton(levelButtons[0]); // Select the first level button (default)

        // Stop the inspection
        if (objectInspector != null)
        {
            objectInspector.StopInspection();
        }
    }


    public void BackToSelectionMenu()
    {
        ShowSelectionMenu();
        SelectButton(levelButtons[0]); // Select the first level button (default)
    }

    // UI transitions
    private void ShowStartMenu()
    {
        startMenu.SetActive(true);
        selectionMenu.SetActive(false);
        settingsMenu.SetActive(false);
        inspectScreen.SetActive(false);
        HideAllLevelDescriptions();

        currentMenu = startMenu; // Track the current menu
    }

    private void ShowSelectionMenu()
    {
        startMenu.SetActive(false);
        selectionMenu.SetActive(true);
        settingsMenu.SetActive(false);
        inspectScreen.SetActive(false);
        HideAllLevelDescriptions();
        ShowLevelDescription(selectedLevelIndex);

        currentMenu = selectionMenu; // Track the current menu

        // Select the first level button
        SelectButton(levelButtons[0]);
    }

    private void ShowSettingsMenu()
    {
        startMenu.SetActive(false);
        selectionMenu.SetActive(false);
        settingsMenu.SetActive(true);
        inspectScreen.SetActive(false);

        currentMenu = settingsMenu; // Track the current menu
    }

    private void ShowInspectScreen()
    {
        startMenu.SetActive(false);
        selectionMenu.SetActive(false);
        settingsMenu.SetActive(false);
        inspectScreen.SetActive(true);

        currentMenu = inspectScreen; // Track the current menu
    }

    private void ShowLevelDescription(int levelIndex)
    {
        HideAllLevelDescriptions();
        selectedLevelIndex = levelIndex;

        if (levelIndex >= 0 && levelIndex < levels.Length)
        {
            levels[levelIndex].description.SetActive(true);
            levels[levelIndex].title.SetActive(true);
        }

        levelPlayButtonComponent.gameObject.SetActive(true); // Show play button when a level is selected
    }

    private void HideAllLevelDescriptions()
    {
        foreach (LevelInfo level in levels)
        {
            level.description.SetActive(false);
            level.title.SetActive(false);
        }
        levelPlayButtonComponent.gameObject.SetActive(false); // Hide play button when no level is selected
    }

    private void HandleControllerBackInput()
    {
        if (!isMenuActive)
            return;

        // Go back to the previous menu using "B" (or Circle) input
        if (Input.GetButtonDown("BButton"))
        {
            if (currentMenu == settingsMenu)
            {
                BackToSelectionMenu();
            }
            else if (currentMenu == inspectScreen)
            {
                BackToSelectionMenuFromInspect();
            }
            else if (currentMenu == selectionMenu)
            {
                BackToStartMenu();
            }
        }
    }

    private void SelectButton(Button button)
    {
        if (button != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // Deselect any currently selected object
            EventSystem.current.SetSelectedGameObject(button.gameObject);
            button.OnSelect(null); // Optionally trigger OnSelect event
        }
    }

    private void SelectFirstButtonInMenu(GameObject menu)
    {
        Button firstButton = menu.GetComponentInChildren<Button>();
        if (firstButton != null)
        {
            SelectButton(firstButton);
        }
    }

    private void NavigateMenu()
    {
        // Cooldown check for navigation
        if (Time.unscaledTime - lastNavigationTime < navigationCooldown)
        {
            return;
        }

        // Read vertical and horizontal inputs from D-pad or left stick
        float vertical = (Input.GetAxis("DPadVertical") * 0.5f + Input.GetAxis("Vertical") * 0.5f); // Scale down left stick input
        float horizontal = Input.GetAxis("DPadHorizontal") + Input.GetAxis("Horizontal");

        // Get the current selected GameObject
        var currentSelected = EventSystem.current.currentSelectedGameObject;

        if (currentSelected == null)
        {
            // Try to select a default button
            if (currentMenu == startMenu && playButton != null)
            {
                SelectButton(playButton);
            }
            else if (currentMenu == selectionMenu && levelButtons.Length > 0)
            {
                SelectButton(levelButtons[0]); // Select the first level button
            }
            else if (currentMenu == settingsMenu && settingsMenu.GetComponentInChildren<Button>() != null)
            {
                SelectButton(settingsMenu.GetComponentInChildren<Button>());
            }
            else if (currentMenu == inspectScreen && inspectScreen.GetComponentInChildren<Button>() != null)
            {
                SelectButton(inspectScreen.GetComponentInChildren<Button>());
            }
            return;
        }

        Selectable current = currentSelected.GetComponent<Selectable>();
        if (current != null)
        {
            // Vertical navigation
            if (Mathf.Abs(vertical) > 0.5f)
            {
                // Determine the next selectable UI element
                Selectable next = vertical > 0 ? current.FindSelectableOnUp() : current.FindSelectableOnDown();

                if (next != null)
                {
                    next.Select();
                    lastNavigationTime = Time.unscaledTime; // Update cooldown timer
                }
            }

            // Horizontal navigation
            if (Mathf.Abs(horizontal) > 0.5f)
            {
                // Check if current selectable is a slider
                if (currentSelected.TryGetComponent(out Slider slider))
                {
                    float step = 0.1f; // Adjust step size if necessary
                    slider.value = Mathf.Clamp(slider.value + (horizontal > 0 ? step : -step), slider.minValue, slider.maxValue);
                    lastNavigationTime = Time.unscaledTime; // Update cooldown timer
                }
                else
                {
                    // Determine the next selectable UI element
                    Selectable next = horizontal > 0 ? current.FindSelectableOnRight() : current.FindSelectableOnLeft();

                    if (next != null)
                    {
                        next.Select();
                        lastNavigationTime = Time.unscaledTime; // Update cooldown timer
                    }
                }
            }
        }
    }
}
*/