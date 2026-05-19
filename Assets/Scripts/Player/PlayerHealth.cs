using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private int maxLives = 3;
    private int currentLives;

    public Transform spawnPoint;

    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    private bool isInvulnerable = false;

    private void Awake()
    {
        currentLives = maxLives;
        playerMovement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        LevelManager.Instance.UpdateHeartsUI(currentLives);
    }

    public void TakeDamage()
    {
        if (isInvulnerable)return;
        currentLives--;
        LevelManager.Instance.UpdateHeartsUI(currentLives);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHurt();
        }

        if (currentLives > 0)
        {
            Respawn();
            StartCoroutine(InvulnerabilityRoutine());
        }
        else
        {
            Die();
        }
    }

    private void Respawn()
    {
        playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;
        transform.position = spawnPoint.position;
        playerMovement.enabled = true;
    }

    private void Die()
    {
        playerMovement.enabled = false;
        rb.linearVelocity = Vector2.zero;
        Time.timeScale = 0f;
        LevelManager.Instance.LoseLevel();
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float elapsed = 0f;

        while (elapsed < 1f)
        {
            elapsed += 0.1f;
            sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(0.1f);
        }
        sr.enabled = true;

        isInvulnerable = false;
    }

}
