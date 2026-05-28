using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ShipHelmCheckpoint : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private const byte StartHelmDialogueEvent = 30;
    private const string StartHelmDialoguePayload = "StartShipHelmDialogue";

    [SerializeField] private GameObject[] comments;

    [SerializeField] private float dialogueDelay = 6.3f;

    private HashSet<GameObject> playersInside = new();
    private bool startedDialogue = false;
    private bool dialogueRequested = false;

    private void Start()
    {
        if (comments == null)
            return;

        foreach (GameObject comment in comments)
        {
            if (comment == null)
                continue;

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
            RequestStartDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Starfy") && !other.CompareTag("Storm"))
            return;

        playersInside.Remove(other.gameObject);
    }

    private void RequestStartDialogue()
    {
        if (dialogueRequested || startedDialogue)
            return;

        dialogueRequested = true;

        if (!PhotonNetwork.InRoom)
        {
            StartDialogue();
            return;
        }

        PhotonNetwork.RaiseEvent(
            StartHelmDialogueEvent,
            StartHelmDialoguePayload,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != StartHelmDialogueEvent)
            return;

        if (!(photonEvent.CustomData is string payload) ||
            payload != StartHelmDialoguePayload)
            return;

        StartDialogue();
    }

    private void StartDialogue()
    {
        if (startedDialogue)
            return;

        startedDialogue = true;

        StartCoroutine(DialogueRoutine());
    }

    private IEnumerator DialogueRoutine()
    {
        DisablePlayers();

        if (comments != null)
        {
            foreach (GameObject comment in comments)
            {
                if (comment == null)
                    continue;

                comment.SetActive(true);

                yield return new WaitForSeconds(dialogueDelay);
            }
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
