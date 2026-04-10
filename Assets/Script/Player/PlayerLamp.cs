using UnityEngine;

public class PlayerLamp : MonoBehaviour
{
    [SerializeField] private Light lampLight;
    [SerializeField] private float baseLightIntensity = 1f;
    [SerializeField] private float maxLightIntensity = 4f;
    [SerializeField] private float intensityChangeSpeed = 2f;

    private float targetLightIntensity;

    private void Awake()
    {
        if (lampLight == null)
        {
            lampLight = GetComponentInChildren<Light>();
        }

        targetLightIntensity = baseLightIntensity;

        if (lampLight != null)
        {
            lampLight.intensity = baseLightIntensity;
        }
    }

    private void Update()
    {
        if (lampLight == null)
        {
            return;
        }

        lampLight.intensity = Mathf.MoveTowards(
            lampLight.intensity,
            targetLightIntensity,
            intensityChangeSpeed * Time.deltaTime
        );
    }

    public void IncreaseLight(float amount)
    {
        targetLightIntensity = Mathf.Clamp(
            targetLightIntensity + Mathf.Max(0f, amount),
            baseLightIntensity,
            maxLightIntensity
        );
    }
}
