using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ArcherEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float jumpForce = 7f;
    public LayerMask groundLayer;

    [Header("Distance Behavior")]
    public float preferredDistance = 10f;
    public float tooCloseDistance = 5f;
    public float maxDistance = 20f;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootCooldown = 2f;
    public float projectileSpeed = 8f;
    public int projectileDamage = 1;

    [Header("Check Points")]
    public Transform groundCheck;
    public Transform wallCheck;

    private Rigidbody2D rb;
    private Animator animator;

    private bool isGrounded;
    private bool facingRight = false;
    private float nextShootTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        UpdateGroundCheck();

        if (distanceToPlayer < tooCloseDistance)
        {
            MoveAwayFromPlayer();
        }
        else if (distanceToPlayer > maxDistance)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            FacePlayer();
        }

        if (distanceToPlayer <= maxDistance && Time.time >= nextShootTime)
        {
            Shoot();
        }
    }

    private void MoveTowardsPlayer()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
            Flip();

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (IsTouchingWall() && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void MoveAwayFromPlayer()
    {
        float direction = -Mathf.Sign(player.position.x - transform.position.x);

        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
            Flip();

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (IsTouchingWall() && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void FacePlayer()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
            Flip();
    }

    private void Shoot()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Projectile prefab ou firePoint non assigné!");
            return;
        }

        nextShootTime = Time.time + shootCooldown;

        Vector2 direction = (player.position - firePoint.position).normalized;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.Euler(0, 0, (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg)));

        AudioManager.Instance.PlaySound("CrossbowFire");

        EnemyProjectile projScript = projectile.GetComponent<EnemyProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(direction, projectileSpeed, projectileDamage);
        }

        Debug.Log("Archer tire un projectile!");
    }

    private void UpdateGroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    private bool IsTouchingWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, tooCloseDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, maxDistance);

        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(wallCheck.position, 0.1f);
        }
    }
}