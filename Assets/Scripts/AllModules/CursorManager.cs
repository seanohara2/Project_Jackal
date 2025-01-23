using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class CursorManager : MonoBehaviour
{
    public GameObject cursor;
    public float cursorSpeed = 10f;

    private Vector2 cursorPosition;
    private EventSystem eventSystem;
    private Slider activeSlider; // Track the currently dragged slider

    private void Start()
    {
        eventSystem = EventSystem.current;
        cursorPosition = new Vector2(Screen.width / 2, Screen.height / 2);
    }

    public void Update()
    {
        if (cursor.activeSelf)
        {
            UpdateCursorPosition();
            DetectUIElementUnderCursor();
        }
    }

    public void ShowCursor()
    {
        cursor.SetActive(true);
        cursorPosition = new Vector2(Screen.width / 2, Screen.height / 2);
        cursor.transform.position = cursorPosition;
    }

    public void HideCursor()
    {
        cursor.SetActive(false);
    }

    private void UpdateCursorPosition()
    {
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            cursorPosition = Input.mousePosition;
            eventSystem.SetSelectedGameObject(null);
        }
        else if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
        {
            cursorPosition += new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * cursorSpeed;
            cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, Screen.width);
            cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, Screen.height);
        }

        cursor.transform.position = cursorPosition;
    }

    private void DetectUIElementUnderCursor()
    {
        PointerEventData pointerData = new PointerEventData(eventSystem) { position = cursorPosition };
        List<RaycastResult> results = new List<RaycastResult>();
        eventSystem.RaycastAll(pointerData, results);

        bool foundUIElement = false;

        foreach (RaycastResult result in results)
        {
            // Handle Sliders
            Slider slider = result.gameObject.GetComponentInParent<Slider>(); // Look in parent for Slider
            Button button = result.gameObject.GetComponent<Button>();

            if (slider != null)
            {
                foundUIElement = true;
                eventSystem.SetSelectedGameObject(result.gameObject);

                // Handle slider drag with A button
                if (Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    activeSlider = slider; // Start dragging the slider
                }
                break;
            }
            else if (button != null)
            {
                foundUIElement = true;
                eventSystem.SetSelectedGameObject(result.gameObject);

                // Handle button click with A button
                if (Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    button.onClick.Invoke();
                }
                break;
            }
        }

        // Stop dragging when A button is released
        if (Input.GetKeyUp(KeyCode.JoystickButton0))
        {
            activeSlider = null;
        }

        // If a slider is currently being dragged, update its value based on cursor position
        if (activeSlider != null)
        {
            RectTransform rectTransform = activeSlider.GetComponent<RectTransform>();
            float sliderMinX = rectTransform.position.x - rectTransform.sizeDelta.x / 2;
            float sliderMaxX = rectTransform.position.x + rectTransform.sizeDelta.x / 2;
            float sliderValue = Mathf.InverseLerp(sliderMinX, sliderMaxX, cursorPosition.x);
            activeSlider.value = Mathf.Clamp(sliderValue, activeSlider.minValue, activeSlider.maxValue);
        }

        if (!foundUIElement)
        {
            eventSystem.SetSelectedGameObject(null);
        }
    }
}
