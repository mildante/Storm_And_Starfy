using UnityEngine;
using Photon.Pun;
public class InvisibleLever : MonoBehaviourPun
{
    public Animator blockAnimator;
    public string boolParameter = "isActive";

    private bool isOn = true;
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
            photonView.RPC("ToggleLeverRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void ToggleLeverRPC()
    {
        isOn = !isOn;

        Vector3 scale = transform.localScale;
        scale.x = isOn ? -originalScaleX : originalScaleX;
        transform.localScale = scale;

        if (blockAnimator != null)
        {
            blockAnimator.SetBool(boolParameter, isOn);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv != null && !pv.IsMine) return;

        if (other.CompareTag("Storm") || other.CompareTag("Starfy"))
        {
            playerNear = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PhotonView pv = other.GetComponent<PhotonView>();
        if (pv != null && !pv.IsMine) return;

        if (other.CompareTag("Storm") || other.CompareTag("Starfy"))
        {
            playerNear = false;
        }
    }
}
