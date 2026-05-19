using UnityEngine;

public class StarCollectible : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayStar();
        }

        LevelManager.Instance.CollectStar();
        Destroy(gameObject);
    }
}
