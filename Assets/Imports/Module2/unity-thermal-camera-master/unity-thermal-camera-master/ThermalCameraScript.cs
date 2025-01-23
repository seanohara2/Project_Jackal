using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class ThermalCameraScript : MonoBehaviour
{
    [Header("Resource References")]
    [Tooltip("ThermalVisionPostProcessing.shader")]
    public Shader TVPostProcessing;
    [Tooltip("ThermalVisionSurfaceReplacement.shader")]
    public Shader TVSurfaceReplacement;
    [Tooltip("ThermalVisionPalettes.png")]
    public Texture2D paletteTex;

    [Header("Palette")]
    public int useTexture = 1;
    public Color coolColor;
    public Color midColor;
    public Color warmColor;

    [Header("Parameters")]
    public float environmentTemperature = 0.2f;

    public bool TV = false; // Thermal Vision
    private Camera cam;

    private Material TVPostProcessingMaterial = null;
    private Material SkyboxMaterialCached = null;
    private Material SkyboxMaterialReplacement = null;

    [Header("Text References")]
    public TextMeshProUGUI rgbText; // Reference to RGB Text
    public TextMeshProUGUI temperatureText; // Reference to Temperature Text

    // List to track active TemperatureControllers
    private List<TemperatureController> activeTemperatureControllers = new List<TemperatureController>();

    void Awake()
    {
        SkyboxMaterialCached = RenderSettings.skybox;
        TVPostProcessingMaterial = new Material(TVPostProcessing);
        SkyboxMaterialReplacement = new Material(TVSurfaceReplacement);

        cam = GetComponent<Camera>();

        // Initialize the text visibility based on the initial thermal mode state
        UpdateTextVisibility();

        // Populate activeTemperatureControllers with all active TemperatureController components in the scene
        activeTemperatureControllers.AddRange(FindObjectsOfType<TemperatureController>());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.JoystickButton2))
        {
            TV = !TV;
            UpdateTextVisibility();

            if (TV)
            {
                EnableThermalView();
            }
            else
            {
                DisableThermalView();
            }
        }

        if (TV)
        {
            foreach (var TC in activeTemperatureControllers)
            {
                Renderer renderer = TC.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material.color = new Color(TC.temperature, 0, 0, 0);
                }
            }
        }
    }

    public void EnableThermalView()
    {
        TV = true; // Enable thermal vision
        cam.SetReplacementShader(TVSurfaceReplacement, "RenderType");

        foreach (var TC in activeTemperatureControllers)
        {
            Renderer renderer = TC.GetComponent<Renderer>();
            if (renderer != null)
            {
                TC.cachedMaterialTag = renderer.material.GetTag("RenderType", false);
                TC.cachedColor = renderer.material.color;
                renderer.material.SetOverrideTag("RenderType", "Temperature");
            }
        }

        RenderSettings.skybox = SkyboxMaterialReplacement;
        UpdateTextVisibility();
    }

    public void DisableThermalView()
    {
        TV = false; // Disable thermal vision
        cam.ResetReplacementShader();

        foreach (var TC in activeTemperatureControllers)
        {
            Renderer renderer = TC.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.SetOverrideTag("RenderType", TC.cachedMaterialTag);
                renderer.material.color = TC.cachedColor;
            }
        }

        RenderSettings.skybox = SkyboxMaterialCached;
        UpdateTextVisibility();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (TV)
        {
            TVPostProcessingMaterial.SetTexture("_CameraDepthTexture", Shader.GetGlobalTexture("_CameraDepthTexture"));
            Shader.SetGlobalFloat("_ThermalAmplify", 2.0f);
            Shader.SetGlobalFloat("_EnvironmentTemperature", environmentTemperature);

            if (useTexture == 1 || useTexture == 2)
            {
                TVPostProcessingMaterial.SetInt("useTexture", useTexture);
                TVPostProcessingMaterial.SetTexture("_PaletteTex", paletteTex);
            }
            else
            {
                TVPostProcessingMaterial.SetInt("useTexture", 0);
                TVPostProcessingMaterial.SetColor("coolColor", coolColor);
                TVPostProcessingMaterial.SetColor("midColor", midColor);
                TVPostProcessingMaterial.SetColor("warmColor", warmColor);
            }

            Graphics.Blit(src, dst, TVPostProcessingMaterial);
        }
        else
        {
            Graphics.Blit(src, dst);
        }
    }

    private void UpdateTextVisibility()
    {
        if (TV)
        {
            rgbText?.gameObject.SetActive(false);
            temperatureText?.gameObject.SetActive(true);
        }
        else
        {
            rgbText?.gameObject.SetActive(true);
            temperatureText?.gameObject.SetActive(false);
        }
    }
}
