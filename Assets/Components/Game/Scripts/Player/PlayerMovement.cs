using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 15f;
    public float jumpForce = 7f;
    private float gravityScale = 5f;

    [Header("State")]
    public bool isAlive = true;
    public bool isReviving = false;

    [Header("Backgrounds")]
    public GameObject OverworldBackground;
    public GameObject underworldBackground;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D col;
    private Animator animator;
    private bool isGrounded;
    private float moveInput;

    private bool facingRight = false;

    public Transform head;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider2D>();

        rb.freezeRotation = true;

        UpdateBackgrounds();
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (Input.GetButton("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }


        animator.SetBool("isMoving", moveInput != 0);
        animator.SetBool("isGrounded", isGrounded);

        UpdateBackgrounds();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);

        if (head != null)
            head.localRotation = Quaternion.Euler(0f, facingRight ? 0f : 180f, 0f);
    }

    private void UpdateBackgrounds()
    {
        if (isReviving && underworldBackground != null)
        {
            underworldBackground.SetActive(true);
            if (OverworldBackground != null)
                OverworldBackground.SetActive(false);
        }
        else if (isAlive && OverworldBackground != null)
        {
            OverworldBackground.SetActive(true);
            if (underworldBackground != null)
                underworldBackground.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }

    public void PlayWalkSound()
    {
        Debug.Log("PlayWalkSound called");

        if (!isGrounded) return;

        Collider2D tileground = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (tileground == null) return;

        if (tileground.gameObject.name.Contains("Grass"))
        {
            AudioManager.Instance.PlaySound("StepGrass");
        }
        else if (tileground.gameObject.name.Contains("Stone"))
        {
            AudioManager.Instance.PlaySound("StepStone");
        }
        else
        {
            AudioManager.Instance.PlaySound("StepGrass");
        }
    }
}