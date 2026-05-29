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
        StopAllCoroutines();
        ResetCoins();

        int collectedCoins = LevelManager.Instance != null
            ? LevelManager.Instance.GetCollectedStarsForRun()
            : 0;

        if (coinsText != null)
        {
            coinsText.text = collectedCoins + "/" + LevelManager.TotalStarsInGame + " монет";
        }

        StartCoroutine(ShowCollectedCoins(collectedCoins));
    }

    private IEnumerator ShowCollectedCoins(int collectedCoins)
    {
        if (coinAnimators == null || coinAnimators.Length == 0)
            yield break;

        int visibleCoins = Mathf.Min(collectedCoins, coinAnimators.Length);

        for (int i = 0; i < visibleCoins; i++)
        {
            if (coinAnimators[i] == null)
                continue;

            coinAnimators[i].SetBool("isCollected", true);

            yield return new WaitForSeconds(coinDelay);
        }
    }

    private void ResetCoins()
    {
        if (coinAnimators == null)
            return;

        foreach (Animator coinAnimator in coinAnimators)
        {
            if (coinAnimator != null)
            {
                coinAnimator.SetBool("isCollected", false);
            }
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
