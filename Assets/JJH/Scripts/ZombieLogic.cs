using UnityEngine;
using UnityEngine.AI;

public class ZombieLightZoneDetector : MonoBehaviour
{
    public enum ZombieState
    {
        Idle,
        Chasing,
        Fleeing
    }

    [Header("속도 설정")]
    public float fleeSpeed = 4.5f;

    [Header("감지 범위 설정")]
    public float detectionRadius = 10f;

    [Header("플레이어 참조")]
    public Transform playerTransform;
    public PlayerFlashlight flashlight;

    public ZombieState currentState = ZombieState.Idle;

    private float fleeCooldown = 5f;
    private float fleeTimer = 0f;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            Debug.LogError("❌ NavMeshAgent가 없습니다.");
    }

    private void Update()
    {
        if (flashlight == null || playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        bool flashlightOn = flashlight.IsEnabled();
        bool inLightRange = distanceToPlayer <= detectionRadius;

        // ✅ 빛 감지 → 도망
        if (flashlightOn && inLightRange)
        {
            fleeTimer = 0f;

            if (currentState != ZombieState.Fleeing)
            {
                Debug.Log("💡 좀비: 손전등 감지, 도망 시작");
                currentState = ZombieState.Fleeing;
                EnemyManager.Instance.ToggleZombieBehavior(false);
            }

            FleeFromLight(); // ✅ 반드시 호출
        }
        else
        {
            // 도망 중이었음 → 쿨타임
            if (currentState == ZombieState.Fleeing)
            {
                fleeTimer += Time.deltaTime;
                if (fleeTimer >= fleeCooldown)
                {
                    Debug.Log("🧟 좀비: 도망 종료, 추적 시작");
                    currentState = ZombieState.Chasing;
                    EnemyManager.Instance.ToggleZombieBehavior(true);
                    fleeTimer = 0f;
                }
            }
        }

        // 너무 멀어지면 Idle 상태로 전환
        if (distanceToPlayer > detectionRadius + 2f && currentState != ZombieState.Idle)
        {
            Debug.Log("😴 좀비: 플레이어 사라짐, 대기 상태");
            currentState = ZombieState.Idle;
            agent.SetDestination(transform.position); // 정지
        }

        // 추적 상태 처리 (원한다면 별도 이동 로직 가능)
        if (currentState == ZombieState.Chasing)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    private void FleeFromLight()
    {
        Vector3 directionAway = (transform.position - flashlight.GetConeOrigin()).normalized;
        Vector3 fleeTarget = transform.position + directionAway * 5f;

        agent.speed = fleeSpeed;
        agent.SetDestination(fleeTarget);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = currentState == ZombieState.Chasing ? Color.red :
                       currentState == ZombieState.Fleeing ? Color.blue : Color.gray;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
