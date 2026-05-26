using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyKingPig : MonoBehaviour
{
    private SpriteRenderer sr;

    public float fadeDuration = 1f;
    public float deathDelay = 1f;
    private Animator animator;
    private bool isDead;
    private Collider2D enemyCollider;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (!collision.collider.CompareTag("Storm") && !collision.collider.CompareTag("Starfy")) return;
        PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage();
        }
    }
    public void Die()
    {
        isDead = true;

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
