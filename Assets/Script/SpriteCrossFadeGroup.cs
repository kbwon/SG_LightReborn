using System.Collections;
using UnityEngine;

public class SpriteCrossFadeGroup : MonoBehaviour
{
    [Header("Fade Out Group")]
    [SerializeField] private SpriteRenderer[] fadeOutRenderers;

    [Header("Fade In Group")]
    [SerializeField] private SpriteRenderer[] fadeInRenderers;

    [Header("Timing")]
    [SerializeField] private float fadeDuration = 1.0f;

    [Header("State")]
    [SerializeField] private bool setFadeInAlphaToZeroOnAwake = true;
    [SerializeField] private bool setFadeOutAlphaToOneOnAwake = true;

    private bool hasPlayed = false;

    private void Awake()
    {
        if (setFadeOutAlphaToOneOnAwake)
        {
            SetAlpha(fadeOutRenderers, 1f);
        }

        if (setFadeInAlphaToZeroOnAwake)
        {
            SetAlpha(fadeInRenderers, 0f);
        }

        SetObjectsActive(fadeOutRenderers, true);
        SetObjectsActive(fadeInRenderers, true);
    }

    public void PlayCrossFade()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        StopAllCoroutines();
        StartCoroutine(CrossFadeRoutine());
    }

    private IEnumerator CrossFadeRoutine()
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);

            SetAlpha(fadeOutRenderers, 1f - t);
            SetAlpha(fadeInRenderers, t);

            yield return null;
        }

        SetAlpha(fadeOutRenderers, 0f);
        SetAlpha(fadeInRenderers, 1f);
    }

    public void ResetFade()
    {
        StopAllCoroutines();
        hasPlayed = false;

        SetAlpha(fadeOutRenderers, 1f);
        SetAlpha(fadeInRenderers, 0f);

        SetObjectsActive(fadeOutRenderers, true);
        SetObjectsActive(fadeInRenderers, true);
    }

    private void SetAlpha(SpriteRenderer[] renderers, float alpha)
    {
        if (renderers == null) return;

        foreach (SpriteRenderer sr in renderers)
        {
            if (sr == null) continue;

            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

    private void SetObjectsActive(SpriteRenderer[] renderers, bool value)
    {
        if (renderers == null) return;

        foreach (SpriteRenderer sr in renderers)
        {
            if (sr == null) continue;
            sr.gameObject.SetActive(value);
        }
    }
}