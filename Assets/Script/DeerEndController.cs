using System.Collections;
using UnityEngine;

public class DeerEndController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private FlowManager flowManager;
    [SerializeField] private HotspotTarget hotspotTarget;

    [Header("Deer Objects")]
    [SerializeField] private GameObject deerReturn;
    [SerializeField] private GameObject deerShadowIdle;
    [SerializeField] private GameObject deerHintGlow;
    [SerializeField] private GameObject deerTransform;
    [SerializeField] private Transform deerTargetPoint;

    [Header("Lamp / FX")]
    [SerializeField] private LampTransferFX lampTransferFX;
    [SerializeField] private GameObject lampObjectToTurnOff;

    [Header("Timing")]
    [SerializeField] private float returnDuration = 1.5f;
    [SerializeField] private float transformTotalDuration = 1.8f;
    [SerializeField] private float finalFadeDuration = 0.8f;

    private bool returnStarted = false;
    private bool finalSequenceStarted = false;

    private void Awake()
    {
        SetInitialState();
    }

    public void BeginReturnSequence()
    {
        if (returnStarted) return;
        returnStarted = true;

        StopAllCoroutines();
        StartCoroutine(ReturnSequenceRoutine());
    }

    public void BeginFinalSequence()
    {
        if (finalSequenceStarted) return;
        finalSequenceStarted = true;

        StopAllCoroutines();
        StartCoroutine(FinalSequenceRoutine());
    }

    private IEnumerator ReturnSequenceRoutine()
    {
        SetObjectActive(deerReturn, true);
        SetObjectActive(deerShadowIdle, false);
        SetObjectActive(deerHintGlow, false);
        SetObjectActive(deerTransform, false);

        if (hotspotTarget != null)
        {
            hotspotTarget.SetInteractable(false);
        }

        yield return new WaitForSeconds(returnDuration);

        SetObjectActive(deerReturn, false);
        SetObjectActive(deerShadowIdle, true);

        if (hotspotTarget != null)
        {
            hotspotTarget.SetInteractable(true);
        }

        SetObjectActive(deerHintGlow, true);
    }

    private IEnumerator FinalSequenceRoutine()
    {
        if (hotspotTarget != null)
        {
            hotspotTarget.SetInteractable(false);
        }

        SetObjectActive(deerHintGlow, false);
        SetObjectActive(deerShadowIdle, false);

        // 빛 전달 효과 + 사슴 변신/고개 들기 동시 시작
        if (lampTransferFX != null && deerTargetPoint != null)
        {
            lampTransferFX.PlayToTarget(deerTargetPoint);
        }

        SetObjectActive(deerTransform, true);

        yield return new WaitForSeconds(transformTotalDuration);

        if (lampTransferFX != null)
        {
            lampTransferFX.StopNow();
        }

        yield return StartCoroutine(FadeOutObjectRoutine(deerTransform, finalFadeDuration));

        SetObjectActive(deerTransform, false);

        if (lampObjectToTurnOff != null)
        {
            lampObjectToTurnOff.SetActive(false);
        }

        if (flowManager != null)
        {
            flowManager.CompleteEnding();
        }
    }

    private IEnumerator FadeOutObjectRoutine(GameObject target, float duration)
    {
        if (target == null) yield break;

        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers == null || renderers.Length == 0) yield break;

        Color[] initialColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            initialColors[i] = renderers[i].color;
        }

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;

                Color c = initialColors[i];
                c.a = Mathf.Lerp(initialColors[i].a, 0f, t);
                renderers[i].color = c;
            }

            yield return null;
        }
    }

    public void ResetSequence()
    {
        StopAllCoroutines();

        returnStarted = false;
        finalSequenceStarted = false;

        SetInitialState();

        if (deerTransform != null)
        {
            ResetSpriteAlpha(deerTransform);
        }

        if (lampTransferFX != null)
        {
            lampTransferFX.StopNow();
        }
    }

    private void SetInitialState()
    {
        SetObjectActive(deerReturn, false);
        SetObjectActive(deerShadowIdle, false);
        SetObjectActive(deerHintGlow, false);
        SetObjectActive(deerTransform, false);

        if (hotspotTarget != null)
        {
            hotspotTarget.SetInteractable(false);
        }
    }

    private void SetObjectActive(GameObject obj, bool value)
    {
        if (obj != null)
        {
            obj.SetActive(value);
        }
    }

    private void ResetSpriteAlpha(GameObject target)
    {
        if (target == null) return;

        SpriteRenderer[] renderers = target.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (SpriteRenderer sr in renderers)
        {
            if (sr == null) continue;

            Color c = sr.color;
            c.a = 1f;
            sr.color = c;
        }
    }
}