using System.Collections;
using UnityEngine;

public class ProjectionActorSequence : MonoBehaviour
{
    [Header("Main Objects")]
    [SerializeField] private GameObject shadowObject;
    [SerializeField] private GameObject activatedObject;
    [SerializeField] private GameObject orbObject;

    [Header("Timing")]
    [SerializeField] private float animationDuration = 1.0f;
    [SerializeField] private float orbSpawnDelay = 0f;

    [Header("After Animation")]
    [SerializeField] private bool hideActivatedObjectAfterAnimation = false;

    [Header("Exit Motion")]
    [SerializeField] private bool playExitMotion = false;
    [SerializeField] private float exitDuration = 0.5f;
    [SerializeField] private Vector3 exitMoveOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Vector3 exitScaleMultiplier = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] private bool fadeOutOnExit = true;

    [Header("Initial State")]
    [SerializeField] private bool disableShadowOnStart = true;
    [SerializeField] private bool disableOrbOnAwake = true;
    [SerializeField] private bool disableActivatedOnAwake = true;

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

        if (disableOrbOnAwake && orbObject != null)
        {
            orbObject.SetActive(false);
        }
    }

    public void PlaySequence()
    {
        if (sequencePlayed) return;
        sequencePlayed = true;

        StartCoroutine(PlaySequenceRoutine());
    }

    private IEnumerator PlaySequenceRoutine()
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

        yield return new WaitForSeconds(animationDuration);

        if (playExitMotion && activatedObject != null)
        {
            yield return StartCoroutine(PlayExitMotionRoutine());
        }
        else if (hideActivatedObjectAfterAnimation && activatedObject != null)
        {
            activatedObject.SetActive(false);
        }

        if (orbSpawnDelay > 0f)
        {
            yield return new WaitForSeconds(orbSpawnDelay);
        }

        if (orbObject != null)
        {
            orbObject.SetActive(true);
        }
    }

    private IEnumerator PlayExitMotionRoutine()
    {
        Vector3 startPos = activatedInitialLocalPosition;
        Vector3 endPos = activatedInitialLocalPosition + exitMoveOffset;

        Vector3 startScale = activatedInitialLocalScale;
        Vector3 endScale = new Vector3(
            activatedInitialLocalScale.x * exitScaleMultiplier.x,
            activatedInitialLocalScale.y * exitScaleMultiplier.y,
            activatedInitialLocalScale.z * exitScaleMultiplier.z
        );

        float time = 0f;

        while (time < exitDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / exitDuration);

            if (activatedObject != null)
            {
                activatedObject.transform.localPosition = Vector3.Lerp(startPos, endPos, t);
                activatedObject.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            }

            if (fadeOutOnExit && activatedRenderers != null)
            {
                for (int i = 0; i < activatedRenderers.Length; i++)
                {
                    if (activatedRenderers[i] == null) continue;

                    Color c = activatedInitialColors[i];
                    c.a = Mathf.Lerp(activatedInitialColors[i].a, 0f, t);
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

        if (orbObject != null)
        {
            orbObject.SetActive(false);
        }
    }
}