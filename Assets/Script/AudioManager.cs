using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSourceA;
    [SerializeField] private AudioSource bgmSourceB;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip bgmDarkRuin;
    [SerializeField] private AudioClip bgmBrightForest;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip sfxRevive;
    [SerializeField] private AudioClip sfxOrbAbsorb;
    [SerializeField] private AudioClip sfxFinalTransfer;

    [Header("BGM Volumes")]
    [SerializeField] private float darkRuinVolume = 0.42f;
    [SerializeField] private float brightForestVolume = 0.24f;
    [SerializeField] private float bgmFadeDuration = 1.2f;

    [Header("SFX Volume")]
    [SerializeField] private float sfxVolume = 0.65f;

    private AudioSource currentBgmSource;
    private AudioSource nextBgmSource;
    private float targetBgmVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentBgmSource = bgmSourceA;
        nextBgmSource = bgmSourceB;

        if (bgmSourceA != null)
        {
            bgmSourceA.playOnAwake = false;
            bgmSourceA.loop = true;
            bgmSourceA.volume = 0f;
        }

        if (bgmSourceB != null)
        {
            bgmSourceB.playOnAwake = false;
            bgmSourceB.loop = true;
            bgmSourceB.volume = 0f;
        }

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
        }
    }

    public void PlayDarkRuinBGM()
    {
        targetBgmVolume = darkRuinVolume;
        CrossfadeTo(bgmDarkRuin);
    }

    public void PlayBrightForestBGM()
    {
        targetBgmVolume = brightForestVolume;
        CrossfadeTo(bgmBrightForest);
    }

    public void PlayReviveSFX()
    {
        PlaySFX(sfxRevive);
    }

    public void PlayOrbAbsorbSFX()
    {
        PlaySFX(sfxOrbAbsorb);
    }

    public void PlayFinalTransferSFX()
    {
        PlaySFX(sfxFinalTransfer);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    private void CrossfadeTo(AudioClip newClip)
    {
        if (newClip == null || currentBgmSource == null || nextBgmSource == null)
        {
            return;
        }

        if (currentBgmSource.clip == newClip && currentBgmSource.isPlaying)
        {
            return;
        }

        StopAllCoroutines();
        StartCoroutine(CrossfadeRoutine(newClip));
    }

    private IEnumerator CrossfadeRoutine(AudioClip newClip)
    {
        nextBgmSource.clip = newClip;
        nextBgmSource.volume = 0f;
        nextBgmSource.Play();

        float time = 0f;
        float startVolume = currentBgmSource != null ? currentBgmSource.volume : 0f;

        while (time < bgmFadeDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / bgmFadeDuration);

            if (currentBgmSource != null)
            {
                currentBgmSource.volume = Mathf.Lerp(startVolume, 0f, t);
            }

            if (nextBgmSource != null)
            {
                nextBgmSource.volume = Mathf.Lerp(0f, targetBgmVolume, t);
            }

            yield return null;
        }

        if (currentBgmSource != null)
        {
            currentBgmSource.Stop();
            currentBgmSource.volume = 0f;
        }

        if (nextBgmSource != null)
        {
            nextBgmSource.volume = targetBgmVolume;
        }

        AudioSource temp = currentBgmSource;
        currentBgmSource = nextBgmSource;
        nextBgmSource = temp;
    }
}