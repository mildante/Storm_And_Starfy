using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject panel;

    public void OpenPanel()
    {
        animator.SetBool("isOpen", true);
    }
    public void ClosePanel()
    {
        animator.SetBool("isOpen", false);
    }
    public void DisablePanel()
    {
        panel.SetActive(false);
    }
}
