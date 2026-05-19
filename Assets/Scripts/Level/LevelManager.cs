using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    private int totalStars;
    private int collectedStars;

    public ExitDoor exitDoor;
    public CameraMove cameraMove;
    public Transform player;

    private bool levelFinished = false;
    public ButtonManager buttonManager;

    public GameObject heart1;
    public GameObject heart2;
    public GameObject heart3;

    public TMP_Text textCountDiamond;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StarCollectible[] stars = FindObjectsByType<StarCollectible>(FindObjectsSortMode.None);
        totalStars = stars.Length;
        collectedStars = 0;

        UpdateDiamondUI();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLevelMusic(GetCurrentLevelNumber());
        }
    }

    public void CollectStar()
    {
        collectedStars++;
        UpdateDiamondUI();

        if (collectedStars >= totalStars)
        {
            StartCoroutine(ShowDoorRoutine(exitDoor.doorPoint));
        }
    }

    public void UpdateHeartsUI(int currentLives)
    {
        heart1.SetActive(currentLives >= 1);
        heart2.SetActive(currentLives >= 2);
        heart3.SetActive(currentLives >= 3);
    }

    public void UpdateDiamondUI()
    {
        textCountDiamond.text = "x" + collectedStars.ToString();
    }

    private IEnumerator ShowDoorRoutine(Transform doorPoint)
    {
        cameraMove.SetTarget(doorPoint);
        yield return new WaitForSeconds(1f);
        exitDoor.OpenDoor();
        yield return new WaitForSeconds(2f);
        cameraMove.SetTarget(player);
    }

    public void FinishLevel()
    {
        if (levelFinished)
            return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWin();
        }
        levelFinished = true;
        StartCoroutine(FinishRoutine());
    }

    private IEnumerator FinishRoutine()
    {
        SendLevelComplete();
        yield return new WaitForSeconds(0.5f);
        buttonManager.ShowWinPanel();
    }

    public void LoseLevel()
    {
        if (levelFinished)
            return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLose();
        }

        levelFinished = true;
        buttonManager.ShowLosePanel();
    }

    private void SendLevelComplete()
    {
        if (!PlayerSession.IsAuthorized)
            return;

        CompleteLevelRequest request = new CompleteLevelRequest
        {
            userId = PlayerSession.UserId,
            levelNumber = GetCurrentLevelNumber(),
            score = collectedStars * 100
        };

        StartCoroutine(ApiManager.Instance.PostRequest(
            ApiRoutes.CompleteLevel,
            JsonUtility.ToJson(request),
            OnCompleteLevelSuccess,
            OnCompleteLevelError,
            true
        ));
    }

    private int GetCurrentLevelNumber()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Level1") return 1;
        if (sceneName == "Level2") return 2;
        if (sceneName == "Level3") return 3;

        return 1;
    }

    private void OnCompleteLevelSuccess(string responseJson)
    {
        CompleteLevelResponse response = JsonUtility.FromJson<CompleteLevelResponse>(responseJson);

        if (response != null && response.status)
        {
            PlayerSession.TotalScore = response.totalScore;
        }
    }

    private void OnCompleteLevelError(string error)
    {
        Debug.LogError("Ошибка сохранения уровня: " + error);
    }

    public int GetScore()
    {
        return collectedStars * 100;
    }
}