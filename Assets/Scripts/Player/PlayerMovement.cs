using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        Move();
        CheckGround();
        Jump();
        UpdateAnimations();
        Flip();
    }

    private void Move()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        if ((Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayJump();
            }
        }
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Flip()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0)
        {
            transform.localScale = new Vector3(3f, 3f, 3f);
        }
        else if (moveInput < 0)
        {
            transform.localScale = new Vector3(-3f, 3f, 3f);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        float moveInput = Input.GetAxisRaw("Horizontal");

        animator.SetBool("IsRunning", moveInput != 0);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

}
