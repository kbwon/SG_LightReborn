using UnityEngine;

public class EnemyDetector : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chasing
    }

    [Header("시야 설정")]
    public float viewAngle = 90f;
    public float viewRange = 10f;

    [Header("손전등 참조")]
    public Transform playerTransform;
    public Transform playerFlashlightObject;
    private PlayerFlashlight flashlight;

    private float loseSightTimer = 0f;
    private float loseSightDelay = 5f;

    public EnemyState currentState = EnemyState.Idle;

    private void Start()
    {
        flashlight = playerFlashlightObject.GetComponent<PlayerFlashlight>();
        if (flashlight == null)
            Debug.LogWarning("⚠️ PlayerFlashlight 스크립트 찾지 못함");
    }

    private void Update()
    {
        if (flashlight == null || playerTransform == null) return;

        bool seesPlayer = IsPlayerInFOV(); // 조건 1
        bool seesFlashlight = flashlight.IsEnabled() && IsLightConeInFOV(); // 조건 2 (손전등이 켜져 있을 때만)

        if (seesPlayer || seesFlashlight)
        {
            loseSightTimer = 0f;
            // 추적 시작
            if (currentState != EnemyState.Chasing)
            {
                Debug.Log("🎯 적 A 추적 시작!");
                currentState = EnemyState.Chasing;
                EnemyManager.Instance.ToggleDollBehavior(true);
            }

            loseSightTimer = 0f; // 추적 유지 중
        }
        else
        {
            // 감지 안 될 경우 타이머 시작
            if (currentState == EnemyState.Chasing)
            {
                loseSightTimer += Time.deltaTime;

                if (loseSightTimer >= loseSightDelay)
                {
                    Debug.Log("🛑 적 A 추적 중단");
                    EnemyManager.Instance.ToggleDollBehavior(false);
                    currentState = EnemyState.Idle;
                    loseSightTimer = 0f;
                }
            }
        }
    }

    private bool IsPlayerInFOV()
    {
        Vector3 toPlayer = playerTransform.position - transform.position;
        float distToPlayer = toPlayer.magnitude;

        if (distToPlayer > viewRange) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, toPlayer.normalized);
        return angleToPlayer <= viewAngle * 0.5f;
    }

    private bool IsLightConeInFOV()
    {
        Vector3 coneOrigin = flashlight.GetConeOrigin();
        Vector3 coneDir = flashlight.GetConeDirection();
        float coneRange = flashlight.GetConeRange();

        int sampleCount = 10;
        for (int i = 0; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            Vector3 samplePoint = coneOrigin + coneDir * coneRange * t;

            Vector3 toSample = samplePoint - transform.position;
            float dist = toSample.magnitude;
            float angle = Vector3.Angle(transform.forward, toSample.normalized);

            if (dist <= viewRange && angle <= viewAngle * 0.5f)
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 origin = transform.position;
        Vector3 forward = transform.forward;

        float halfAngle = viewAngle * 0.5f;

        Vector3 rightDir = Quaternion.Euler(0, halfAngle, 0) * forward;
        Vector3 leftDir = Quaternion.Euler(0, -halfAngle, 0) * forward;

        Gizmos.DrawLine(origin, origin + rightDir * viewRange);
        Gizmos.DrawLine(origin, origin + leftDir * viewRange);
    }
}