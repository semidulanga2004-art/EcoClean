using UnityEngine;
using UnityEngine.Rendering;

// Switches to dense teal fog and an underwater colour grade while the camera is below a WaterZone surface.
[DefaultExecutionOrder(100)] // after ThirdPersonCameraFollow has moved the camera
[RequireComponent(typeof(Camera))]
public sealed class UnderwaterEffect : MonoBehaviour
{
    [SerializeField] private Volume underwaterVolume;
    [SerializeField] private Color fogColor = new Color(0.08f, 0.42f, 0.5f);
    [SerializeField, Min(0f)] private float fogDensity = 0.045f;

    private bool underwater;
    private bool savedFog;
    private FogMode savedMode;
    private Color savedColor;
    private float savedDensity;

    private void Start()
    {
        // Start above water with the underwater grade off, whatever weight the scene was saved with.
        if (underwaterVolume != null && !underwater)
        {
            underwaterVolume.weight = 0f;
        }
    }

    private void LateUpdate()
    {
        Vector3 position = transform.position;
        bool below = WaterZone.TryGetSurface(position, out float surfaceY) && position.y < surfaceY;
        if (below != underwater)
        {
            SetUnderwater(below);
        }
    }

    private void OnDisable()
    {
        if (underwater)
        {
            SetUnderwater(false);
        }
    }

    private void SetUnderwater(bool value)
    {
        underwater = value;
        if (value)
        {
            savedFog = RenderSettings.fog;
            savedMode = RenderSettings.fogMode;
            savedColor = RenderSettings.fogColor;
            savedDensity = RenderSettings.fogDensity;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }
        else
        {
            RenderSettings.fog = savedFog;
            RenderSettings.fogMode = savedMode;
            RenderSettings.fogColor = savedColor;
            RenderSettings.fogDensity = savedDensity;
        }

        if (underwaterVolume != null)
        {
            underwaterVolume.weight = value ? 1f : 0f;
        }
    }
}
