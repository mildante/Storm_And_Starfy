using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyKingPig : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (!collision.collider.CompareTag("Player")) return;
        PlayerHealth playerHealth = collision.collider.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage();
        }
    }
}
