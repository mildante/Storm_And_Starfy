using System.Collections;
using UnityEngine;
using TMPro;

public class FinishPanel : MonoBehaviour
{
    [SerializeField] private Animator[] coinAnimators;

    [SerializeField] private TMP_Text coinsText;

    [SerializeField] private float coinDelay = 1f;

    private void OnEnable()
    {
        int collectedCoins = LevelManager.Instance != null
            ? LevelManager.Instance.collectedStars
            : 0;

        coinsText.text =
            collectedCoins + "/" + coinAnimators.Length + " монет";

        StartCoroutine(ShowCollectedCoins(collectedCoins));
    }

    private IEnumerator ShowCollectedCoins(int collectedCoins)
    {
        int visibleCoins = Mathf.Min(collectedCoins, coinAnimators.Length);

        for (int i = 0; i < visibleCoins; i++)
        {
            coinAnimators[i].SetBool("isCollected", true);

            yield return new WaitForSeconds(coinDelay);
        }
    }

    public void BackToMenu()
    {
        LevelManager.Instance?.RequestReturnToMenu();
    }

    public void RestartGame()
    {
        LevelManager.Instance?.RequestRestartLevel();
    }
}
