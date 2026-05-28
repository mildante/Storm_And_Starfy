using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDamage : MonoBehaviour
{
    [SerializeField] private float damageDelay = 5f;

    private readonly Dictionary<GameObject, Coroutine> damageCoroutines =
        new Dictionary<GameObject, Coroutine>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Storm") || other.CompareTag("Starfy"))
        {
            if (!damageCoroutines.ContainsKey(other.gameObject))
            {
                damageCoroutines[other.gameObject] =
                    StartCoroutine(DamageAfterDelay(other.gameObject));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Storm") || other.CompareTag("Starfy"))
        {
            if (damageCoroutines.TryGetValue(other.gameObject, out Coroutine damageCoroutine))
            {
                StopCoroutine(damageCoroutine);
                damageCoroutines.Remove(other.gameObject);
            }
        }
    }

    private void OnDisable()
    {
        foreach (Coroutine damageCoroutine in damageCoroutines.Values)
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
            }
        }

        damageCoroutines.Clear();
    }

    private IEnumerator DamageAfterDelay(GameObject player)
    {
        yield return new WaitForSeconds(damageDelay);

        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage();
        }

        damageCoroutines.Remove(player);
    }
}
