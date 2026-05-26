using UnityEngine;
using System.Collections;

public class WaterDamage : MonoBehaviour
{
    [SerializeField] private float damageDelay = 5f;

    private Coroutine damageCoroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Storm") || other.CompareTag("Starfy"))
        {
            damageCoroutine = StartCoroutine(DamageAfterDelay(other.gameObject));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Storm") || other.CompareTag("Starfy"))
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private IEnumerator DamageAfterDelay(GameObject player)
    {
        yield return new WaitForSeconds(damageDelay);

        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage();
        }
    }
}