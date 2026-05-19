using TMPro;
using UnityEngine;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject leaderboardItemPrefab;
    [SerializeField] private TMP_Text messageText;

    private void OnEnable()
    {
        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        ClearItems();
        messageText.text = "Загрузка...";

        StartCoroutine(ApiManager.Instance.GetRequest(
            ApiRoutes.GetLeaderboard,
            OnLeaderboardLoaded,
            OnLeaderboardError,
            true
        ));
    }

    private void OnLeaderboardLoaded(string responseJson)
    {
        messageText.text = "";

        LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(responseJson);

        if (response == null || !response.status || response.leaderboard == null)
        {
            messageText.text = "Ошибка загрузки лидерборда";
            return;
        }

        for (int i = 0; i < response.leaderboard.Length; i++)
        {
            LeaderboardUser user = response.leaderboard[i];

            GameObject itemObject = Instantiate(leaderboardItemPrefab, contentParent);
            LeaderBoardItemUI itemUI = itemObject.GetComponent<LeaderBoardItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(i + 1, user.name, user.totalScore);
            }
        }
    }

    private void OnLeaderboardError(string error)
    {
        Debug.LogError("Ошибка лидерборда: " + error);
        messageText.text = "Ошибка загрузки лидерборда";
    }

    private void ClearItems()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }
    }
}