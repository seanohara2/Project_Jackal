using System.Collections.Generic;
using UnityEngine;

public class SwayingWindZone : MonoBehaviour
{
    [SerializeField] private Vector3 baseWindDirection = new Vector3(1, 0, 0); // Base direction of the wind force
    [SerializeField] private float windStrength = 10f; // Base strength of the wind force
    [SerializeField] private float swayFrequency = 1f; // Speed of the wind direction change, in cycles per second
    private List<Rigidbody> affectedRigidbodies = new List<Rigidbody>();

    private void FixedUpdate()
    {
        // Calculate the sway factor using a sine wave for smooth back-and-forth motion
        float swayFactor = Mathf.Sin(Time.time * swayFrequency) * windStrength;

        // Create a swaying wind direction by adjusting the base direction with the sway factor
        Vector3 swayingWind = baseWindDirection.normalized * swayFactor;

        // Apply the swaying wind force to each rigidbody in the zone
        foreach (Rigidbody rb in affectedRigidbodies)
        {
            rb.AddForce(swayingWind, ForceMode.Force);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Add the rigidbody to the list when it enters the wind zone
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && !affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Remove the rigidbody from the list when it leaves the wind zone
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && affectedRigidbodies.Contains(rb))
        {
            affectedRigidbodies.Remove(rb);
        }
    }
}
