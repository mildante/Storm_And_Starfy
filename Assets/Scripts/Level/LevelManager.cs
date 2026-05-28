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

    private const byte DefeatRequestEvent = 1;
    private const byte DefeatEvent = 2;
    private const byte RestartRequestEvent = 3;
    private const byte ReturnToMenuRequestEvent = 4;
    private const byte ReturnToMenuEvent = 5;
    private const byte RestartEvent = 6;
    private const byte FinishRequestEvent = 7;
    private const byte VictoryEvent = 8;
    private const byte NextLevelRequestEvent = 9;

    public GameObject startBlackPanel;
    public Animator startPanelAnimator;

    public int collectedStars;

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
    }

    private void Start()
    {
        collectedStars = 0;

        UpdateDiamondUI();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLevelMusic(GetCurrentLevelNumber());
        }

        StartCoroutine(LevelStartRoutine());
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
        PhotonNetwork.LoadLevel(sceneName);
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
        }

        SceneManager.LoadScene("MainMenu");
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
        return collectedStars * 100;
    }

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }
    private IEnumerator LevelStartRoutine()
    {
        if (startBlackPanel == null)
            yield break;

        startBlackPanel.SetActive(true);

        SetLocalPlayerControl(false);

        if (startPanelAnimator != null)
        {
            startPanelAnimator.Play("StartPanelHide");
        }

        if (SceneManager.GetActiveScene().name == "Level1")
        {
            yield return new WaitForSecondsRealtime(17.5f);
        }
        else
        {
            yield return new WaitForSecondsRealtime(1f);
        }

        startBlackPanel.SetActive(false);

        SetLocalPlayerControl(true);
    }
}
