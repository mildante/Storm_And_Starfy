using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public Animator animator;
    public Transform doorPoint;

    private readonly HashSet<int> playersInside = new HashSet<int>();
    private bool isOpened = false;

    public void OpenDoor()
    {
        isOpened = true;
        animator.SetBool("isOpen", true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!ShouldTrackPlayer(collision, out int actorNumber))
            return;

        playersInside.Add(actorNumber);

        if (playersInside.Count >= 2)
        {
            LevelManager.Instance.FinishLevel();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!ShouldTrackPlayer(collision, out int actorNumber))
            return;

        playersInside.Remove(actorNumber);
    }

    private bool ShouldTrackPlayer(Collider2D collision, out int actorNumber)
    {
        actorNumber = 0;

        if (!isOpened || !PhotonNetwork.IsMasterClient)
            return false;

        if (!collision.CompareTag("Player"))
            return false;

        PhotonView photonView = collision.GetComponentInParent<PhotonView>();
        if (photonView == null || photonView.Owner == null)
            return false;

        actorNumber = photonView.Owner.ActorNumber;
        return true;
    }
}
