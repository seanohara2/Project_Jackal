using System.Collections.Generic;
using UnityEngine;

public class InputUIManager : MonoBehaviour
{
    public List<GameObject> controllerUIElements;
    public List<GameObject> keyboardUIElements;

    // Simple enum to track current input method
    public enum InputMethod
    {
        Controller,
        KeyboardMouse
    }

    public static InputMethod currentInputMethod = InputMethod.KeyboardMouse;

    void Start()
    {
        // By default, enable keyboard UI and disable controller UI
        SetKeyboardUI();
    }

    void Update()
    {
        bool controllerInputDetected = false;
        bool keyboardMouseInputDetected = false;

        // Detect controller input: Check common controller axes/buttons
        // If any axis or button (like LeftStickVertical, LeftStickHorizontal, RightBumper, LeftBumper) is non-zero or pressed:
        if (Mathf.Abs(Input.GetAxis("LeftStickHorizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxis("LeftStickVertical")) > 0.1f ||
            Input.GetButton("LeftBumper") || Input.GetButton("RightBumper") ||
            Mathf.Abs(Input.GetAxis("RightStickHorizontal")) > 0.1f ||
            Mathf.Abs(Input.GetAxis("RightStickVertical")) > 0.1f)
        {
            controllerInputDetected = true;
        }

        // Detect keyboard/mouse input:
        // Check if any key is pressed this frame or if the mouse moved
        if (Input.anyKeyDown ||
            Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f ||
            Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f)
        {
            keyboardMouseInputDetected = true;
        }

        // Determine which input method to use based on the latest detected input
        if (controllerInputDetected && !keyboardMouseInputDetected)
        {
            // Controller input detected this frame
            if (currentInputMethod != InputMethod.Controller)
            {
                currentInputMethod = InputMethod.Controller;
                SetControllerUI();
            }
        }
        else if (keyboardMouseInputDetected && !controllerInputDetected)
        {
            // Keyboard/mouse input detected this frame
            if (currentInputMethod != InputMethod.KeyboardMouse)
            {
                currentInputMethod = InputMethod.KeyboardMouse;
                SetKeyboardUI();
            }
        }
    }

    private void SetControllerUI()
    {
        // Enable controller UI, disable keyboard UI
        foreach (var go in controllerUIElements)
        {
            if (go != null) go.SetActive(true);
        }
        foreach (var go in keyboardUIElements)
        {
            if (go != null) go.SetActive(false);
        }
    }

    private void SetKeyboardUI()
    {
        // Enable keyboard UI, disable controller UI
        foreach (var go in controllerUIElements)
        {
            if (go != null) go.SetActive(false);
        }
        foreach (var go in keyboardUIElements)
        {
            if (go != null) go.SetActive(true);
        }
    }
}
