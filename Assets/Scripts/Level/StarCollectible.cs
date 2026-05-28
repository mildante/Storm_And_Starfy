using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StarCollectible : MonoBehaviour, IOnEventCallback
{
    private const byte StarCollectedEvent = 20;

    private string networkKey;
    private bool isCollected;
    private Collider2D starCollider;
    private SpriteRenderer[] spriteRenderers;
    private Animator animator;

    private void Awake()
    {
        networkKey = BuildNetworkKey();
        starCollider = GetComponent<Collider2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!CanLocalStormCollect(collision))
            return;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.RaiseEvent(
                StarCollectedEvent,
                networkKey,
                new RaiseEventOptions { Receivers = ReceiverGroup.All },
                SendOptions.SendReliable);
            return;
        }

        ApplyCollected(true);
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code != StarCollectedEvent)
            return;

        if (!(photonEvent.CustomData is string collectedKey) || collectedKey != networkKey)
            return;

        bool playSound = PhotonNetwork.LocalPlayer != null &&
                         photonEvent.Sender == PhotonNetwork.LocalPlayer.ActorNumber;

        ApplyCollected(playSound);
    }

    private bool CanLocalStormCollect(Collider2D collision)
    {
        if (isCollected || !collision.CompareTag("Storm"))
            return false;

        PhotonView playerView = collision.GetComponentInParent<PhotonView>();
        return playerView == null || playerView.IsMine;
    }

    private void ApplyCollected(bool playSound)
    {
        if (isCollected)
            return;

        isCollected = true;

        if (playSound && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayStar();
        }

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CollectStar();
        }

        if (starCollider != null)
        {
            starCollider.enabled = false;
        }

        if (animator != null)
        {
            animator.enabled = false;
        }

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }
    }

    private string BuildNetworkKey()
    {
        Scene scene = gameObject.scene;
        Vector3 position = transform.position;

        return scene.name + ":" +
               BuildHierarchyPath(transform) + ":" +
               Mathf.RoundToInt(position.x * 100f) + ":" +
               Mathf.RoundToInt(position.y * 100f) + ":" +
               Mathf.RoundToInt(position.z * 100f);
    }

    private static string BuildHierarchyPath(Transform current)
    {
        string localName = current.GetSiblingIndex() + ":" + current.name;

        if (current.parent == null)
            return localName;

        return BuildHierarchyPath(current.parent) + "/" + localName;
    }
}
