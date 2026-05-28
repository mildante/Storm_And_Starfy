using Photon.Pun;
using UnityEngine;

public class MovingLever : MonoBehaviourPun
{
    public MovingPlatform platform;

    private bool isOn = false;
    private bool playerNear = false;
    private float originalScaleX;

    private void Start()
    {
        originalScaleX = Mathf.Abs(transform.localScale.x);
    }

    private void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            RequestToggleLever();
        }
    }

    private void RequestToggleLever()
    {
        if (PhotonNetwork.InRoom && photonView != null && photonView.ViewID != 0)
        {
            photonView.RPC(nameof(ToggleLeverRPC), RpcTarget.All);
            return;
        }

        ToggleLeverRPC();
    }

    [PunRPC]
    private void ToggleLeverRPC()
    {
        isOn = !isOn;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLever();
        }

        Vector3 scale = transform.localScale;
        scale.x = isOn ? -originalScaleX : originalScaleX;
        transform.localScale = scale;

        if (platform != null)
        {
            platform.MoveToState(isOn);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PhotonView playerView = other.GetComponentInParent<PhotonView>();
        if (playerView != null && !playerView.IsMine)
            return;

        if (other.CompareTag("Storm") || other.CompareTag("Starfy"))
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PhotonView playerView = other.GetComponentInParent<PhotonView>();
        if (playerView != null && !playerView.IsMine)
            return;

        if (other.CompareTag("Storm") || other.CompareTag("Starfy"))
        {
            playerNear = false;
        }
    }
}
