using System;
using System.Collections.Generic;
using UnityEngine;

public class LookInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private FlowManager flowManager;
    [SerializeField] private PlayerInteractionBridge player;

    [Header("Cast")]
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask hotspotMask = ~0;
    [SerializeField] private bool useSphereCast = true;
    [SerializeField] private float sphereCastRadius = 0.35f;

    [Header("Distance Check")]
    [SerializeField] private bool useDistanceCheck = true;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private Transform distanceOrigin;

    [Header("Optional View Check")]
    [SerializeField] private bool useViewAngleCheck = false;
    [SerializeField, Range(0f, 180f)] private float maxOffCenterAngle = 60f;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;
    [SerializeField] private Color validColor = Color.green;
    [SerializeField] private Color invalidColor = Color.yellow;
    [SerializeField] private Color missColor = Color.red;

    [Header("Runtime")]
    [SerializeField] private HotspotTarget currentTarget;
    [SerializeField] private float currentProgress = 0f;

    [Header("Auto Interaction")]
    [SerializeField] private bool autoInteractWhenClose = true;
    [SerializeField] private bool requireLampEquipped = true;

    public HotspotTarget CurrentTarget => currentTarget;
    public float CurrentProgress => currentProgress;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponentInChildren<Camera>();
        }

        if (flowManager == null)
        {
            flowManager = FlowManager.Instance;
        }

        if (player == null)
        {
            player = GetComponent<PlayerInteractionBridge>();
        }
    }

    private void Update()
    {
        if (targetCamera == null) return;

        Ray ray = new Ray(targetCamera.transform.position, targetCamera.transform.forward);

        bool hadAnyHit;
        bool hadValidCandidate;
        HotspotTarget newTarget = FindBestTarget(ray, out hadAnyHit, out hadValidCandidate);

        if (newTarget != currentTarget)
        {
            if (currentTarget != null)
            {
                currentTarget.ResetHold();
            }

            currentTarget = newTarget;
            currentProgress = 0f;
        }

        if (currentTarget == null)
        {
            currentProgress = 0f;

            if (drawDebugRay)
            {
                Color debugColor = hadAnyHit ? invalidColor : missColor;
                Debug.DrawRay(ray.origin, ray.direction * maxDistance, debugColor);
            }

            return;
        }

        if (drawDebugRay)
        {
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, validColor);
        }

        bool usingLamp = false;

        if (autoInteractWhenClose)
        {
            bool hasLamp = (player == null) || !requireLampEquipped || player.HasLamp;
            usingLamp = hasLamp;
        }
        else
        {
            usingLamp = (player != null && player.IsUsingLamp);
        }

        currentProgress = currentTarget.TickHold(
            Time.deltaTime,
            usingLamp,
            flowManager,
            player
        );

        if (!currentTarget.CanInteract(flowManager, player))
        {
            currentProgress = 0f;
        }
    }

    private HotspotTarget FindBestTarget(Ray ray, out bool hadAnyHit, out bool hadValidCandidate)
    {
        hadAnyHit = false;
        hadValidCandidate = false;

        RaycastHit[] hits = useSphereCast && sphereCastRadius > 0f
            ? Physics.SphereCastAll(ray, sphereCastRadius, maxDistance, hotspotMask)
            : Physics.RaycastAll(ray, maxDistance, hotspotMask);

        if (hits == null || hits.Length == 0)
        {
            return null;
        }

        hadAnyHit = true;

        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        HashSet<HotspotTarget> seenTargets = new HashSet<HotspotTarget>();

        HotspotTarget bestInteractable = null;
        float bestInteractableScore = float.NegativeInfinity;

        HotspotTarget bestFallback = null;
        float bestFallbackScore = float.NegativeInfinity;

        foreach (RaycastHit hit in hits)
        {
            HotspotTarget target = hit.collider.GetComponentInParent<HotspotTarget>();
            if (target == null) continue;
            if (!seenTargets.Add(target)) continue;

            if (!PassesExtraChecks(target))
            {
                continue;
            }

            hadValidCandidate = true;

            bool canInteract = target.CanInteract(flowManager, player);

            float score = -hit.distance;

            if (target == currentTarget)
            {
                score += 0.2f;
            }

            if (canInteract)
            {
                if (score > bestInteractableScore)
                {
                    bestInteractableScore = score;
                    bestInteractable = target;
                }
            }
            else
            {
                if (score > bestFallbackScore)
                {
                    bestFallbackScore = score;
                    bestFallback = target;
                }
            }
        }

        return bestInteractable != null ? bestInteractable : bestFallback;
    }

    private bool PassesExtraChecks(HotspotTarget target)
    {
        if (target == null) return false;

        Vector3 targetPoint = target.transform.position;

        if (useDistanceCheck)
        {
            Transform origin = distanceOrigin != null ? distanceOrigin : targetCamera.transform;
            float distance = Vector3.Distance(origin.position, targetPoint);

            if (distance > interactionDistance)
            {
                return false;
            }
        }

        if (useViewAngleCheck)
        {
            Vector3 toTarget = targetPoint - targetCamera.transform.position;
            toTarget.y = 0f;

            Vector3 forward = targetCamera.transform.forward;
            forward.y = 0f;

            if (toTarget.sqrMagnitude > 0.0001f && forward.sqrMagnitude > 0.0001f)
            {
                float angle = Vector3.Angle(forward.normalized, toTarget.normalized);

                if (angle > maxOffCenterAngle)
                {
                    return false;
                }
            }
        }

        return true;
    }
}