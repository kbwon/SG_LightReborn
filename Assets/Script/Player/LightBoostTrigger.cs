using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LightBoostTrigger : MonoBehaviour
{
    [SerializeField] private string targetPlayerTag = "Player";
    [SerializeField] private float boostAmount = 0.5f;
    [SerializeField] private bool consumeOnTouch = false;

    private void Reset()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(targetPlayerTag) && !other.CompareTag(targetPlayerTag))
        {
            return;
        }

        PlayerLamp playerLamp = other.GetComponentInParent<PlayerLamp>();
        if (playerLamp == null)
        {
            return;
        }

        playerLamp.IncreaseLight(boostAmount);
        Debug.Log("빛이 증가했습니다");

        if (consumeOnTouch)
        {
            Destroy(gameObject);
        }
    }
}
