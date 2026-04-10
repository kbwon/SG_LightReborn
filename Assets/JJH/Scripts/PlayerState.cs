using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerState : MonoBehaviour
{
    private bool hasCrowbar = false;
    private bool hasKey = false;
    private bool isGameOverTriggered = false;

    [Header("Escape UI")]
    public GameObject escapeUI;

    [Header("Enemy refs")]
    public GameObject enemyA;
    public GameObject enemyB;

    private void Update()
    {
        CheckHiddenEndingTrigger();
        CheckDeadEndingTrigger();
    }

    public bool HiddenEndingCase()
    {
        return GameManager.Instance != null && GameManager.Instance.letterCount >= 5;
    }

    public void ObtainCrowbar()
    {
        hasCrowbar = true;
        Debug.Log("Crowbar obtained");
    }

    public void ObtainKey()
    {
        hasKey = true;
        Debug.Log("Key obtained");
    }

    public bool HasKey() => hasKey;

    public void CheckEscapeTrigger()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPhase(GamePhase.GameOver);
        }

        SceneManager.LoadScene("EscapeScene");
    }

    private void CheckHiddenEndingTrigger()
    {
        if (isGameOverTriggered)
        {
            return;
        }

        if (HiddenEndingCase() && enemyA == null && enemyB == null)
        {
            isGameOverTriggered = true;
            Debug.Log("Hidden ending condition met");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPhase(GamePhase.GameOver);
            }

            SceneManager.LoadScene("HiddenEndingScene");
        }
    }

    private void CheckDeadEndingTrigger()
    {
        CheckOneEnemy(enemyA);
        CheckOneEnemy(enemyB);
    }

    private void CheckOneEnemy(GameObject enemy)
    {
        if (enemy == null || isGameOverTriggered)
        {
            return;
        }

        var ai = enemy.GetComponent<MonsterAI>();
        if (ai != null && ai.IsChasingPlayer())
        {
            float dist = Vector3.Distance(transform.position, ai.transform.position);
            if (dist <= 5f)
            {
                isGameOverTriggered = true;
                Debug.Log("Enemy too close - DeadEnding");
                StartCoroutine(TriggerGameOverAfterDelay());
            }
        }
    }

    private IEnumerator TriggerGameOverAfterDelay()
    {
        yield return new WaitForSeconds(3f);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPhase(GamePhase.GameOver);
        }

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadDeadEndingScene();
        }
        else
        {
            SceneManager.LoadScene("DeadEndingScene");
        }
    }
}
