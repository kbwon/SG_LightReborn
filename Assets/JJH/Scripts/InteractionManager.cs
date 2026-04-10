using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum InteractableType
{
    Letter,
    Crowbar,
    Key,
    EscapeDoor,
    Door   
}

public class InteractionManager : MonoBehaviour
{
    public InteractableType interactableType;

    [Header("공통 상호작용 UI")]
    public GameObject interactionUI;

    [Header("문 관련")]
    public GameObject doorToOpen;  // 문 오브젝트
    private bool isDoorOpening = false;

    [Header("문 열림 이후 처리")]
    public GameObject doorOpenUI;             // 문이 열리면 보여줄 UI
    public AudioClip doorOpenSound;           // 문 열릴 때 나는 소리
    public GameObject objectToDisappear;      // 사라질 오브젝트
    public float disappearDelay = 2f;         // 몇 초 뒤에 사라질지

    [Header("Letter 관련")]
    //public GameObject[] letterDetails; // detail1 ~ detail5

    [Header("아이템 UI")]
    public GameObject crowbarUI;
    public GameObject keyUI;
    public GameObject dontEscapeUI;

    [Header("사운드")]
    public AudioClip lockSound;
    public AudioClip unlockSound;

    private bool isPlayerNear = false;
    private bool isUsed = false;
    private PlayerState playerState;

    private void Start()
    {
        interactionUI?.SetActive(false);
        playerState = GameObject.FindWithTag("Player")?.GetComponent<PlayerState>();
    }

    private void Update()
    {
        if (!isPlayerNear || isUsed) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            switch (interactableType)
            {
                case InteractableType.Letter:
                    HandleLetterInteraction();
                    break;

                case InteractableType.Crowbar:
                    HandleCrowbarInteraction();
                    break;

                case InteractableType.Key:
                    HandleKeyInteraction();
                    break;

                case InteractableType.EscapeDoor:
                    HandleEscapeInteraction();
                    break;
                case InteractableType.Door:   
                    HandleDoorInteraction();
                    break;
            }
        }
    }

    private void HandleLetterInteraction()
    {
        int letterCount = GameManager.Instance.GetLetterCount();

        if (letterCount < GameManager.Instance.letterDetails.Length)
        {
            GameManager.Instance.CollectLetter(); // GameManager가 수집 처리
            Debug.Log($"📩 편지 {letterCount + 1} 획득");

            isUsed = true; // 재상호작용 방지
            interactionUI?.SetActive(false);
            Destroy(gameObject); // 편지 오브젝트 제거
        }
    }

    private void HandleDoorInteraction()
    {
        if (doorToOpen != null && !isDoorOpening)
        {
            StartCoroutine(RotateDoorSmoothly(doorToOpen, -150f, 1f)); // 1초 동안 회전
            Debug.Log("🚪 문 열리는 중...");
            isUsed = true;
            interactionUI?.SetActive(false);
        }
        else
        {
            Debug.LogWarning("⚠️ doorToOpen이 설정되지 않았거나 이미 열림.");
        }
    }

    private IEnumerator RotateDoorSmoothly(GameObject door, float yAngle, float duration)
    {
        isDoorOpening = true;

        Quaternion startRotation = door.transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, yAngle, 0);

        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            door.transform.rotation = Quaternion.Slerp(startRotation, endRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        door.transform.rotation = endRotation;
        Debug.Log("✅ 문 열림 완료");

        // ✅ 추가: UI 표시
        if (doorOpenUI != null)
        {
            yield return new WaitForSeconds(1);
            doorOpenUI.SetActive(true);
        }

        // ✅ 추가: 소리 재생
        if (doorOpenSound != null)
        {
            AudioSource.PlayClipAtPoint(doorOpenSound, door.transform.position);
        }

        if (objectToDisappear != null)
        {
            bool wasInitiallyActive = objectToDisappear.activeSelf; // 현재 상태 저장

            yield return new WaitForSeconds(disappearDelay);

            objectToDisappear.SetActive(!wasInitiallyActive); // 반대로 설정
            if (doorOpenUI != null)
            {
                doorOpenUI.SetActive(false);
            }

            Debug.Log($"🧨 오브젝트 {(wasInitiallyActive ? "비활성화" : "활성화")} 완료");
        }

        isDoorOpening = false;
    }



    private void HandleCrowbarInteraction()
    {
        if (crowbarUI != null)
            crowbarUI.SetActive(true);

        playerState.ObtainCrowbar();
        Debug.Log("🔧 Crowbar 획득");
        isUsed = true;
        interactionUI?.SetActive(false);
        Destroy(gameObject);
    }

    private void HandleKeyInteraction()
    {
        if (keyUI != null)
            keyUI.SetActive(true);

        playerState.ObtainKey();
        Debug.Log("🗝️ 열쇠 획득");
        isUsed = true;
        interactionUI?.SetActive(false);
        Destroy(gameObject);
    }

    private void HandleEscapeInteraction()
    {
        if (playerState.HasKey())
        {
            Debug.Log("🔓 탈출 시도: 열쇠 있음");
            PlaySound(unlockSound);
            interactionUI?.SetActive(false);
            playerState.CheckEscapeTrigger(); // 탈출 시도
        }
        else
        {
            Debug.Log("🔒 탈출 시도: 열쇠 없음");
            PlaySound(lockSound);
            if (dontEscapeUI != null)
                dontEscapeUI.SetActive(true);
            interactionUI?.SetActive(false);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
            AudioSource.PlayClipAtPoint(clip, transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            interactionUI?.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            interactionUI?.SetActive(false);
        }
    }
}
