using System.Collections;
using UnityEngine;

public class SmallAnimalRunSequence : MonoBehaviour
{
    [Header("Main Objects")]
    [SerializeField] private GameObject shadowObject;
    [SerializeField] private GameObject activatedObject;

    [Header("Timing")]
    [SerializeField] private float startMoveDelay = 0.4f;
    [SerializeField] private float runDuration = 0.8f;

    [Header("Run Motion")]
    [SerializeField] private Vector3 runOffset = new Vector3(-1.5f, 0f, 0f);

    [Header("Fade")]
    [SerializeField] private bool fadeOut = true;
    [SerializeField, Range(0f, 1f)] private float fadeStartNormalized = 0.6f;

    [Header("Initial State")]
    [SerializeField] private bool disableActivatedOnAwake = true;
    [SerializeField] private bool disableShadowOnStart = true;

    private bool sequencePlayed = false;

    private Vector3 activatedInitialLocalPosition;
    private Vector3 activatedInitialLocalScale;
    private SpriteRenderer[] activatedRenderers;
    private Color[] activatedInitialColors;

    private void Awake()
    {
        if (activatedObject != null)
        {
            activatedInitialLocalPosition = activatedObject.transform.localPosition;
            activatedInitialLocalScale = activatedObject.transform.localScale;

            activatedRenderers = activatedObject.GetComponentsInChildren<SpriteRenderer>(true);
            activatedInitialColors = new Color[activatedRenderers.Length];

            for (int i = 0; i < activatedRenderers.Length; i++)
            {
                activatedInitialColors[i] = activatedRenderers[i].color;
            }
        }

        if (disableActivatedOnAwake && activatedObject != null)
        {
            activatedObject.SetActive(false);
        }
    }

    public void PlaySequence()
    {
        if (sequencePlayed) return;
        sequencePlayed = true;

        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        if (disableShadowOnStart && shadowObject != null)
        {
            shadowObject.SetActive(false);
        }

        if (activatedObject != null)
        {
            activatedObject.SetActive(true);
            ResetActivatedVisualState();
        }

        // 1. 먼저 제자리에서 애니메이션 재생
        if (startMoveDelay > 0f)
        {
            yield return new WaitForSeconds(startMoveDelay);
        }

        // 2. 그 다음부터 이동 시작
        Vector3 startPos = activatedInitialLocalPosition;
        Vector3 endPos = activatedInitialLocalPosition + runOffset;

        float time = 0f;

        while (time < runDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / runDuration);

            if (activatedObject != null)
            {
                activatedObject.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            }

            if (fadeOut && activatedRenderers != null)
            {
                float fadeT = 0f;

                if (t >= fadeStartNormalized)
                {
                    float fadeRange = Mathf.Max(0.0001f, 1f - fadeStartNormalized);
                    fadeT = (t - fadeStartNormalized) / fadeRange;
                }

                for (int i = 0; i < activatedRenderers.Length; i++)
                {
                    if (activatedRenderers[i] == null) continue;

                    Color c = activatedInitialColors[i];
                    c.a = Mathf.Lerp(activatedInitialColors[i].a, 0f, fadeT);
                    activatedRenderers[i].color = c;
                }
            }

            yield return null;
        }

        if (activatedObject != null)
        {
            activatedObject.SetActive(false);
        }
    }

    private void ResetActivatedVisualState()
    {
        if (activatedObject != null)
        {
            activatedObject.transform.localPosition = activatedInitialLocalPosition;
            activatedObject.transform.localScale = activatedInitialLocalScale;
        }

        if (activatedRenderers != null)
        {
            for (int i = 0; i < activatedRenderers.Length; i++)
            {
                if (activatedRenderers[i] == null) continue;
                activatedRenderers[i].color = activatedInitialColors[i];
            }
        }
    }

    public void ResetSequence()
    {
        StopAllCoroutines();
        sequencePlayed = false;

        if (shadowObject != null)
        {
            shadowObject.SetActive(true);
        }

        if (activatedObject != null)
        {
            activatedObject.SetActive(false);
            activatedObject.transform.localPosition = activatedInitialLocalPosition;
            activatedObject.transform.localScale = activatedInitialLocalScale;
        }

        if (activatedRenderers != null)
        {
            for (int i = 0; i < activatedRenderers.Length; i++)
            {
                if (activatedRenderers[i] == null) continue;
                activatedRenderers[i].color = activatedInitialColors[i];
            }
        }
    }
}