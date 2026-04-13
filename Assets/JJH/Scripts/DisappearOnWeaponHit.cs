using System.Collections;
using UnityEngine;

public class DisappearOnWeaponHit : MonoBehaviour
{
    [Header("맞으면 3초 후 사라질 오브젝트")]
    public GameObject targetObject;

    [Header("맞는 태그")]
    public string weaponTag = "Weapon";

    [Header("충돌 시 재생할 사운드")]
    public AudioClip hitSound;

    [Header("외부 Animator (예: Doll Animator)")]
    public Animator targetAnimator;  // ✅ 외부 Animator 연결

    private AudioSource audioSource;
    private bool isDead = false;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        if (other.CompareTag(weaponTag))
        {
            isDead = true;

            if (hitSound != null)
                audioSource.PlayOneShot(hitSound);

            if (targetAnimator != null)
                targetAnimator.SetBool("isDie", true);  // ✅ Bool로 애니메이션 전이 트리거

            StartCoroutine(RemoveAfterDelay(3f));
        }
    }

    private IEnumerator RemoveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (targetObject != null)
            Destroy(targetObject);
    }
}
