using System.Collections;
using TMPro;
using UnityEngine;

public class WorldObjectiveMarker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HotspotTarget target;
    [SerializeField] private GameObject sourceVisibleObject;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text markerText;

    [Header("Default Text")]
    [SerializeField] private string activeMessage = "빛을 비추세요";
    [SerializeField] private bool showWhenLocked = false;
    [SerializeField] private string lockedMessage = "더 밝은 빛이 필요합니다";

    [Header("Success Message")]
    [SerializeField] private bool showSuccessMessageOnActivated = false;
    [SerializeField] private string successMessage = "빛이 강해졌습니다";
    [SerializeField] private float successMessageDuration = 1.2f;

    [Header("Behavior")]
    [SerializeField] private bool hideWhenActivated = true;
    [SerializeField] private float fadeSpeed = 6f;

    private bool lastActivatedState = false;
    private bool forceShowTemporary = false;
    private bool keepCurrentTextWhileHiding = false;
    private Coroutine temporaryRoutine;

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

        UpdateTextImmediate();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (target != null)
        {
            lastActivatedState = target.Activated;
        }
    }

    private void Update()
    {
        if (target == null || canvasGroup == null)
        {
            return;
        }

        if (!lastActivatedState && target.Activated)
        {
            OnTargetActivated();
        }

        lastActivatedState = target.Activated;

        bool sourceVisible = sourceVisibleObject == null || sourceVisibleObject.activeInHierarchy;
        bool interactable = target.Interactable;
        bool activated = target.Activated;

        bool shouldShow = false;

        if (forceShowTemporary)
        {
            shouldShow = true;
        }
        else
        {
            if (hideWhenActivated && activated)
            {
                shouldShow = false;
            }
            else
            {
                if (interactable)
                {
                    shouldShow = sourceVisible;
                }
                else
                {
                    shouldShow = sourceVisible && showWhenLocked;
                }
            }

            // 성공 메시지 후 숨겨지는 동안에는 기존 텍스트를 유지한다.
            if (!keepCurrentTextWhileHiding)
            {
                UpdateTextImmediate();
            }
        }

        float targetAlpha = shouldShow ? 1f : 0f;
        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // 완전히 사라진 뒤에만 다시 기본 텍스트 갱신 허용
        if (!forceShowTemporary && keepCurrentTextWhileHiding && canvasGroup.alpha <= 0.001f)
        {
            keepCurrentTextWhileHiding = false;
            UpdateTextImmediate();
        }
    }

    private void OnTargetActivated()
    {
        if (!showSuccessMessageOnActivated)
        {
            return;
        }

        if (temporaryRoutine != null)
        {
            StopCoroutine(temporaryRoutine);
        }

        temporaryRoutine = StartCoroutine(ShowTemporarySuccessRoutine());
    }

    private IEnumerator ShowTemporarySuccessRoutine()
    {
        forceShowTemporary = true;
        keepCurrentTextWhileHiding = false;

        if (markerText != null)
        {
            markerText.text = successMessage;
        }

        yield return new WaitForSeconds(successMessageDuration);

        forceShowTemporary = false;

        // 활성화 후 사라질 대상이면 성공 메시지 텍스트를 유지한 채 fade out
        if (target != null && hideWhenActivated && target.Activated)
        {
            keepCurrentTextWhileHiding = true;
        }
        else
        {
            keepCurrentTextWhileHiding = false;
            UpdateTextImmediate();
        }

        temporaryRoutine = null;
    }

    private void UpdateTextImmediate()
    {
        if (markerText == null || target == null)
        {
            return;
        }

        markerText.text = target.Interactable ? activeMessage : lockedMessage;
    }

    public void SetActiveMessage(string text)
    {
        activeMessage = text;
        if (!forceShowTemporary && !keepCurrentTextWhileHiding)
        {
            UpdateTextImmediate();
        }
    }

    public void SetLockedMessage(string text)
    {
        lockedMessage = text;
        if (!forceShowTemporary && !keepCurrentTextWhileHiding)
        {
            UpdateTextImmediate();
        }
    }

    public void SetSuccessMessage(string text)
    {
        successMessage = text;
    }

    public void ShowTemporaryMessage(string text)
    {
        if (temporaryRoutine != null)
        {
            StopCoroutine(temporaryRoutine);
        }

        successMessage = text;
        temporaryRoutine = StartCoroutine(ShowTemporarySuccessRoutine());
    }
}