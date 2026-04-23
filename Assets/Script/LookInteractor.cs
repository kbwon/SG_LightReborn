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

        bool hitSomething;
        RaycastHit hit;

        if (useSphereCast && sphereCastRadius > 0f)
        {
            hitSomething = Physics.SphereCast(
                ray,
                sphereCastRadius,
                out hit,
                maxDistance,
                hotspotMask
            );
        }
        else
        {
            hitSomething = Physics.Raycast(
                ray,
                out hit,
                maxDistance,
                hotspotMask
            );
        }

        HotspotTarget newTarget = null;

        if (hitSomething)
        {
            newTarget = hit.collider.GetComponentInParent<HotspotTarget>();
        }

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
                Debug.DrawRay(ray.origin, ray.direction * maxDistance, missColor);
            }

            return;
        }

        bool extraChecksPassed = PassesExtraChecks(currentTarget);

        if (drawDebugRay)
        {
            Debug.DrawRay(
                ray.origin,
                ray.direction * maxDistance,
                extraChecksPassed ? validColor : invalidColor
            );
        }

        if (!extraChecksPassed)
        {
            currentTarget.ResetHold();
            currentProgress = 0f;
            return;
        }

        bool usingLamp = false;

        if (autoInteractWhenClose)
        {
            bool hasLamp = (player == null) || !requireLampEquipped || player.HasLamp;
            usingLamp = extraChecksPassed && hasLamp;
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