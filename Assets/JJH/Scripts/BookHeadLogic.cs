using UnityEngine;

public class EnemyLightZoneDetector : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chasing
    }

    [Header("감지 범위 설정")]
    public float detectionRadius = 7f;

    [Header("플레이어 참조")]
    public Transform playerTransform;
    public PlayerFlashlight flashlight;

    public EnemyState currentState = EnemyState.Idle;
    private float loseSightTimer = 0f;
    private float loseSightDelay = 5f;

    private void Update()
    {
        if (flashlight == null || playerTransform == null) return;

        bool flashlightOn = flashlight.IsEnabled();
        bool inRange = Vector3.Distance(transform.position, playerTransform.position) <= detectionRadius;

        if (flashlightOn && inRange)
        {
            loseSightTimer = 0f;
            if (currentState != EnemyState.Chasing)
            {
                Debug.Log("🔵 적 B: 추적 시작");
                currentState = EnemyState.Chasing;
                EnemyManager.Instance.ToggleBookheadBehavior(true);
            }

            loseSightTimer = 0f;
        }
        else
        {
            if (currentState == EnemyState.Chasing)
            {
                loseSightTimer += Time.deltaTime;

                if (loseSightTimer >= loseSightDelay)
                {
                    Debug.Log("🛑 적 B: 추적 중단");
                    EnemyManager.Instance.ToggleBookheadBehavior(false);
                    currentState = EnemyState.Idle;
                    loseSightTimer = 0f;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}