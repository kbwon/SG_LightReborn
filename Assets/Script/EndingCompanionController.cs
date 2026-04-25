using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EndingCompanionEntry
{
    public string rescuedTargetId;
    public bool alwaysShowInEnding = false;
    public GameObject rootObject;
}

public class EndingCompanionController : MonoBehaviour
{
    [SerializeField] private FlowManager flowManager;
    [SerializeField] private EndingCompanionEntry[] companions;

    private readonly Dictionary<GameObject, Color[]> cachedColors = new Dictionary<GameObject, Color[]>();
    private readonly Dictionary<GameObject, SpriteRenderer[]> cachedRenderers = new Dictionary<GameObject, SpriteRenderer[]>();
    private Coroutine fadeRoutine;

    private void Awake()
    {
        CacheRenderers();
        HideAllImmediate();
    }

    public void ShowForEnding(float fadeDuration)
    {
        if (flowManager == null)
        {
            flowManager = FlowManager.Instance;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeVisibleCompanionsRoutine(true, fadeDuration));
    }

    public void FadeOutVisibleCompanions(float fadeDuration)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeVisibleCompanionsRoutine(false, fadeDuration));
    }

    public void HideAllImmediate()
    {
        if (companions == null) return;

        foreach (EndingCompanionEntry entry in companions)
        {
            if (entry == null || entry.rootObject == null) continue;

            SpriteRenderer[] renderers = GetRenderers(entry.rootObject);
            Color[] colors = GetBaseColors(entry.rootObject);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;

                Color c = colors[i];
                c.a = 0f;
                renderers[i].color = c;
            }

            entry.rootObject.SetActive(false);
        }
    }

    private IEnumerator FadeVisibleCompanionsRoutine(bool fadeIn, float duration)
    {
        List<EndingCompanionEntry> activeEntries = new List<EndingCompanionEntry>();

        foreach (EndingCompanionEntry entry in companions)
        {
            if (entry == null || entry.rootObject == null) continue;

            bool shouldShow = entry.alwaysShowInEnding;

            if (!shouldShow && flowManager != null && !string.IsNullOrEmpty(entry.rescuedTargetId))
            {
                shouldShow = flowManager.HasRescuedSmallAnimal(entry.rescuedTargetId);
            }

            if (shouldShow)
            {
                entry.rootObject.SetActive(true);
                activeEntries.Add(entry);
            }
            else if (fadeIn)
            {
                entry.rootObject.SetActive(false);
            }
        }

        if (activeEntries.Count == 0)
        {
            yield break;
        }

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float alpha = fadeIn ? t : 1f - t;

            foreach (EndingCompanionEntry entry in activeEntries)
            {
                SpriteRenderer[] renderers = GetRenderers(entry.rootObject);
                Color[] colors = GetBaseColors(entry.rootObject);

                for (int i = 0; i < renderers.Length; i++)
                {
                    if (renderers[i] == null) continue;

                    Color c = colors[i];
                    c.a = colors[i].a * alpha;
                    renderers[i].color = c;
                }
            }

            yield return null;
        }

        if (!fadeIn)
        {
            foreach (EndingCompanionEntry entry in activeEntries)
            {
                if (entry != null && entry.rootObject != null)
                {
                    entry.rootObject.SetActive(false);
                }
            }
        }

        fadeRoutine = null;
    }

    private void CacheRenderers()
    {
        if (companions == null) return;

        foreach (EndingCompanionEntry entry in companions)
        {
            if (entry == null || entry.rootObject == null) continue;

            if (cachedRenderers.ContainsKey(entry.rootObject)) continue;

            SpriteRenderer[] renderers = entry.rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            cachedRenderers.Add(entry.rootObject, renderers);

            Color[] colors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                colors[i] = renderers[i] != null ? renderers[i].color : Color.white;
            }

            cachedColors.Add(entry.rootObject, colors);
        }
    }

    private SpriteRenderer[] GetRenderers(GameObject rootObject)
    {
        if (!cachedRenderers.ContainsKey(rootObject))
        {
            SpriteRenderer[] renderers = rootObject.GetComponentsInChildren<SpriteRenderer>(true);
            cachedRenderers[rootObject] = renderers;

            Color[] colors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                colors[i] = renderers[i] != null ? renderers[i].color : Color.white;
            }

            cachedColors[rootObject] = colors;
        }

        return cachedRenderers[rootObject];
    }

    private Color[] GetBaseColors(GameObject rootObject)
    {
        return cachedColors[rootObject];
    }
}