using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private LayerMask groundLayer;

    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject ZombiePrefab;
    [SerializeField] private GameObject archerEnemyPrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField, Range(0f, 1f)] private float archerSpawnChance = 0.4f; 

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxEnemiesAlive = 10;
    [SerializeField] private float spawnDistanceX = 30f;
    [SerializeField] private float spawnRangeX = 10f;

    [Header("Boss Settings")]
    [SerializeField] private int killsNeededForBoss = 10;
    [SerializeField] private float bossSpawnDistanceX = 40f;

    [Header("Ground Detection")]
    [SerializeField] private float raycastHeight = 50f;
    [SerializeField] private float raycastMaxDistance = 100f;
    [SerializeField] private float spawnHeightOffset = 1f;

    private float spawnTimer = 0f;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool wasReviving = false;
    private int enemiesKilled = 0;
    private bool bossIsAlive = false;
    private GameObject currentBoss;

    private void Start()
    {
        if (playerMovement == null && player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
        }
    }

    private void Update()
    {
        if (player == null || playerMovement == null) return;

        spawnedEnemies.RemoveAll(enemy => enemy == null);

        if (bossIsAlive && currentBoss == null)
        {
            bossIsAlive = false;
            enemiesKilled = 0;
            Debug.Log("Boss vaincu! Compteur remis à zéro.");
        }

        if (playerMovement.isReviving && !wasReviving)
        {
            DestroyAllEnemies();
            wasReviving = true;
            Debug.Log("Mode revival activé - Tous les ennemis détruits");
        }
        else if (!playerMovement.isReviving && wasReviving)
        {
            wasReviving = false;
            Debug.Log("Mode revival terminé - Spawn d'ennemis réactivé");
        }

        if (!playerMovement.isAlive || playerMovement.isReviving)
        {
            return;
        }

        if (enemiesKilled >= killsNeededForBoss && !bossIsAlive)
        {
            SpawnBoss();
            return;
        }

        if (bossIsAlive)
        {
            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && spawnedEnemies.Count < maxEnemiesAlive)
        {
            SpawnRandomEnemy();
            spawnTimer = 0f;
        }
    }

    private void SpawnRandomEnemy()
    {
        GameObject prefabToSpawn;
        EnemyDeathTracker.EnemyType type;

        if (Random.value <= archerSpawnChance)
        {
            prefabToSpawn = archerEnemyPrefab;
            type = EnemyDeathTracker.EnemyType.Archer;
            Debug.Log("Spawn d'un Archer");
        }
        else
        {
            prefabToSpawn = ZombiePrefab;
            type = EnemyDeathTracker.EnemyType.Normal;
            Debug.Log("Spawn d'un Zombie");
        }

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("Prefab manquant!");
            return;
        }

        SpawnEnemyAtPosition(prefabToSpawn, spawnDistanceX, type);
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("Boss prefab non assigné!");
            return;
        }

        Debug.Log("BOSS SPAWN!");
        GameObject boss = SpawnEnemyAtPosition(bossPrefab, bossSpawnDistanceX, EnemyDeathTracker.EnemyType.Boss);

        if (boss != null)
        {
            bossIsAlive = true;
            currentBoss = boss;
        }
    }

    private GameObject SpawnEnemyAtPosition(GameObject prefab, float distanceX, EnemyDeathTracker.EnemyType enemyType = EnemyDeathTracker.EnemyType.Normal)
    {
        float randomOffset = Random.Range(-spawnRangeX / 2f, spawnRangeX / 2f);
        float spawnX = player.position.x + distanceX + randomOffset;

        Vector2 rayStart = new Vector2(spawnX, player.position.y + raycastHeight);
        RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, raycastMaxDistance, groundLayer);

        if (hit.collider != null)
        {
            Vector3 spawnPosition = new Vector3(spawnX, hit.point.y + spawnHeightOffset, 0);

            GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);

            EnemyController normalEnemy = enemy.GetComponent<EnemyController>();
            if (normalEnemy != null)
            {
                normalEnemy.player = player;
            }

            ArcherEnemy archer = enemy.GetComponent<ArcherEnemy>();
            if (archer != null)
            {
                archer.player = player;
            }

            BossController boss = enemy.GetComponent<BossController>();
            if (boss != null)
            {
                boss.player = player;
            }

            EnemyDeathTracker tracker = enemy.AddComponent<EnemyDeathTracker>();
            tracker.spawner = this;
            tracker.enemyType = enemyType;

            spawnedEnemies.Add(enemy);
            Debug.Log($"Ennemi spawné à {spawnPosition}");

            return enemy;
        }
        else
        {
            Debug.LogWarning($"Pas de sol trouvé pour spawn à X={spawnX}");
            return null;
        }
    }

    public void OnEnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"Ennemis tués: {enemiesKilled}/{killsNeededForBoss}");
    }

    private void DestroyAllEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
        spawnTimer = 0f;

        if (currentBoss != null)
        {
            Destroy(currentBoss);
            currentBoss = null;
            bossIsAlive = false;
        }
    }

    private void OnDrawGizmos()
    {
        if (player == null) return;

        Gizmos.color = Color.red;
        Vector3 spawnCenter = player.position + Vector3.right * spawnDistanceX;
        Gizmos.DrawWireSphere(spawnCenter, spawnRangeX / 2f);

        Gizmos.color = Color.magenta;
        Vector3 bossSpawnCenter = player.position + Vector3.right * bossSpawnDistanceX;
        Gizmos.DrawWireSphere(bossSpawnCenter, spawnRangeX / 2f);

        Gizmos.color = Color.yellow;
        Vector3 rayStart = spawnCenter + Vector3.up * raycastHeight;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * raycastMaxDistance);
    }
}