using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;
    public AudioClip attackSound;
    public float soundDelay = 0.5f;
    private AudioSource audioSource;

    [Header("I key toggles UI")]
    public GameObject uiToToggle;

    private bool warnedMissingUI;

    private void Start()
    {
        LockCursor();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        bool isUIOpen = uiToToggle != null && uiToToggle.activeSelf;

        if (Input.GetKeyDown(KeyCode.I))
        {
            if (uiToToggle != null)
            {
                isUIOpen = uiToToggle.activeSelf;
                uiToToggle.SetActive(!isUIOpen);

                if (isUIOpen)
                {
                    LockCursor();
                }
                else
                {
                    UnlockCursor();
                }
            }
            else if (!warnedMissingUI)
            {
                warnedMissingUI = true;
                Debug.LogWarning("PlayerAttack.uiToToggle is not assigned. Assign it in Inspector to use I-key UI toggle.");
            }
        }

        if (uiToToggle != null && uiToToggle.activeSelf)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (animator != null)
            {
                animator.SetTrigger("attackTrigger");
            }

            StartCoroutine(PlaySoundAfterDelay(soundDelay));
        }
    }

    private IEnumerator PlaySoundAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound);
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
