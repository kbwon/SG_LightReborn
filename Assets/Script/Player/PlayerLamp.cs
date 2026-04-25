using UnityEngine;

public class PlayerLamp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light lampLight;
    [SerializeField] private Renderer lampGlowRenderer;

    [Header("Light Color")]
    [SerializeField] private Color lampLightColor = new Color(1f, 0.58f, 0.22f);

    [Header("Base / Max")]
    [SerializeField] private float baseLightIntensity = 0.22f;
    [SerializeField] private float maxLightIntensity = 4.2f;
    [SerializeField] private float intensityChangeSpeed = 3f;

    [SerializeField] private float baseLightRange = 1.25f;
    [SerializeField] private float maxLightRange = 5.2f;
    [SerializeField] private float rangeChangeSpeed = 2.4f;

    [SerializeField] private float baseGlowEmission = 0.15f;
    [SerializeField] private float maxGlowEmission = 2.8f;
    [SerializeField] private float glowChangeSpeed = 3f;

    [Header("Progress Curves")]
    [SerializeField] private AnimationCurve intensityByProgress = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve rangeByProgress = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve glowByProgress = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Optional Physical Boost")]
    [SerializeField] private bool allowPhysicalBoostFromTags = false;
    [SerializeField] private string lightBoostObjectTag = "object";
    [SerializeField] private float fallbackAmountForFullCharge = 12f;

    [Header("Legacy / Debug")]
    [SerializeField] private float lightChangeLogInterval = 0.25f;
    [SerializeField] private bool useDramaticLightBoostDefaults = false;

    private float visualProgress01;
    private float targetLightIntensity;
    private float targetLightRange;
    private float currentGlowEmission;
    private float targetGlowEmission;
    private float nextLightChangeLogTime;

    public float VisualProgress01 => visualProgress01;

    private void Awake()
    {
        if (lampLight == null)
        {
            lampLight = GetComponentInChildren<Light>();
        }

        if (useDramaticLightBoostDefaults)
        {
            baseLightIntensity = 0.08f;
            maxLightIntensity = 3.8f;
            intensityChangeSpeed = 2.6f;

            baseLightRange = 1.0f;
            maxLightRange = 4.2f;
            rangeChangeSpeed = 2.0f;

            baseGlowEmission = 0.08f;
            maxGlowEmission = 2.2f;
            glowChangeSpeed = 2.4f;
        }

        if (lampLight != null)
        {
            lampLight.type = LightType.Point;
            lampLight.color = lampLightColor;
            lampLight.shadows = LightShadows.Soft;
        }

        currentGlowEmission = baseGlowEmission;
        ApplyVisualProgress01(0f, true);
    }

    private void Update()
    {
        if (lampLight == null)
        {
            return;
        }

        float previousIntensity = lampLight.intensity;
        float previousRange = lampLight.range;
        float previousGlowEmission = currentGlowEmission;

        lampLight.intensity = Mathf.MoveTowards(
            lampLight.intensity,
            targetLightIntensity,
            intensityChangeSpeed * Time.deltaTime
        );

        lampLight.range = Mathf.MoveTowards(
            lampLight.range,
            targetLightRange,
            rangeChangeSpeed * Time.deltaTime
        );

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
            Debug.Log(
                $"[PlayerLamp] 진행도 {visualProgress01:0.00} | 밝기 {lampLight.intensity:0.00}/{targetLightIntensity:0.00}, " +
                $"범위 {lampLight.range:0.00}/{targetLightRange:0.00}, 발광 {currentGlowEmission:0.00}/{targetGlowEmission:0.00}"
            );

            nextLightChangeLogTime = Time.time + lightChangeLogInterval;
        }
    }

    public void SetVisualProgress01(float normalizedProgress)
    {
        ApplyVisualProgress01(normalizedProgress, false);
    }

    public void ResetVisuals()
    {
        ApplyVisualProgress01(0f, true);
    }

    public void IncreaseLight(float amount)
    {
        // 예전 구조와 호환용: 물리 충돌/구형 트리거를 쓰는 경우에만 fallback으로 사용
        if (fallbackAmountForFullCharge <= 0.0001f)
        {
            fallbackAmountForFullCharge = 12f;
        }

        float deltaNormalized = Mathf.Max(0f, amount) / fallbackAmountForFullCharge;
        ApplyVisualProgress01(visualProgress01 + deltaNormalized, false);
    }

    public void ForceLampOff()
    {
        if (lampLight != null)
        {
            lampLight.enabled = false;
        }

        if (lampGlowRenderer != null)
        {
            Material glowMaterial = lampGlowRenderer.material;
            glowMaterial.EnableKeyword("_EMISSION");
            glowMaterial.SetColor("_EmissionColor", Color.black);
        }
    }

    public void ForceLampOn()
    {
        if (lampLight != null)
        {
            lampLight.enabled = true;
        }

        ApplyGlowEmission();
    }

    private void ApplyVisualProgress01(float normalizedProgress, bool applyImmediately)
    {
        visualProgress01 = Mathf.Clamp01(normalizedProgress);

        float intensityT = Mathf.Clamp01(intensityByProgress.Evaluate(visualProgress01));
        float rangeT = Mathf.Clamp01(rangeByProgress.Evaluate(visualProgress01));
        float glowT = Mathf.Clamp01(glowByProgress.Evaluate(visualProgress01));

        targetLightIntensity = Mathf.Lerp(baseLightIntensity, maxLightIntensity, intensityT);
        targetLightRange = Mathf.Lerp(baseLightRange, maxLightRange, rangeT);
        targetGlowEmission = Mathf.Lerp(baseGlowEmission, maxGlowEmission, glowT);

        if (applyImmediately)
        {
            if (lampLight != null)
            {
                lampLight.intensity = targetLightIntensity;
                lampLight.range = targetLightRange;
                lampLight.enabled = true;

                Debug.Log(
                    $"[PlayerLamp] 시작 밝기 적용: 진행도 {visualProgress01:0.00}, " +
                    $"밝기 {lampLight.intensity:0.00}, 범위 {lampLight.range:0.00}"
                );
            }

            currentGlowEmission = targetGlowEmission;
            ApplyGlowEmission();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!allowPhysicalBoostFromTags) return;
        TryIncreaseLightFromObject(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!allowPhysicalBoostFromTags) return;
        TryIncreaseLightFromObject(collision.gameObject);
    }

    private void TryIncreaseLightFromObject(GameObject touchedObject)
    {
        if (touchedObject == null || !touchedObject.CompareTag(lightBoostObjectTag))
        {
            return;
        }

        Debug.Log($"[PlayerLamp] object 태그 물체에 닿음: {touchedObject.name}");
        IncreaseLight(1f);
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