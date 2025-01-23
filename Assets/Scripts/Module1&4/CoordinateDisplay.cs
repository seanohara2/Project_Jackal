using UnityEngine;
using TMPro;

public class CoordinateDisplay : MonoBehaviour
{
    public Transform player; // Reference to the player
    public TextMeshProUGUI coordinateText; // Reference to the TextMeshProUGUI component
    [SerializeField] private float latScale = 0.01f; // Scale for latitude (Z-axis)
    [SerializeField] private float lonScale = 0.01f; // Scale for longitude (X-axis)

    private void Update()
    {
        // Map the player's position to "latitude" and "longitude" based on a smaller scale
        float longitude = player.position.x * lonScale;
        float latitude = player.position.z * latScale;

        // Convert latitude to degrees and minutes
        int latDegrees = Mathf.FloorToInt(Mathf.Abs(latitude));
        int latMinutes = Mathf.FloorToInt((Mathf.Abs(latitude) - latDegrees) * 60);
        string latDirection = latitude >= 0 ? "N" : "S";

        // Convert longitude to degrees and minutes
        int lonDegrees = Mathf.FloorToInt(Mathf.Abs(longitude));
        int lonMinutes = Mathf.FloorToInt((Mathf.Abs(longitude) - lonDegrees) * 60);
        string lonDirection = longitude >= 0 ? "E" : "W";

        // Display the formatted coordinates
        coordinateText.text = $"Latitude: {latDegrees}° {latMinutes}' {latDirection}\nLongitude: {lonDegrees}° {lonMinutes}' {lonDirection}";
    }
}
