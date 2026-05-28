using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCheckpoint : MonoBehaviour
{
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private float waitTime = 2f;

    private HashSet<GameObject> playersInside = new();
    private bool transitionStarted;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Starfy") && !other.CompareTag("Storm"))
            return;

        playersInside.Add(other.gameObject);

        if (!transitionStarted && playersInside.Count >= 2)
        {
            DisablePlayersOnShip();
            StartCoroutine(StartTransition());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Starfy") && !other.CompareTag("Storm"))
            return;

        playersInside.Remove(other.gameObject);
    }

    private IEnumerator StartTransition()
    {
        transitionStarted = true;

        yield return new WaitForSeconds(waitTime);

        if (playersInside.Count >= 2)
        {
            storyPanel.SetActive(true);
        }
        else
        {
            transitionStarted = false;
        }
    }

    private void DisablePlayersOnShip()
    {
        foreach (GameObject player in playersInside)
        {
            DisablePlayer(player);
        }
    }

    private void DisablePlayer(GameObject player)
    {
        if (player == null)
            return;

        PlayerMovement stormMovement = player.GetComponent<PlayerMovement>();
        if (stormMovement != null)
        {
            stormMovement.enabled = false;
        }

        StarfyMovement starfyMovement = player.GetComponent<StarfyMovement>();
        if (starfyMovement != null)
        {
            starfyMovement.enabled = false;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
