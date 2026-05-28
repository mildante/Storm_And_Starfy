using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class ShipHelmCheckpoint : MonoBehaviourPun
{
    [SerializeField] private GameObject[] comments;

    [SerializeField] private float dialogueDelay = 6.3f;

    private HashSet<GameObject> playersInside = new();
    private bool startedDialogue = false;

    private void Start()
    {
        foreach (GameObject comment in comments)
        {
            comment.SetActive(false);
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Starfy") && !other.CompareTag("Storm"))
            return;

        playersInside.Add(other.gameObject);

        if (!startedDialogue && playersInside.Count >= 2)
        {
            photonView.RPC(nameof(StartDialogueRPC), RpcTarget.All);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Starfy") && !other.CompareTag("Storm"))
            return;

        playersInside.Remove(other.gameObject);
    }

    [PunRPC]
    private void StartDialogueRPC()
    {
        if (startedDialogue)
            return;

        startedDialogue = true;

        StartCoroutine(DialogueRoutine());
    }

    private IEnumerator DialogueRoutine()
    {
        DisablePlayers();

        foreach (GameObject comment in comments)
        {
            comment.SetActive(true);

            yield return new WaitForSeconds(dialogueDelay);
        }

        LevelManager.Instance?.FinishLevel();
    }

    private void DisablePlayers()
    {
        GameObject storm = GameObject.FindGameObjectWithTag("Storm");
        GameObject starfy = GameObject.FindGameObjectWithTag("Starfy");

        DisablePlayer(storm);
        DisablePlayer(starfy);
    }

    private void DisablePlayer(GameObject player)
    {
        if (player == null)
            return;

        MonoBehaviour movement = player.GetComponent<PlayerMovement>();

        if (movement == null)
        {
            movement = player.GetComponent<StarfyMovement>();
        }

        if (movement != null)
        {
            movement.enabled = false;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
