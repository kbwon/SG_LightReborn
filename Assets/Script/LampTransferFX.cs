using System.Collections;
using UnityEngine;

public class LampTransferFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform sourceOrigin;
    [SerializeField] private ParticleSystem beamPS;
    [SerializeField] private ParticleSystem arrivalPS;

    [Header("Beam Settings")]
    [SerializeField] private float beamSpeed = 5f;
    [SerializeField] private float beamDuration = 1.0f;
    [SerializeField] private float beamLifetimePadding = 0.05f;

    private Coroutine playRoutine;

    private void Awake()
    {
        StopNow();
    }

    public void PlayToTarget(Transform target)
    {
        if (target == null || sourceOrigin == null || beamPS == null) return;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        playRoutine = StartCoroutine(PlayRoutine(target));
    }

    private IEnumerator PlayRoutine(Transform target)
    {
        Vector3 start = sourceOrigin.position;
        Vector3 end = target.position;
        Vector3 dir = end - start;

        if (dir.sqrMagnitude > 0.0001f)
        {
            transform.position = start;
            transform.rotation = Quaternion.LookRotation(dir.normalized);
        }
        else
        {
            transform.position = start;
        }

        float distance = Vector3.Distance(start, end);

        var main = beamPS.main;
        main.startSpeed = beamSpeed;
        main.startLifetime = Mathf.Max(0.05f, distance / beamSpeed + beamLifetimePadding);

        beamPS.Play(true);

        if (arrivalPS != null)
        {
            arrivalPS.transform.position = end;
        }

        yield return new WaitForSeconds(beamDuration * 0.7f);

        if (arrivalPS != null)
        {
            arrivalPS.Play(true);
        }

        yield return new WaitForSeconds(beamDuration * 0.3f);

        beamPS.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        playRoutine = null;
    }

    public void StopNow()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        if (beamPS != null)
        {
            beamPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (arrivalPS != null)
        {
            arrivalPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}