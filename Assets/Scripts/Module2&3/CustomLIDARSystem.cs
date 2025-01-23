using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiDARController : MonoBehaviour
{
    public float scanRange = 10f;                  // Range of the LiDAR
    public int scanResolution = 360;              // Number of rays per scan
    public float rotationSpeed = 10f;             // Rotation speed of the scanner
    public Transform scannerPivot;                // Pivot point of the scanner
    public GameObject lineRendererPrefab;         // Prefab for visualizing rays
    public LayerMask lidarLayer;                  // Layer for detectable objects
    public Color defaultColor = Color.blue;       // Default color for no hits
    public Color hitColor = Color.red;            // Color for hits

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private bool isScanning = false;              // LiDAR scanning state (off by default)

    void Start()
    {
        // Create LineRenderers for each ray
        for (int i = 0; i < scanResolution; i++)
        {
            GameObject lineObj = Instantiate(lineRendererPrefab, scannerPivot);
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2; // Ensure LineRenderer has two points
            lineRenderer.enabled = false;  // Disable initially
            lineRenderers.Add(lineRenderer);
        }
    }

    void Update()
    {
        // Toggle scanning when X key or button (JoystickButton2) is pressed
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            ToggleScanning();
        }
    }

    public void ToggleScanning()
    {
        isScanning = !isScanning;
        SetLineRenderersEnabled(isScanning); // Enable or disable LineRenderers
    }

    private void SetLineRenderersEnabled(bool enabled)
    {
        foreach (var lineRenderer in lineRenderers)
        {
            lineRenderer.enabled = enabled;
        }
    }

    void FixedUpdate()
    {
        if (!isScanning) return; // Exit if scanning is turned off

        for (int i = 0; i < scanResolution; i++)
        {
            Vector3 origin = scannerPivot.position;
            Vector3 direction = Quaternion.Euler(0, i * (360f / scanResolution), 0) * scannerPivot.forward;

            SimulateRay(origin, direction, i);
        }

        // Rotate scanner
        if (scannerPivot != null)
        {
            scannerPivot.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    void SimulateRay(Vector3 origin, Vector3 direction, int rayIndex)
    {
        if (Physics.Raycast(origin, direction, out RaycastHit hit, scanRange, lidarLayer))
        {
            // Update LineRenderer for a hit
            lineRenderers[rayIndex].SetPosition(0, origin);
            lineRenderers[rayIndex].SetPosition(1, hit.point);
            lineRenderers[rayIndex].startColor = hitColor;
            lineRenderers[rayIndex].endColor = hitColor;

            // Highlight the hit object
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_EmissionColor"))
            {
                renderer.material.SetColor("_EmissionColor", Color.green); // Add glow
                StartCoroutine(ResetEmission(renderer));
            }
        }
        else
        {
            // Update LineRenderer for no hit
            lineRenderers[rayIndex].SetPosition(0, origin);
            lineRenderers[rayIndex].SetPosition(1, origin + direction * scanRange);
            lineRenderers[rayIndex].startColor = defaultColor;
            lineRenderers[rayIndex].endColor = defaultColor;
        }
    }

    IEnumerator ResetEmission(Renderer renderer)
    {
        yield return new WaitForSeconds(1f); // Keep the glow for 1 second
        if (renderer != null && renderer.material.HasProperty("_EmissionColor"))
        {
            renderer.material.SetColor("_EmissionColor", Color.black); // Reset glow
        }
    }

    private void OnDisable()
    {
        // Ensure LineRenderers are disabled when the script is disabled
        SetLineRenderersEnabled(false);
    }

    private void OnEnable()
    {
        // Ensure LineRenderers match the scanning state when the script is enabled
        SetLineRenderersEnabled(isScanning);
    }
}
