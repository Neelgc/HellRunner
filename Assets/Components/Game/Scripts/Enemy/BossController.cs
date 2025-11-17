using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BossController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Boss Stats")]
    public int maxHealth = 100;
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float detectionRange = 150f;
    public LayerMask groundLayer;

    [Header("Combat")]
    public float attackRange = 2f;
    public float attackCooldown = 0.8f;
    public int damage = 3;

    [Header("Check Points")]
    public Transform groundCheck;
    public Transform wallCheck;

    [Header("Visual")]
    public Vector3 bossScale = new Vector3(2f, 2f, 1f);

    private Rigidbody2D rb;
    private Animator animator;
    private int currentHealth;

    private bool isGrounded;
    private bool isAttacking;
    private bool facingRight = false;
    private float nextAttackTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;

        currentHealth = maxHealth;
        transform.localScale = bossScale;
    }

    private void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            MoveTowardsPlayer();
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            Attack();
        }

        UpdateGroundCheck();
    }

    private void MoveTowardsPlayer()
    {
        if (isAttacking) return;

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
            Flip();

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (IsTouchingWall() && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void Attack()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        nextAttackTime = Time.time + attackCooldown;

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            Health health = player.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);
        }

        Invoke(nameof(ResetAttack), 0.5f);
    }

    private void ResetAttack()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log($"Boss health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Boss defeated!");
        Destroy(gameObject);
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
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
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