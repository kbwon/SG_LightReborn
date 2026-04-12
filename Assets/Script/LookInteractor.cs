using UnityEngine;

public class LookInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private FlowManager flowManager;
    [SerializeField] private SimpleFirstPersonTestPlayer player;

    [Header("Raycast")]
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask hotspotMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool drawDebugRay = true;
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.red;

    [Header("Runtime")]
    [SerializeField] private HotspotTarget currentTarget;
    [SerializeField] private float currentProgress = 0f;

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
            player = GetComponent<SimpleFirstPersonTestPlayer>();
        }
    }

    private void Update()
    {
        if (targetCamera == null) return;

        Ray ray = new Ray(targetCamera.transform.position, targetCamera.transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, maxDistance, hotspotMask);

        if (drawDebugRay)
        {
            Debug.DrawRay(
                ray.origin,
                ray.direction * maxDistance,
                hitSomething ? hitColor : missColor
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
            return;
        }

        bool usingLamp = (player != null && player.IsUsingLamp);

        currentProgress = currentTarget.TickHold(
            Time.deltaTime,
            usingLamp,
            flowManager,
            player
        );

        // 이미 다른 조건 때문에 상호작용이 안 되는 상태면 진행도는 0으로 보이게
        if (!currentTarget.CanInteract(flowManager, player))
        {
            currentProgress = 0f;
        }
    }
}