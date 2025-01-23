using UnityEngine;

[System.Serializable]
public class InspectableComponent
{
    public GameObject componentObject; // The component to highlight
    public string componentName;       // Name of the component (for the title)

    [TextArea(5, 10)]
    public string description;         // Description of the component
    public Vector3 focusRotation;      // Predefined rotation for the robot when focusing on this component

    public GameObject[] objectsToDisable; // Objects to disable when this component is inspected
}
