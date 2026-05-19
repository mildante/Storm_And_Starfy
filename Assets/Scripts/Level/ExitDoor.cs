using UnityEngine;

public class ExitDoor : MonoBehaviour
{
    public Animator animator;
    public Transform doorPoint;

    private bool isOpened = false;

    public void OpenDoor()
    {
        isOpened = true;
        animator.SetBool("isOpen", true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isOpened)
            return;

        if (!collision.CompareTag("Player"))
            return;

        LevelManager.Instance.FinishLevel();
    }
}
