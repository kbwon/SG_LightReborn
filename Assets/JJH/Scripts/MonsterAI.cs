using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MonsterAI : MonoBehaviour
{
    public enum MonsterType
    {
        Doll,
        BookheadMonster,
        Zombie
    }

    [Header("몬스터 타입 설정")]
    public MonsterType monsterType = MonsterType.BookheadMonster;

    [Header("각 타입별 추적/공격 허용 여부")]
    public bool dollCanChaseAndAttack = false;
    public bool bookheadCanChaseAndAttack = true;
    public bool zombieCanChaseAndAttack = true;

    [Header("공통 설정")]
    public Transform player;
    public float chaseDistance = 8f;
    public float attackDistance = 2f;
    public float wanderRadius = 10f;
    public float wanderTimer = 5f;
    public float attackDuration = 1.2f;

    private NavMeshAgent agent;
    private Animator animator;
    private float timer;
    private bool isAttacking = false;

    [Header("공격 시 UI")]
    public GameObject scareUI;
    public float scareUIDistance = 1.5f;
    public AudioClip scareSound;
    private bool uiTriggered = false;

    [Header("추적 시작 시 사운드")]
    public AudioClip chaseSound;
    private AudioSource audioSource;
    private bool hasPlayedChaseSound = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        timer = wanderTimer;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (scareUI != null)
            scareUI.SetActive(false);
    }

    void Update()
    {
        bool isEnabled = (monsterType == MonsterType.Doll)
            ? dollCanChaseAndAttack
            : (monsterType == MonsterType.BookheadMonster)
                ? bookheadCanChaseAndAttack
                : zombieCanChaseAndAttack; // ✅ Zombie 추가

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (isEnabled && distanceToPlayer <= attackDistance && !isAttacking)
        {
            agent.SetDestination(transform.position);
            animator.speed = 1f;
            SetAnimation(false, true);
            StartCoroutine(EndAttackAfter(attackDuration));
        }
        else if (isEnabled && distanceToPlayer <= chaseDistance && !isAttacking)
        {
            agent.SetDestination(player.position);
            animator.speed = 3f;
            SetAnimation(true, false);

            if (!hasPlayedChaseSound && chaseSound != null)
            {
                audioSource.PlayOneShot(chaseSound);
                hasPlayedChaseSound = true;
            }
        }
        else
        {
            if (!isAttacking)
            {
                timer += Time.deltaTime;
                if (timer >= wanderTimer)
                {
                    Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                    agent.SetDestination(newPos);
                    timer = 0f;
                }

                bool isMoving = agent.velocity.magnitude > 0.1f;
                animator.speed = 1f;
                SetAnimation(isMoving, false);
            }

            hasPlayedChaseSound = false;
        }

        // 😱 Scare UI 표시
        if (!uiTriggered && isEnabled && distanceToPlayer <= scareUIDistance && distanceToPlayer <= attackDistance)
        {
            if (scareUI != null)
            {
                Debug.Log("✅ 깜짝 UI 표시");
                scareUI.SetActive(true);
                uiTriggered = true;

                if (scareSound != null)
                    audioSource.PlayOneShot(scareSound);

                Invoke("HideScareUI", 1.5f);
            }
        }
    }

    void SetAnimation(bool isWalking, bool isAttacking)
    {
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isAttacking", isAttacking);
    }

    IEnumerator EndAttackAfter(float seconds)
    {
        isAttacking = true;
        yield return new WaitForSeconds(seconds);
        animator.SetBool("isAttacking", false);
        isAttacking = false;
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    public void SetChaseAndAttackEnabled(bool enabled)
    {
        switch (monsterType)
        {
            case MonsterType.Doll:
                dollCanChaseAndAttack = enabled;
                break;
            case MonsterType.BookheadMonster:
                bookheadCanChaseAndAttack = enabled;
                break;
            case MonsterType.Zombie:
                zombieCanChaseAndAttack = enabled;
                break;
        }
    }

    public void EnableChaseAndAttack() => SetChaseAndAttackEnabled(true);
    public void DisableChaseAndAttack() => SetChaseAndAttackEnabled(false);

    public bool IsChasingPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        bool isEnabled = (monsterType == MonsterType.Doll)
            ? dollCanChaseAndAttack
            : (monsterType == MonsterType.BookheadMonster)
                ? bookheadCanChaseAndAttack
                : zombieCanChaseAndAttack;

        return isEnabled && distanceToPlayer <= chaseDistance && distanceToPlayer > attackDistance;
    }

    private void HideScareUI()
    {
        if (scareUI != null)
            scareUI.SetActive(false);

        uiTriggered = false;
    }
}
