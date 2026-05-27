using System.Collections;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class VictoryPanel : MonoBehaviour
{
    [SerializeField] private Animator[] coinAnimators;

    [SerializeField] private TMP_Text coinsText;

    [SerializeField] private float coinDelay = 1f;

    [SerializeField] private string menuSceneName = "MainMenu";
    [SerializeField] private string restartSceneName = "Level2";

    private void OnEnable()
    {
        int collectedCoins = LevelManager.Instance.collectedStars;

        coinsText.text =
            collectedCoins + "/" + coinAnimators.Length + " монет";

        StartCoroutine(ShowCollectedCoins(collectedCoins));
    }

    private IEnumerator ShowCollectedCoins(int collectedCoins)
    {
        for (int i = 0; i < collectedCoins; i++)
        {
            coinAnimators[i].SetBool("isCollected", true);

            yield return new WaitForSeconds(coinDelay);
        }
    }

    public void BackToMenu()
    {
        PhotonNetwork.LoadLevel(menuSceneName);
    }

    public void RestartGame()
    {
        PhotonNetwork.LoadLevel(restartSceneName);
    }
}