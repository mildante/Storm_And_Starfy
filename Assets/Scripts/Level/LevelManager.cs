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

    public int collectedStars;

    public ExitDoor exitDoor;
    public CameraMove cameraMove;
    public Transform player;

    private bool levelFinished = false;
    private bool isLeavingRoom = false;
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

    //private IEnumerator ShowDoorRoutine(Transform doorPoint)
    //{
    //    cameraMove.SetTarget(doorPoint);
    //    yield return new WaitForSeconds(1f);
    //    exitDoor.OpenDoor();
    //    yield return new WaitForSeconds(2f);
    //    cameraMove.SetTarget(player);
    //}

    public void FinishLevel()
    {
        if (levelFinished || !PhotonNetwork.IsMasterClient)
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
        yield return new WaitForSeconds(0.5f);

        if (SceneManager.GetActiveScene().name == "Level1")
        {
            PhotonNetwork.LoadLevel("Level2");
        }
        else if (buttonManager != null)
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

        if (PhotonNetwork.IsMasterClient)
        {
            BroadcastDefeat();
            return;
        }

        RaiseToMaster(DefeatRequestEvent);
    }

    private void BroadcastDefeat()
    {
        RaiseToAll(DefeatEvent);
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
        if (PhotonNetwork.IsMasterClient)
        {
            BroadcastRestartCurrentLevel();
            return;
        }

        RaiseToMaster(RestartRequestEvent);
    }

    public void RequestReturnToMenu()
    {
        if (PhotonNetwork.IsMasterClient)
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
        if (!PhotonNetwork.IsMasterClient)
            return;

        RaiseToAll(RestartEvent, SceneManager.GetActiveScene().name);
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
        RaiseToAll(ReturnToMenuEvent);
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
}
