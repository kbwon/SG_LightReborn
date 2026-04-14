using UnityEngine;

public class FootstepFlowerSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform spawnOrigin;
    [SerializeField] private PlayerInteractionBridge playerBridge;
    [SerializeField] private GameObject flowerPrefab;
    [SerializeField] private Sprite[] flowerSprites;

    [Header("Spawn Conditions")]
    [SerializeField] private bool requireLampEquipped = true;
    [SerializeField] private bool requireUsingLamp = false;
    [SerializeField] private float moveThreshold = 0.5f;
    [SerializeField] private float rayDistance = 3f;
    [SerializeField] private LayerMask floorMask;

    [Header("Placement")]
    [SerializeField] private float randomOffsetX = 0.15f;
    [SerializeField] private float randomOffsetZ = 0.15f;
    [SerializeField] private float yOffset = 0.01f;
    [SerializeField] private float minScaleMultiplier = 0.8f;
    [SerializeField] private float maxScaleMultiplier = 1.2f;

    private Vector3 lastSpawnPosition;

    private void Start()
    {
        if (spawnOrigin == null)
        {
            spawnOrigin = transform;
        }

        if (playerBridge == null)
        {
            playerBridge = GetComponent<PlayerInteractionBridge>();
        }

        lastSpawnPosition = transform.position;
    }

    private void Update()
    {
        if (flowerPrefab == null) return;
        if (flowerSprites == null || flowerSprites.Length == 0) return;

        if (playerBridge != null)
        {
            if (requireLampEquipped && !playerBridge.HasLamp) return;
            if (requireUsingLamp && !playerBridge.IsUsingLamp) return;
        }

        float moved = Vector3.Distance(transform.position, lastSpawnPosition);
        if (moved < moveThreshold) return;

        Vector3 rayStart = spawnOrigin.position + Vector3.up * 0.2f;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, rayDistance, floorMask))
        {
            Vector3 spawnPos = hit.point;
            Vector3 moveDir = (transform.position - lastSpawnPosition).normalized;
            Vector3 sideDir = Vector3.Cross(Vector3.up, moveDir).normalized;

            float sideOffset = Random.Range(-randomOffsetX, randomOffsetX);
            float forwardOffset = Random.Range(-randomOffsetZ, randomOffsetZ);

            spawnPos += sideDir * sideOffset;
            spawnPos += moveDir * forwardOffset;
            spawnPos.y += yOffset;

            Quaternion rot = Quaternion.Euler(90f, Random.Range(0f, 360f), 0f);
            GameObject flower = Instantiate(flowerPrefab, spawnPos, rot);

            FlowerBloomEffect bloom = flower.GetComponent<FlowerBloomEffect>();
            if (bloom != null)
            {
                Sprite selectedSprite = flowerSprites[Random.Range(0, flowerSprites.Length)];
                float randomScale = Random.Range(minScaleMultiplier, maxScaleMultiplier);
                bloom.Initialize(selectedSprite, randomScale);
            }

            lastSpawnPosition = transform.position;
        }
    }
}