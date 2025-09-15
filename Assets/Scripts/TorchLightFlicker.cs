using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TorchLightFlicker : MonoBehaviour
{
    public Light2D torchLight;
    public float flickerSpeed = 5f;
    public float intensityMin = 0.8f;
    public float intensityMax = 1.2f;
    public float radiusVariation = 0.2f;
    private float baseOuterRadius;

    void Start()
    {
        if (torchLight == null)
            torchLight = GetComponent<Light2D>();

        baseOuterRadius = torchLight.pointLightOuterRadius;
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        torchLight.intensity = Mathf.Lerp(intensityMin, intensityMax, noise);
        torchLight.pointLightOuterRadius = baseOuterRadius + (noise - 0.5f) * radiusVariation;
    }
}
