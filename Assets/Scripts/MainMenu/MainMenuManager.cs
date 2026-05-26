using Photon.Pun;
using Photon.Realtime;

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject mainMenuPage;
    [SerializeField] private GameObject settingsPage;
    [SerializeField] private GameObject levelsPage;
    [SerializeField] private GameObject registerPage;
    [SerializeField] private GameObject loginPage;
    [SerializeField] private GameObject leaderBoardPage;

    [SerializeField] private GameObject chooseConnectionPage;
    [SerializeField] private GameObject createRoomPage;
    [SerializeField] private GameObject joinRoomPage;

    [SerializeField] private GameObject chooseCharacterPage;

    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text roomCodeText;

    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;

    [SerializeField] private TMP_Text hostPlayer1Text;
    [SerializeField] private TMP_Text hostPlayer2Text;
    [SerializeField] private TMP_Text guestPlayer1Text;
    [SerializeField] private TMP_Text guestPlayer2Text;

    [SerializeField] private TMP_Text player1NameText;
    [SerializeField] private TMP_Text player2NameText;

    [SerializeField] private Button stormButton; 
    [SerializeField] private Button starfyButton;

    [SerializeField] private Image stormImage;
    [SerializeField] private Image starfyImage;

    [SerializeField] private Sprite stormDefaultSprite;
    [SerializeField] private Sprite stormRedSprite;
    [SerializeField] private Sprite stormBlueSprite;

    [SerializeField] private Sprite starfyDefaultSprite;
    [SerializeField] private Sprite starfyRedSprite;
    [SerializeField] private Sprite starfyBlueSprite;

    [SerializeField] private TMP_Text stormPlayerNameText;
    [SerializeField] private TMP_Text starfyPlayerNameText;

    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private bool soundOn = true;

    private Vector3 normalScale = Vector3.one;
    private Vector3 selectedScale = new Vector3(1.15f, 1.15f, 1f);

    private const string CHAR_KEY = "SelectedChar";
    private const string READY_KEY = "IsReady";

    private void Start()
    {
        PlayerSession.LoadSession();

        Time.timeScale = 1f;

        PhotonNetwork.AutomaticallySyncScene = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMenuMusic();
        }

        if (PlayerSession.IsAuthorized)
        {
            ShowMainMenu();
            StartCoroutine(LoadProgress());

            ConnectToPhoton();
        }
        else
        {
            ShowLogin();
        }

        if (startGameButton != null)
            startGameButton.interactable = false;

        stormImage.sprite = stormRedSprite;
    }
    private void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            string pName = !string.IsNullOrEmpty(PlayerSession.Name) ? PlayerSession.Name : "Player_" + Random.Range(10, 99);
            PhotonNetwork.NickName = pName;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    private void HideAllPages()
    {
        mainMenuPage.SetActive(false);
        settingsPage.SetActive(false);
        levelsPage.SetActive(false);
        registerPage.SetActive(false);
        loginPage.SetActive(false);
        leaderBoardPage.SetActive(false);
        chooseConnectionPage.SetActive(false);
        createRoomPage.SetActive(false);
        joinRoomPage.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAllPages();
        mainMenuPage.SetActive(true);
    }

    public void ShowSettings()
    {
        HideAllPages();
        settingsPage.SetActive(true);
    }
    public void ShowLeaderboard()
    {
        HideAllPages();
        leaderBoardPage.SetActive(true);
    }

    public void ShowConnectionSelection()
    {
        if (PhotonNetwork.InLobby)
        {
            HideAllPages();
            chooseConnectionPage.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Photon еще не подключился к лобби, подождите...");
            ConnectToPhoton();
        }
    }

    public void ShowCreateRoomPage()
    {
        HideAllPages();
        createRoomPage.SetActive(true);
    }
    public void ShowJoinRoomPage()
    {
        HideAllPages();
        if (joinRoomPage != null) joinRoomPage.SetActive(true);
    }

    public void ShowLevels()
    {
        HideAllPages();
        levelsPage.SetActive(true);

        if (PlayerSession.IsAuthorized)
        {
            StartCoroutine(LoadProgress());
        }
        else
        {
            UpdateLevelButtons();
        }
    }

    public void ShowRegister()
    {
        HideAllPages();
        registerPage.SetActive(true);
    }

    public void ShowLogin()
    {
        HideAllPages();
        loginPage.SetActive(true);
    }

    public void Logout()
    {
        PlayerSession.Clear();
        ShowLogin();
        UpdateLevelButtons();
    }

    public void StartLevel1()
    {
        if (!PlayerSession.Level1Unlocked) return;
        Time.timeScale = 1f;
        PhotonNetwork.LoadLevel("Level1");
    }

    public void StartLevel2()
    {
        if (!PlayerSession.Level2Unlocked) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level2");
    }

    public void StartLevel3()
    {
        if (!PlayerSession.Level3Unlocked) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene("Level3");
    }

    public void ToggleSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleSound();
        }
    }

    private IEnumerator LoadProgress()
    {
        yield return ApiManager.Instance.GetRequest(
            ApiRoutes.GetProgress + "?userId=" + PlayerSession.UserId,
            OnProgressLoaded,
            OnProgressError,
            true
        );
    }

    private void OnProgressLoaded(string responseJson)
    {
        ProgressResponse response = JsonUtility.FromJson<ProgressResponse>(responseJson);

        PlayerSession.Level1Unlocked = true;
        PlayerSession.Level2Unlocked = false;
        PlayerSession.Level3Unlocked = false;

        if (response != null && response.progress != null)
        {
            foreach (var item in response.progress)
            {
                if (item.levelNumber == 1) PlayerSession.Level1Unlocked = item.isUnlocked;
                if (item.levelNumber == 2) PlayerSession.Level2Unlocked = item.isUnlocked;
                if (item.levelNumber == 3) PlayerSession.Level3Unlocked = item.isUnlocked;
            }
        }

        UpdateLevelButtons();
    }

    private void OnProgressError(string error)
    {
        Debug.LogError("Ошибка загрузки прогресса: " + error);
        UpdateLevelButtons();
    }

    private void UpdateLevelButtons()
    {
        if (level1Button != null) level1Button.interactable = PlayerSession.Level1Unlocked;
        if (level2Button != null) level2Button.interactable = PlayerSession.Level2Unlocked;
        if (level3Button != null) level3Button.interactable = PlayerSession.Level3Unlocked;
    }

    public void CreatePhotonRoom()
    {
        string code = GenerateRoomCode(6);

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsOpen = true,
            IsVisible = true
        };

        PhotonNetwork.CreateRoom(code, roomOptions);
    }
    private string GenerateRoomCode(int length = 6)
    {
        string code = "";
        for (int i = 0; i < length; i++)
        {
            code += Random.Range(0, 10).ToString();
        }
        return code;
    }
    public void JoinPhotonRoomByCode()
    {
        if (joinCodeInput == null) return;

        string codeToJoin = joinCodeInput.text.Trim();
        if (!string.IsNullOrEmpty(codeToJoin))
        {
            PhotonNetwork.JoinRoom(codeToJoin);
        }
    }
    public void BackFromNetwork()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        ShowConnectionSelection();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon: Подключено к мастер-серверу.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Photon: Успешно вошли в лобби! Сеть готова.");
    }

    public override void OnJoinedRoom()
    {
        Hashtable props = new Hashtable
        {
            { CHAR_KEY, 0 },
            { READY_KEY, false }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log($"Photon: Вы вошли в комнату {PhotonNetwork.CurrentRoom.Name}");

        if (PhotonNetwork.IsMasterClient)
        {
            ShowCreateRoomPage();
        }
        else
        {
            ShowJoinRoomPage();
        }

        UpdateRoomPlayersUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateRoomPlayersUI();
        UpdateReadyState();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateRoomPlayersUI();
        UpdateReadyState();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Photon: Ошибка входа в комнату: {message}");
    }


    private void UpdateRoomPlayersUI()
    {
        string p1 = "...";
        string p2 = "...";

        Player[] players = PhotonNetwork.PlayerList;

        if (players.Length > 0)
            p1 = players[0].NickName;

        if (players.Length > 1)
            p2 = players[1].NickName;

        hostPlayer1Text.text = p1;
        hostPlayer2Text.text = p2;

        guestPlayer1Text.text = p1;
        guestPlayer2Text.text = p2;

        UpdateCharacterButtonsState();
    }

    public void SelectCharacter1() { SetCharacterProperty(1); }
    public void SelectCharacter2() { SetCharacterProperty(2); }

    private void SetCharacterProperty(int characterId)
    {
        int currentSelection = -1;

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(
            CHAR_KEY,
            out object value))
        {
            currentSelection = (int)value;
        }

        Hashtable props = new Hashtable();

        if (currentSelection == characterId)
        {
            props[CHAR_KEY] = 0;
        }
        else
        {
            props[CHAR_KEY] = characterId;
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(CHAR_KEY))
        {
            UpdateCharacterButtonsState();
        }

        if (changedProps.ContainsKey(READY_KEY))
        {
            UpdateReadyState();
        }
    }

    private void UpdateCharacterButtonsState()
    {
        stormButton.interactable = true;
        starfyButton.interactable = true;

        stormImage.sprite = stormDefaultSprite;
        starfyImage.sprite = starfyDefaultSprite;

        stormButton.transform.localScale = normalScale;
        starfyButton.transform.localScale = normalScale;

        stormPlayerNameText.text = "";
        starfyPlayerNameText.text = "";

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.TryGetValue(
                CHAR_KEY,
                out object value)) 
                continue;

            int selectedCharacter = (int)value;

            bool isHost = player.IsMasterClient;

            if (selectedCharacter == 1)
            {
                stormPlayerNameText.text = player.NickName;

                Debug.Log("Changing Storm sprite");

                stormImage.sprite =
                    isHost ?
                    stormRedSprite :
                    stormBlueSprite;

                stormButton.transform.localScale =
                    selectedScale;

                if (player != PhotonNetwork.LocalPlayer)
                    stormButton.interactable = false;
            }

            if (selectedCharacter == 2)
            {
                starfyPlayerNameText.text = player.NickName;

                starfyImage.sprite =
                    isHost ?
                    starfyRedSprite :
                    starfyBlueSprite;

                starfyButton.transform.localScale =
                    selectedScale;

                if (player != PhotonNetwork.LocalPlayer)
                    starfyButton.interactable = false;
            }
        }

        UpdateStartButtonState();
    } 

    private void UpdateStartButtonState()
    {
        bool hostSelected = false;
        bool guestSelected = false;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.TryGetValue(
                CHAR_KEY,
                out object value))
                continue;

            int selectedCharacter = (int)value;

            if (selectedCharacter == 0)
                continue;

            if (player.IsMasterClient)
                hostSelected = true;
            else
                guestSelected = true;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.interactable =
                PhotonNetwork.PlayerList.Length == 2
                && hostSelected
                && guestSelected;
        }
    }

    public void Ready()
    {
        Hashtable props = new Hashtable
        {
            { READY_KEY, true }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        readyButton.interactable = false;
    }

    private void UpdateReadyState()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        bool guestReady = false;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == PhotonNetwork.LocalPlayer)
                continue;

            if (player.CustomProperties.TryGetValue(READY_KEY, out object readyObj))
            {
                guestReady = (bool)readyObj;
            }
        }

        startGameButton.interactable =
            PhotonNetwork.PlayerList.Length == 2 &&
            guestReady;
    }
    public void OpenCharacterSelection()
    {
        photonView.RPC(nameof(RPC_OpenCharacterSelection),
            RpcTarget.All);
    }

    [PunRPC] 
    private void RPC_OpenCharacterSelection()
    {
        chooseCharacterPage.SetActive(true);
        UpdateCharacterButtonsState();
    }
}