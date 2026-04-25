using UnityEngine;
using UnityEngine.UI;

public class WorldHoldGauge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HotspotTarget target;
    [SerializeField] private LookInteractor lookInteractor;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fillImage;

    [Header("Display")]
    [SerializeField] private bool onlyShowWhenTargeted = true;
    [SerializeField] private float fadeSpeed = 8f;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        }

        if (fillImage == null)
        {
            fillImage = GetComponentInChildren<Image>(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = 0f;
        }
    }

    private void Update()
    {
        if (target == null || canvasGroup == null || fillImage == null)
        {
            return;
        }

        bool isTargeted = lookInteractor != null && lookInteractor.CurrentTarget == target;

        bool shouldShow =
            target.Interactable &&
            !target.Activated &&
            (!onlyShowWhenTargeted || isTargeted);

        float targetAlpha = shouldShow ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        fillImage.fillAmount = target.HoldProgressNormalized;
    }
}