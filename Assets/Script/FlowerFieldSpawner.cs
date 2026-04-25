using System.Collections;
using UnityEngine;

public class FlowerFieldSpawner : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private BoxCollider spawnArea;
    [SerializeField] private LayerMask floorMask;
    [SerializeField] private float rayStartHeight = 5f;
    [SerializeField] private float yOffset = 0.01f;

    [Header("Flower")]
    [SerializeField] private GameObject flowerPrefab;
    [SerializeField] private Sprite[] flowerSprites;
    [SerializeField] private int spawnCount = 60;
    [SerializeField] private float spawnInterval = 0.03f;
    [SerializeField] private float minScaleMultiplier = 0.8f;
    [SerializeField] private float maxScaleMultiplier = 1.3f;

    private bool hasSpawned = false;

    public void BloomAll()
    {
        if (hasSpawned) return;
        hasSpawned = true;
        StartCoroutine(BloomRoutine());
    }

    private IEnumerator BloomRoutine()
    {
        if (spawnArea == null || flowerPrefab == null || flowerSprites == null || flowerSprites.Length == 0)
            yield break;

        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                bounds.max.y + rayStartHeight,
                Random.Range(bounds.min.z, bounds.max.z)
            );

            if (Physics.Raycast(randomPoint, Vector3.down, out RaycastHit hit, rayStartHeight * 2f, floorMask))
            {
                Vector3 spawnPos = hit.point;
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
            }

            if (spawnInterval > 0f)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }
    }
}