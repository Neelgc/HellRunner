using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private float underworldYOffset = -64f;
    [SerializeField] private int destructionRadius = 1;
    [SerializeField] private LayerMask tileLayer;

    [Header("Revival Target")]
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private float targetYPosition = 5f; 
    [SerializeField] private float targetDetectionRadius = 2f;

    private ChunkManager chunkManager;
    private GameObject currentTarget;

    private void Start()
    {
        chunkManager = FindObjectOfType<ChunkManager>();
    }

    private void Update()
    {
        if (currentTarget != null)
        {
            Vector3 targetPos = currentTarget.transform.position;
            targetPos.x = transform.position.x;
            currentTarget.transform.position = targetPos;

            float distance = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (distance <= targetDetectionRadius)
            {
                ReachTarget();
            }
        }
    }

    public void RespawnInUnderworld()
    {
        Vector3 currentPos = transform.position;
        Vector3 underworldPos = new Vector3(currentPos.x, underworldYOffset, currentPos.z);

        transform.position = underworldPos;

        DestroyBlocksAround(underworldPos);

        CreateRevivalTarget(currentPos.x);

        Debug.Log($"Player respawned in underworld at {underworldPos}");
    }

    private void CreateRevivalTarget(float playerX)
    {
        if (currentTarget != null)
        {
            Destroy(currentTarget);
        }

        if (targetPrefab != null)
        {
            Vector3 targetPosition = new Vector3(playerX, targetYPosition, 0);
            currentTarget = Instantiate(targetPrefab, targetPosition, Quaternion.identity);
            currentTarget.name = "RevivalTarget";
        }
        else
        {
            currentTarget = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            currentTarget.transform.position = new Vector3(playerX, targetYPosition, 0);
            currentTarget.transform.localScale = Vector3.one * 2f;
            currentTarget.name = "RevivalTarget";

            Renderer renderer = currentTarget.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.cyan;
            }

            Destroy(currentTarget.GetComponent<Collider>());
        }
    }

    private void ReachTarget()
    {
        Debug.Log("Revival target reached!");

        UnderworldProjectile[] projectiles = FindObjectsOfType<UnderworldProjectile>();
        foreach (var proj in projectiles)
        {
            Destroy(proj.gameObject);
        }

        if (currentTarget != null)
        {
            Destroy(currentTarget);
            currentTarget = null;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnPlayerRevived();
        }

        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.CompleteRevival();
        }
    }

    private void DestroyBlocksAround(Vector3 centerPos)
    {
        for (int x = -destructionRadius; x <= destructionRadius; x++)
        {
            for (int y = -destructionRadius; y <= destructionRadius; y++)
            {
                Vector2 checkPos = new Vector2(centerPos.x + x, centerPos.y + y);
                Collider2D[] colliders = Physics2D.OverlapCircleAll(checkPos, 0.4f, tileLayer);

                foreach (Collider2D col in colliders)
                {
                    Destroy(col.gameObject);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (currentTarget != null)
        {
            Destroy(currentTarget);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (currentTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentTarget.transform.position, targetDetectionRadius);
        }
    }
}