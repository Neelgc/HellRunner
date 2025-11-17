using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float detectionRange = 100f;
    public LayerMask groundLayer;

    [Header("Combat")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.0f;
    public int damage = 1;

    [Header("Check Points")]
    public Transform groundCheck;
    public Transform wallCheck;

    private Rigidbody2D rb;
    private Animator animator;

    private bool isGrounded;
    private bool isAttacking;
    private bool facingRight = false;
    private float nextAttackTime = 0f;

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

        // Vérifie si le joueur est proche
        if (distanceToPlayer <= detectionRange)
        {
            MoveTowardsPlayer();
            //Debug.Log("Moving Towards Player");
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            //Debug.Log("Idle");
            //animator?.SetBool("isMoving", false);
        }

        // Vérifie si on peut attaquer
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            Attack();
            //Debug.Log("Attacking Player");
        }

        UpdateGroundCheck();
    }

    private void MoveTowardsPlayer()
    {
        if (isAttacking) return;

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        // Flip si nécessaire
        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
            Flip();

        // Déplacement horizontal
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        //animator?.SetBool("isMoving", true);

        // Si un mur bloque le chemin -> saute
        if (IsTouchingWall() && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void Attack()
    {
        isAttacking = true;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // stop pendant attaque
        //animator?.SetTrigger("attack");

        nextAttackTime = Time.time + attackCooldown;

        // Appliquer les dégâts au joueur si à portée
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            Health health = player.GetComponent<Health>();
            if (health != null)
                health.TakeDamage(damage);
        }

        // L'attaque dure un petit temps (simulation)
        Invoke(nameof(ResetAttack), 0.4f);
    }

    private void ResetAttack()
    {
        isAttacking = false;
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(wallCheck.position, 0.1f);
    }
}
