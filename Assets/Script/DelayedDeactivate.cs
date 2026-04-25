using System.Collections;
using UnityEngine;

public class DelayedDeactivate : MonoBehaviour
{
    [SerializeField] private GameObject targetObject;
    [SerializeField] private float delay = 0.8f;

    public void DeactivateAfterDelay()
    {
        StartCoroutine(DeactivateRoutine());
    }

    private IEnumerator DeactivateRoutine()
    {
        yield return new WaitForSeconds(delay);

        if (targetObject != null)
        {
            targetObject.SetActive(false);
        }
    }
}