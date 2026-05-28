using Photon.Pun;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 2f;
    public float jumpForce = 4f;

    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    public Transform attackPoint;
    public float attackRadius = 0.4f;
    public LayerMask enemyLayer;
    public float attackDuration = 0.3f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;

    private bool isAttacking;

    private PhotonView photonView;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        photonView = GetComponent<PhotonView>();
    }
    private void Update()
    {
        if (photonView != null && !photonView.IsMine)
            return;

        Move();
        CheckGround();
        Jump();
        UpdateAnimations();
        Flip();
        Attack();
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
        isGrounded =
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer) ||
            IsStandingOnOtherPlayer();
    }

    private bool IsStandingOnOtherPlayer()
    {
        Collider2D ownCollider = GetComponent<Collider2D>();

        if (ownCollider == null || groundCheck == null)
            return false;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            groundCheck.position,
            groundCheckRadius);

        foreach (Collider2D hit in hits)
        {
            if (hit == null || IsOwnCollider(hit) || !IsPlayerCollider(hit))
                continue;

            Bounds ownBounds = ownCollider.bounds;
            Bounds supportBounds = hit.bounds;
            float horizontalOverlap =
                Mathf.Min(ownBounds.max.x, supportBounds.max.x) -
                Mathf.Max(ownBounds.min.x, supportBounds.min.x);

            if (horizontalOverlap <= 0f)
                continue;

            float supportTolerance = Mathf.Max(groundCheckRadius * 2f, 0.05f);

            if (ownBounds.min.y >= supportBounds.max.y - supportTolerance)
                return true;
        }

        return false;
    }

    private bool IsOwnCollider(Collider2D hit)
    {
        GameObject owner = hit.attachedRigidbody != null
            ? hit.attachedRigidbody.gameObject
            : hit.gameObject;

        return owner == gameObject || hit.transform.IsChildOf(transform);
    }

    private bool IsPlayerCollider(Collider2D hit)
    {
        GameObject owner = hit.attachedRigidbody != null
            ? hit.attachedRigidbody.gameObject
            : hit.gameObject;

        return owner.CompareTag("Storm") ||
               owner.CompareTag("Starfy") ||
               hit.CompareTag("Storm") ||
               hit.CompareTag("Starfy");
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

    private void Attack()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isAttacking && isGrounded)
        {
            StartCoroutine(AttackRoutine());
        }
    }
    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        PlayAttackSound();

        Collider2D[] enemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRadius,
            enemyLayer);

        Debug.Log("Найдено врагов: " + enemies.Length);

        foreach (Collider2D enemy in enemies)
        {
            EnemyPig pig = enemy.GetComponent<EnemyPig>();

            if (pig != null)
            {
                pig.RequestDie();
            }

            EnemyKingPig kingPig = enemy.GetComponent<EnemyKingPig>();

            if (kingPig != null)
            {
                kingPig.RequestDie();
            }
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }

    private void PlayAttackSound()
    {
        if (PhotonNetwork.InRoom && photonView != null && photonView.ViewID != 0)
        {
            photonView.RPC(nameof(PlayAttackSoundRPC), RpcTarget.All);
            return;
        }

        PlayAttackSoundRPC();
    }

    [PunRPC]
    private void PlayAttackSoundRPC()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayAttack();
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
        animator.SetBool("isAttacked", isAttacking);
    }
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(
                attackPoint.position,
                attackRadius);
        }
    }

}
