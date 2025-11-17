using UnityEngine;

public class UnderworldProjectileSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnRangeY = 30f; 
    [SerializeField] private float spawnDistanceX = 40f;

    [Header("Projectile Settings")]
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private int projectileDamage = 10;

    private float spawnTimer = 0f;

    private void Update()
    {
        if (player == null) return;

        PlayerMovement pm = player.GetComponent<PlayerMovement>();
        if (pm == null || !pm.isReviving) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            SpawnProjectile();
            spawnTimer = 0f;
        }
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile prefab non assigné!");
            return;
        }

        float randomY = player.position.y + Random.Range(-spawnRangeY / 2f, spawnRangeY / 2f);

        bool spawnLeft = Random.value > 0.5f;
        float spawnX = player.position.x + (spawnLeft ? -spawnDistanceX : spawnDistanceX);

        Vector3 spawnPosition = new Vector3(spawnX, randomY, 0);
        Vector2 direction = spawnLeft ? Vector2.right : Vector2.left;

        Quaternion quaternion = spawnLeft ? Quaternion.Euler(0, 0, 47.76f) : Quaternion.Euler(0, 180, 47.76f);

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, quaternion);

       

        UnderworldProjectile projScript = projectile.GetComponent<UnderworldProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(direction);
        }

        Debug.Log($"Projectile spawné à {spawnPosition}, direction: {direction}");
    }
}