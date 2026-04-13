using UnityEngine;

public class PlayerLamp : MonoBehaviour
{
    [SerializeField] private Light lampLight;
    [SerializeField] private Renderer lampGlowRenderer;
    [SerializeField] private Color lampLightColor = new Color(1f, 0.58f, 0.22f);
    [SerializeField] private float baseLightIntensity = 1f;
    [SerializeField] private float maxLightIntensity = 4f;
    [SerializeField] private float intensityChangeSpeed = 2f;
    [SerializeField] private float baseLightRange = 0.35f;
    [SerializeField] private float maxLightRange = 5f;
    [SerializeField] private float rangeChangeSpeed = 4f;
    [SerializeField] private float baseGlowEmission = 0.05f;
    [SerializeField] private float maxGlowEmission = 3f;
    [SerializeField] private float glowChangeSpeed = 3f;
    [SerializeField] private string lightBoostObjectTag = "object";
    [SerializeField] private float lightIncreaseAmount = 0.5f;
    [SerializeField] private float rangeIncreaseAmount = 1.5f;
    [SerializeField] private float glowIncreaseAmount = 0.75f;
    [SerializeField] private float lightChangeLogInterval = 0.25f;
    [SerializeField] private bool useDramaticLightBoostDefaults = true;

    private float targetLightIntensity;
    private float targetLightRange;
    private float currentGlowEmission;
    private float targetGlowEmission;
    private float nextLightChangeLogTime;

    private void Awake()
    {
        if (lampLight == null)
        {
            lampLight = GetComponentInChildren<Light>();
        }

        if (useDramaticLightBoostDefaults)
        {
            baseLightIntensity = 0.05f;
            maxLightIntensity = 6f;
            intensityChangeSpeed = 2.5f;
            baseLightRange = 0.2f;
            maxLightRange = 3.5f;
            rangeChangeSpeed = 1.2f;
            baseGlowEmission = 0.03f;
            maxGlowEmission = 2.5f;
            glowChangeSpeed = 1.5f;
            lightIncreaseAmount = 0.75f;
            rangeIncreaseAmount = 0.45f;
            glowIncreaseAmount = 0.3f;
        }

        targetLightIntensity = baseLightIntensity;
        targetLightRange = baseLightRange;
        currentGlowEmission = baseGlowEmission;
        targetGlowEmission = baseGlowEmission;

        if (lampLight != null)
        {
            lampLight.type = LightType.Point;
            lampLight.color = lampLightColor;
            lampLight.intensity = baseLightIntensity;
            lampLight.range = baseLightRange;
            lampLight.shadows = LightShadows.Soft;
            Debug.Log($"[PlayerLamp] 시작 밝기 적용: 밝기 {lampLight.intensity:0.00}, 범위 {lampLight.range:0.00}");
        }

        ApplyGlowEmission();
    }

    private void Update()
    {
        if (lampLight == null)
        {
            return;
        }

        float previousIntensity = lampLight.intensity;

        lampLight.intensity = Mathf.MoveTowards(
            lampLight.intensity,
            targetLightIntensity,
            intensityChangeSpeed * Time.deltaTime
        );

        float previousRange = lampLight.range;

        lampLight.range = Mathf.MoveTowards(
            lampLight.range,
            targetLightRange,
            rangeChangeSpeed * Time.deltaTime
        );

        float previousGlowEmission = currentGlowEmission;

        currentGlowEmission = Mathf.MoveTowards(
            currentGlowEmission,
            targetGlowEmission,
            glowChangeSpeed * Time.deltaTime
        );

        if (!Mathf.Approximately(previousGlowEmission, currentGlowEmission))
        {
            ApplyGlowEmission();
        }

        if ((!Mathf.Approximately(previousIntensity, lampLight.intensity) ||
             !Mathf.Approximately(previousRange, lampLight.range) ||
             !Mathf.Approximately(previousGlowEmission, currentGlowEmission)) &&
            Time.time >= nextLightChangeLogTime)
        {
            Debug.Log($"[PlayerLamp] 빛이 밝아지는 중: 밝기 {lampLight.intensity:0.00}/{targetLightIntensity:0.00}, 범위 {lampLight.range:0.00}/{targetLightRange:0.00}, 발광 {currentGlowEmission:0.00}/{targetGlowEmission:0.00}");
            nextLightChangeLogTime = Time.time + lightChangeLogInterval;
        }
    }

    public void IncreaseLight(float amount)
    {
        float previousTargetIntensity = targetLightIntensity;
        float previousTargetRange = targetLightRange;
        float previousTargetGlowEmission = targetGlowEmission;

        targetLightIntensity = Mathf.Clamp(
            targetLightIntensity + Mathf.Max(0f, amount),
            baseLightIntensity,
            maxLightIntensity
        );

        targetLightRange = Mathf.Clamp(
            targetLightRange + Mathf.Max(0f, rangeIncreaseAmount),
            baseLightRange,
            maxLightRange
        );

        targetGlowEmission = Mathf.Clamp(
            targetGlowEmission + Mathf.Max(0f, glowIncreaseAmount),
            baseGlowEmission,
            maxGlowEmission
        );

        Debug.Log($"[PlayerLamp] 목표 빛 증가: 밝기 {previousTargetIntensity:0.00} -> {targetLightIntensity:0.00}, 범위 {previousTargetRange:0.00} -> {targetLightRange:0.00}, 발광 {previousTargetGlowEmission:0.00} -> {targetGlowEmission:0.00}");
    }

    private void OnTriggerEnter(Collider other)
    {
        TryIncreaseLightFromObject(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryIncreaseLightFromObject(collision.gameObject);
    }

    private void TryIncreaseLightFromObject(GameObject touchedObject)
    {
        if (touchedObject == null || !touchedObject.CompareTag(lightBoostObjectTag))
        {
            return;
        }

        Debug.Log($"[PlayerLamp] object 태그 물체에 닿음: {touchedObject.name}");
        IncreaseLight(lightIncreaseAmount);
    }

    private void ApplyGlowEmission()
    {
        if (lampGlowRenderer == null)
        {
            return;
        }

        Material glowMaterial = lampGlowRenderer.material;
        Color emissionColor = lampLightColor * currentGlowEmission;

        glowMaterial.EnableKeyword("_EMISSION");
        glowMaterial.SetColor("_EmissionColor", emissionColor);
    }
}
