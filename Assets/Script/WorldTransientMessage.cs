using System.Collections;
using TMPro;
using UnityEngine;

public class WorldTransientMessage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text messageText;

    [Header("Message")]
    [SerializeField] private string defaultMessage = "램프의 빛이 강해졌습니다.";

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float holdDuration = 1.8f;
    [SerializeField] private float fadeOutDuration = 0.35f;

    private Coroutine showRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>(true);
        }

        if (messageText == null)
        {
            messageText = GetComponentInChildren<TMP_Text>(true);
        }

        if (messageText != null)
        {
            messageText.text = defaultMessage;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        gameObject.SetActive(false);
    }

    public void Show()
    {
        Show(defaultMessage);
    }

    public void Show(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
        }

        gameObject.SetActive(true);
        showRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float time = 0f;

        canvasGroup.alpha = 0f;

        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeInDuration);
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(holdDuration);

        time = 0f;
        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeOutDuration);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        showRoutine = null;
        gameObject.SetActive(false);
    }
}