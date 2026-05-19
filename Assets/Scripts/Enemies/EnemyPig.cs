using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyPig : MonoBehaviour
{
    public float moveSpeed = 2f;
    public bool movingRight = true;

    public Transform leftCheck;
    public Transform rightCheck;
    public float wallCheckRadius = 0.05f;
    public LayerMask groundLayer;

    public Transform headPoint;
    public float headCheckRadius = 0.25f;
    public LayerMask playerLayer;
    public float bounceForce = 5f;

    private SpriteRenderer sr;

    public float flipCooldown = 0.15f;

    private Rigidbody2D rb;
    private float flipTimer;

    public float fadeDuration = 1f;
    public float deathDelay = 1f;
    private Animator animator;
    private bool isDead;
    private Collider2D enemyCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr= GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        if (isDead)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        Move();

        if (flipTimer > 0f)
        {
            flipTimer -= Time.fixedDeltaTime;
            return;
        }

        CheckWall();
    }

    private void Move()
    {
        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    private void CheckWall()
    {
        bool hitWall = false;

        if (movingRight)
        {
            hitWall = Physics2D.OverlapCircle(rightCheck.position, wallCheckRadius, groundLayer);
        }
        else if (!movingRight)
        {
            hitWall = Physics2D.OverlapCircle(leftCheck.position, wallCheckRadius, groundLayer);
        }

        if (hitWall)
        {
            Flip();
        }
    }

    private void Flip()
    {
        movingRight = !movingRight;
        flipTimer = flipCooldown;

        sr.flipX = !sr.flipX;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (!collision.collider.CompareTag("Player")) return;

        bool playerOnHead = Physics2D.OverlapCircle(headPoint.position, headCheckRadius, playerLayer);

        if (playerOnHead)
        {
            Rigidbody2D playerRb = collision.collider.GetComponent<Rigidbody2D>();

            if (playerRb != null)
            {
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, bounceForce);
            }

            Die();
        }
        else
        {
            PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage();
            }
        }
    }

    private void Die()
    {
        isDead = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        enemyCollider.enabled = false;

        animator.SetBool("isDead", true);

        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return new WaitForSeconds(deathDelay);

        Color color = sr.color;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = 1f - (elapsed / fadeDuration);
            sr.color = color;
            yield return null;
        }

        Destroy(gameObject);
    }

}