using System.Collections;
using Photon.Pun;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private int maxLives = 3;
    private int currentLives;

    private Vector3 respawnPosition;

    private MonoBehaviour movementScript;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PhotonView photonView;

    private bool isInvulnerable = false;

    private void Awake()
    {
        currentLives = maxLives;

        movementScript =
            GetComponent<PlayerMovement>() ??
            (MonoBehaviour)GetComponent<StarfyMovement>();

        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        photonView = GetComponent<PhotonView>();

        respawnPosition = transform.position;

        if (IsOwnedByLocalPlayer() && LevelManager.Instance != null)
        {
            LevelManager.Instance.UpdateHeartsUI(currentLives);
        }
    }

    private void Start()
    {
        if (IsOwnedByLocalPlayer() && LevelManager.Instance != null)
        {
            LevelManager.Instance.UpdateHeartsUI(currentLives);
        }
    }

    public void TakeDamage()
    {
        if (!IsOwnedByLocalPlayer()) return;
        if (isInvulnerable) return;
        if (LevelManager.Instance == null) return;

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
        movementScript.enabled = false;
        rb.linearVelocity = Vector2.zero;
        transform.position = respawnPosition;
        movementScript.enabled = true;
    }

    private void Die()
    {
        movementScript.enabled = false;
        rb.linearVelocity = Vector2.zero;
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

    private bool IsOwnedByLocalPlayer()
    {
        return photonView == null || photonView.IsMine;
    }

}
