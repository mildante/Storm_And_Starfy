using System.Collections;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    public static LevelManager Instance { get; private set; }

    public const int TotalStarsInGame = 8;

    private const byte DefeatRequestEvent = 1;
    private const byte DefeatEvent = 2;
    private const byte RestartRequestEvent = 3;
    private const byte ReturnToMenuRequestEvent = 4;
    private const byte ReturnToMenuEvent = 5;
    private const byte RestartEvent = 6;
    private const byte FinishRequestEvent = 7;
    private const byte VictoryEvent = 8;
    private const byte NextLevelRequestEvent = 9;
    private const string CompletedLevelStarsProperty = "CompletedLevelStars";

    private static int completedLevelStars = 0;

    public GameObject startBlackPanel;

    [SerializeField] private GameObject[] startDialogueComments;
    [SerializeField] private float startDialogueDelay = 0.5f;

    public int collectedStars = 0;

    public CameraMove cameraMove;
    public Transform player;

    private bool levelFinished = false;
    private bool isLeavingRoom = false;
    private bool isSceneTransitionInProgress = false;
    public ButtonManager buttonManager;

    public GameObject heart1;
    public GameObject heart2;
    public GameObject heart3;

    public TMP_Text textCountDiamond;

    private void Awake()
    {
        Instance = this;

        string activeSceneName = SceneManager.GetActiveScene().name;

        if (activeSceneName == "Level1")
        {
            ResetRunProgress();
        }
        else
        {
            SyncCompletedStarsFromRoom();
        }

        SetStartDialogueCommentsActive(false);
    }

    private void Start()
    {
        UpdateDiamondUI();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLevelMusic(GetCurrentLevelNumber());
        }

        StartCoroutine(LevelStartRoutine());
        StartCoroutine(StartDialogueRoutine());
    }

    public void CollectStar()
    {
        collectedStars++;
        UpdateDiamondUI();
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

    public void FinishLevel()
    {
        if (levelFinished)
            return;

        if (!ShouldActAsHost())
        {
            RaiseToMaster(FinishRequestEvent);
            return;
        }

        levelFinished = true;
        StartCoroutine(FinishRoutine());
    }

    private IEnumerator FinishRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        if (SceneManager.GetActiveScene().name == "Level1")
        {
            CommitCurrentLevelStars();
            LoadLevel2AsHost();
        }
        else
            BroadcastVictory();
    }

    public void RequestLoadNextLevel()
    {
        if (ShouldActAsHost())
        {
            LoadLevel2AsHost();
            return;
        }

        RaiseToMaster(NextLevelRequestEvent);
    }

    private void LoadLevel2AsHost()
    {
        if (!ShouldActAsHost() || isSceneTransitionInProgress)
            return;

        if (SceneManager.GetActiveScene().name != "Level1")
            return;

        CommitCurrentLevelStars();
        isSceneTransitionInProgress = true;
        levelFinished = true;
        Time.timeScale = 1f;

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LoadLevel("Level2");
        else
            SceneManager.LoadScene("Level2");
    }

    private void BroadcastVictory()
    {
        if (!ShouldActAsHost())
            return;

        if (PhotonNetwork.InRoom)
            RaiseToAll(VictoryEvent);
        else
            ApplyVictory();
    }

    private void ApplyVictory()
    {
        if (buttonManager != null &&
            buttonManager.winPanel != null &&
            buttonManager.winPanel.activeSelf)
            return;

        SyncCompletedStarsFromRoom();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayWin();
        }

        levelFinished = true;
        SetLocalPlayerControl(false);

        if (buttonManager != null)
        {
            buttonManager.ShowWinPanel();
        }
    }

    public void LoseLevel()
    {
        RequestDefeat();
    }

    public void RequestDefeat()
    {
        if (levelFinished)
            return;

        if (ShouldActAsHost())
        {
            BroadcastDefeat();
            return;
        }

        RaiseToMaster(DefeatRequestEvent);
    }

    private void BroadcastDefeat()
    {
        if (PhotonNetwork.InRoom)
            RaiseToAll(DefeatEvent);
        else
            ApplyDefeat();
    }

    private void ApplyDefeat()
    {
        if (levelFinished)
            return;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLose();
        }

        levelFinished = true;
        SetLocalPlayerControl(false);

        if (buttonManager != null)
        {
            buttonManager.ShowLosePanel();
        }
    }

    public void RequestRestartLevel()
    {
        if (ShouldActAsHost())
        {
            BroadcastRestartCurrentLevel();
            return;
        }

        RaiseToMaster(RestartRequestEvent);
    }

    public void RequestReturnToMenu()
    {
        if (ShouldActAsHost())
        {
            BroadcastReturnToMenu();
            return;
        }

        RaiseToMaster(ReturnToMenuRequestEvent);
    }

    public void RestartCurrentLevelAsHost()
    {
        BroadcastRestartCurrentLevel();
    }

    private void BroadcastRestartCurrentLevel()
    {
        if (!ShouldActAsHost())
            return;

        string sceneName = SceneManager.GetActiveScene().name;

        if (PhotonNetwork.InRoom)
            RaiseToAll(RestartEvent, sceneName);
        else
            RestartScene(sceneName);
    }

    private void RestartScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
            return;

        Time.timeScale = 1f;

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LoadLevel(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }

    private void BroadcastReturnToMenu()
    {
        if (PhotonNetwork.InRoom)
            RaiseToAll(ReturnToMenuEvent);
        else
            ReturnToMenu();
    }

    private void ReturnToMenu()
    {
        if (isLeavingRoom)
            return;

        isLeavingRoom = true;
        Time.timeScale = 1f;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            return;
        }

        SceneManager.LoadScene("MainMenu");
    }

    public override void OnLeftRoom()
    {
        if (isLeavingRoom)
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    public void SetLocalPlayerControl(bool isEnabled)
    {
        if (player == null)
            return;

        PlayerMovement stormMovement = player.GetComponent<PlayerMovement>();
        if (stormMovement != null)
        {
            stormMovement.enabled = isEnabled;
        }

        StarfyMovement starfyMovement = player.GetComponent<StarfyMovement>();
        if (starfyMovement != null)
        {
            starfyMovement.enabled = isEnabled;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (!isEnabled && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        switch (photonEvent.Code)
        {
            case DefeatRequestEvent:
                if (PhotonNetwork.IsMasterClient)
                    BroadcastDefeat();
                break;

            case DefeatEvent:
                ApplyDefeat();
                break;

            case RestartRequestEvent:
                if (PhotonNetwork.IsMasterClient)
                    BroadcastRestartCurrentLevel();
                break;

            case RestartEvent:
                RestartScene(photonEvent.CustomData as string);
                break;

            case ReturnToMenuRequestEvent:
                if (PhotonNetwork.IsMasterClient)
                    BroadcastReturnToMenu();
                break;

            case ReturnToMenuEvent:
                ReturnToMenu();
                break;

            case FinishRequestEvent:
                if (PhotonNetwork.IsMasterClient)
                    FinishLevel();
                break;

            case VictoryEvent:
                ApplyVictory();
                break;

            case NextLevelRequestEvent:
                if (PhotonNetwork.IsMasterClient)
                    LoadLevel2AsHost();
                break;
        }
    }

    private void RaiseToMaster(byte eventCode)
    {
        PhotonNetwork.RaiseEvent(
            eventCode,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            SendOptions.SendReliable);
    }

    private void RaiseToAll(byte eventCode)
    {
        RaiseToAll(eventCode, null);
    }

    private void RaiseToAll(byte eventCode, object content)
    {
        PhotonNetwork.RaiseEvent(
            eventCode,
            content,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable);
    }

    private bool ShouldActAsHost()
    {
        return !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;
    }

    private int GetCurrentLevelNumber()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Level1") return 1;
        if (sceneName == "Level2") return 2;

        return 1;
    }

    public int GetScore()
    {
        return GetCollectedStarsForRun() * 100;
    }

    public int GetCollectedStarsForRun()
    {
        SyncCompletedStarsFromRoom();
        return Mathf.Clamp(completedLevelStars + collectedStars, 0, TotalStarsInGame);
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

    private void CommitCurrentLevelStars()
    {
        if (SceneManager.GetActiveScene().name != "Level1")
            return;

        completedLevelStars = Mathf.Clamp(collectedStars, 0, TotalStarsInGame);

        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
        {
            ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { CompletedLevelStarsProperty, completedLevelStars }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
    }

    private void ResetRunProgress()
    {
        completedLevelStars = 0;

        if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
        {
            ExitGames.Client.Photon.Hashtable roomProperties = new ExitGames.Client.Photon.Hashtable
            {
                { CompletedLevelStarsProperty, completedLevelStars }
            };

            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
        }
    }

    private void SyncCompletedStarsFromRoom()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
            return;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(CompletedLevelStarsProperty, out object starsValue) &&
            starsValue is int stars)
        {
            completedLevelStars = Mathf.Clamp(stars, 0, TotalStarsInGame);
        }
    }

    private IEnumerator LevelStartRoutine()
    {
        if (startBlackPanel == null)
            yield break;

        startBlackPanel.SetActive(true);

        SetLocalPlayerControl(false);

        if (SceneManager.GetActiveScene().name == "Level1")
        {
            yield return new WaitForSecondsRealtime(17.5f);
        }
        else
        {
            yield return new WaitForSecondsRealtime(2f);
            startBlackPanel.SetActive(false);
        }

        SetLocalPlayerControl(true);
    }

    private IEnumerator StartDialogueRoutine()
    {
        if (startDialogueComments == null || startDialogueComments.Length == 0)
            yield break;

        yield return new WaitForSecondsRealtime(startDialogueDelay);

        foreach (GameObject comment in startDialogueComments)
        {
            if (comment == null)
                continue;

            comment.SetActive(true);

            Animator animator = comment.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }
        }
    }

    private void SetStartDialogueCommentsActive(bool isActive)
    {
        if (startDialogueComments == null)
            return;

        foreach (GameObject comment in startDialogueComments)
        {
            if (comment != null)
            {
                comment.SetActive(isActive);
            }
        }
    }
}
