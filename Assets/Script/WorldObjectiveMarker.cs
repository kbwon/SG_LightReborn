using TMPro;
using UnityEngine;

public class WorldObjectiveMarker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HotspotTarget target;
    [SerializeField] private GameObject sourceVisibleObject;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text markerText;

    [Header("Text")]
    [SerializeField] private string message = "빛을 비추세요";

    [Header("Fade")]
    [SerializeField] private float fadeSpeed = 6f;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        }

        if (markerText == null)
        {
            markerText = GetComponentInChildren<TMP_Text>(true);
        }

        if (markerText != null)
        {
            markerText.text = message;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    private void Update()
    {
        bool shouldShow =
            target != null &&
            target.Interactable &&
            !target.Activated &&
            (sourceVisibleObject == null || sourceVisibleObject.activeInHierarchy);

        if (canvasGroup == null) return;

        float targetAlpha = shouldShow ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }

    public void SetMessage(string text)
    {
        message = text;

        if (markerText != null)
        {
            markerText.text = message;
        }
    }
}