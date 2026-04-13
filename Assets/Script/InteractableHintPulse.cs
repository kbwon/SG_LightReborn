using UnityEngine;

public class InteractableHintPulse : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HotspotTarget target;
    [SerializeField] private LookInteractor lookInteractor;
    [SerializeField] private GameObject sourceVisibleObject;
    [SerializeField] private SpriteRenderer[] hintRenderers;

    [Header("Pulse")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minAlpha = 0.05f;
    [SerializeField] private float maxAlpha = 0.18f;
    [SerializeField] private float minScaleMultiplier = 1.0f;
    [SerializeField] private float maxScaleMultiplier = 1.08f;

    [Header("Targeted Boost")]
    [SerializeField] private bool boostWhenTargeted = true;
    [SerializeField] private float targetedAlphaMultiplier = 1.4f;
    [SerializeField] private float targetedScaleMultiplier = 1.05f;

    private Vector3 baseLocalScale;
    private Color[] baseColors;

    private void Awake()
    {
        baseLocalScale = transform.localScale;

        if (hintRenderers == null || hintRenderers.Length == 0)
        {
            hintRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        baseColors = new Color[hintRenderers.Length];
        for (int i = 0; i < hintRenderers.Length; i++)
        {
            if (hintRenderers[i] != null)
            {
                baseColors[i] = hintRenderers[i].color;
            }
        }

        SetVisible(false);
        ApplyAlpha(0f);
    }

    private void Update()
    {
        bool shouldShow =
            target != null &&
            target.Interactable &&
            !target.Activated &&
            (sourceVisibleObject == null || sourceVisibleObject.activeInHierarchy);

        if (!shouldShow)
        {
            SetVisible(false);
            ApplyAlpha(0f);
            transform.localScale = baseLocalScale;
            return;
        }

        SetVisible(true);

        float pulse = (Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f;

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);
        float scaleMultiplier = Mathf.Lerp(minScaleMultiplier, maxScaleMultiplier, pulse);

        if (boostWhenTargeted && lookInteractor != null && lookInteractor.CurrentTarget == target)
        {
            alpha *= targetedAlphaMultiplier;
            scaleMultiplier *= targetedScaleMultiplier;
        }

        alpha = Mathf.Clamp01(alpha);

        ApplyAlpha(alpha);
        transform.localScale = baseLocalScale * scaleMultiplier;
    }

    private void ApplyAlpha(float alpha)
    {
        if (hintRenderers == null) return;

        for (int i = 0; i < hintRenderers.Length; i++)
        {
            if (hintRenderers[i] == null) continue;

            Color c = baseColors[i];
            c.a = alpha;
            hintRenderers[i].color = c;
        }
    }

    private void SetVisible(bool visible)
    {
        if (hintRenderers == null) return;

        for (int i = 0; i < hintRenderers.Length; i++)
        {
            if (hintRenderers[i] == null) continue;
            hintRenderers[i].enabled = visible;
        }
    }
}