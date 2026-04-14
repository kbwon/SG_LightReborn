using System.Collections;
using UnityEngine;

public class FlowerBloomEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Bloom")]
    [SerializeField] private Vector3 startScale = new Vector3(0.2f, 0.2f, 0.2f);
    [SerializeField] private Vector3 endScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private float bloomDuration = 0.25f;

    [Header("Lifetime")]
    [SerializeField] private float stayDuration = 3f;
    [SerializeField] private float fadeDuration = 0.8f;

    private bool initialized = false;
    private float scaleMultiplier = 1f;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    public void Initialize(Sprite sprite, float randomScaleMultiplier)
    {
        if (spriteRenderer != null && sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }

        scaleMultiplier = randomScaleMultiplier;
        initialized = true;
    }

    private void Start()
    {
        if (!initialized)
        {
            initialized = true;
        }

        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        Vector3 scaledStart = startScale * scaleMultiplier;
        Vector3 scaledEnd = endScale * scaleMultiplier;

        transform.localScale = scaledStart;

        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }

        float time = 0f;
        while (time < bloomDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / bloomDuration);
            transform.localScale = Vector3.Lerp(scaledStart, scaledEnd, t);
            yield return null;
        }

        yield return new WaitForSeconds(stayDuration);

        if (spriteRenderer != null)
        {
            Color startColor = spriteRenderer.color;
            time = 0f;

            while (time < fadeDuration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / fadeDuration);

                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                spriteRenderer.color = c;

                yield return null;
            }
        }

        Destroy(gameObject);
    }
}