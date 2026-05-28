using Photon.Pun;
using System.Collections;
using UnityEngine;

public class StarfyMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;

    public float dashForce = 10f;
    public float dashDuration = 0.35f;
    public float dashCooldown = 1f;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator animator;

    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;

    private Vector3 startScale;
    private int facingDirection = 1;

    private PhotonView photonView;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        photonView = GetComponent<PhotonView>();

        startScale = transform.localScale;
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        CheckGround();

        if (!isDashing)
        {
            Move();
            Jump();
            Flip();
        }

        if (Input.GetKeyDown(KeyCode.F) && canDash)
        {
            StartCoroutine(Dash());
        }

        UpdateAnimations();
    }

    private void Move()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        rb.linearVelocity = new Vector2(
            moveInput * speed,
            rb.linearVelocity.y);
    }

    private void Jump()
    {
        if ((Input.GetKeyDown(KeyCode.W) ||
             Input.GetKeyDown(KeyCode.Space))
            && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce);

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayJump();
            }
        }
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayDash();
        }

        float gravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = Vector2.zero;

        rb.linearVelocity =
            new Vector2(facingDirection * dashForce, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = gravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer);
    }

    private void Flip()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0)
        {
            facingDirection = 1;

            transform.localScale = new Vector3(
                Mathf.Abs(startScale.x),
                startScale.y,
                startScale.z);
        }
        else if (moveInput < 0)
        {
            facingDirection = -1;

            transform.localScale = new Vector3(
                -Mathf.Abs(startScale.x),
                startScale.y,
                startScale.z);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        float moveInput = Input.GetAxisRaw("Horizontal");

        animator.SetBool("IsRunning", moveInput != 0);
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isDash", isDashing);
        animator.SetFloat("YVelocity", rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius);
    }
}
