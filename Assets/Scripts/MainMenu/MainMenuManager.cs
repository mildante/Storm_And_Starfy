using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MainMenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject mainMenuPage;
    [SerializeField] private GameObject settingsPage;
    [SerializeField] private GameObject registerPage;
    [SerializeField] private GameObject loginPage;

    [SerializeField] private GameObject chooseConnectionPage;
    [SerializeField] private GameObject createRoomPage;
    [SerializeField] private GameObject joinRoomPage;
    [SerializeField] private GameObject chooseCharacterPage;

    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Text networkStatusText;

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

    [SerializeField] private Button openCharacterSelectionButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startGameButton;

    private readonly Vector3 normalScale = Vector3.one;
    private readonly Vector3 selectedScale = new Vector3(1.15f, 1.15f, 1f);

    private const string CharacterKey = "SelectedChar";
    private const string LevelName = "Level1";

    private bool isRoomRequestInProgress;
    private bool isSceneTransitionInProgress;
    private bool isReturningToConnectionPage;
    private bool joinedRoomAsHost;

    private void Start()
    {
        PlayerSession.LoadSession();

        Time.timeScale = 1f;
        PhotonNetwork.AutomaticallySyncScene = true;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMenuMusic();
        }

        HideDeprecatedReadyButton();
        ResetRoomState();
        ResetCharacterVisuals();

        if (PlayerSession.IsAuthorized)
        {
            StartCoroutine(ValidateSavedSession());
        }
        else
        {
            ShowLogin();
        }
    }

    private void ConnectToPhoton()
    {
        if (PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState == ClientState.ConnectingToNameServer)
            return;

        string playerName = !string.IsNullOrEmpty(PlayerSession.Name)
            ? PlayerSession.Name
            : "Player_" + Random.Range(10, 99);

        PhotonNetwork.NickName = playerName;
        SetStatus("Подключение к Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }

    private System.Collections.IEnumerator ValidateSavedSession()
    {
        yield return ApiManager.Instance.GetRequest(
            ApiRoutes.Me,
            OnMeLoaded,
            OnMeError,
            true
        );
    }

    private void OnMeLoaded(string responseJson)
    {
        MeResponse response = JsonUtility.FromJson<MeResponse>(responseJson);

        if (response != null && response.user != null)
        {
            PlayerSession.UpdateUser(response.user);
            OnAuthenticated();
            return;
        }

        ClearInvalidSession();
    }

    private void OnMeError(string error)
    {
        Debug.LogWarning("Не удалось проверить сохраненный токен: " + error);
        ClearInvalidSession();
    }

    private void ClearInvalidSession()
    {
        PlayerSession.Clear();
        ShowLogin();
    }

    private void HideAllPages()
    {
        SetActive(mainMenuPage, false);
        SetActive(settingsPage, false);
        SetActive(registerPage, false);
        SetActive(loginPage, false);
        SetActive(chooseConnectionPage, false);
        SetActive(createRoomPage, false);
        SetActive(joinRoomPage, false);
        SetActive(chooseCharacterPage, false);
    }

    public void ShowMainMenu()
    {
        HideAllPages();
        SetActive(mainMenuPage, true);
    }

    public void ShowSettings()
    {
        HideAllPages();
        SetActive(settingsPage, true);
    }

    public void OnAuthenticated()
    {
        SetStatus("");
        ShowMainMenu();

        if (!string.IsNullOrEmpty(PlayerSession.Name))
        {
            PhotonNetwork.NickName = PlayerSession.Name;
        }

        ConnectToPhoton();
    }

    public void ShowConnectionSelection()
    {
        HideAllPages();
        SetActive(chooseConnectionPage, true);

        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }

            SetStatus("Photon подключен");
            return;
        }

        SetStatus("Подключение к Photon...");
        ConnectToPhoton();
    }

    public void ShowCreateRoomPage()
    {
        HideAllPages();
        SetActive(createRoomPage, true);

        if (PhotonNetwork.InRoom)
        {
            UpdateRoomPlayersUI();
            UpdateOpenCharacterSelectionButtonState();
        }
        else
        {
            ClearRoomPlayersUI();
            UpdateOpenCharacterSelectionButtonState();
        }
    }

    public void ShowJoinRoomPage()
    {
        HideAllPages();
        SetActive(joinRoomPage, true);

        if (PhotonNetwork.InRoom)
        {
            UpdateRoomPlayersUI();
        }
        else
        {
            ClearRoomPlayersUI();
        }
    }

    public void ShowRegister()
    {
        HideAllPages();
        SetActive(registerPage, true);
    }

    public void ShowLogin()
    {
        HideAllPages();
        SetActive(loginPage, true);
    }

    public void Logout()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }

        PlayerSession.Clear();
        SetStatus("");
        ShowLogin();
    }

    public void ToggleSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.ToggleSound();
        }
    }

    public void CreatePhotonRoom()
    {
        if (isRoomRequestInProgress || PhotonNetwork.InRoom)
            return;

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            SetStatus("Подключение к Photon...");
            ConnectToPhoton();
            return;
        }

        isRoomRequestInProgress = true;
        string code = GenerateRoomCode(6);
        SetRoomCode(code);
        SetStatus("Создание комнаты...");

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsOpen = true,
            IsVisible = true
        };

        PhotonNetwork.CreateRoom(code, roomOptions);
    }

    private string GenerateRoomCode(int length)
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
        bool hasJoinCodeInput = joinCodeInput != null;
        if (isRoomRequestInProgress || PhotonNetwork.InRoom || !hasJoinCodeInput)
            return;

        string codeToJoin = joinCodeInput.text.Trim();
        bool isRoomCodeValid = IsValidRoomCode(codeToJoin);
        if (!isRoomCodeValid)
        {
            SetStatus("Введите 6-значный код комнаты");
            return;
        }

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            SetStatus("Подключение к Photon...");
            ConnectToPhoton();
            return;
        }

        if (!CanStartJoinRoomRequest(
            isRoomRequestInProgress,
            PhotonNetwork.InRoom,
            hasJoinCodeInput,
            isRoomCodeValid,
            PhotonNetwork.IsConnectedAndReady))
        {
            return;
        }

        isRoomRequestInProgress = true;
        SetStatus("Вход в комнату...");
        PhotonNetwork.JoinRoom(codeToJoin);
    }

    private static bool CanStartJoinRoomRequest(
        bool isRequestInProgress,
        bool isInRoom,
        bool hasJoinCodeInput,
        bool isRoomCodeValid,
        bool isConnectedAndReady)
    {
        return !isRequestInProgress &&
            !isInRoom &&
            hasJoinCodeInput &&
            isRoomCodeValid &&
            isConnectedAndReady;
    }

    private bool IsValidRoomCode(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 6)
            return false;

        for (int i = 0; i < code.Length; i++)
        {
            if (!char.IsDigit(code[i]))
                return false;
        }

        return true;
    }

    public void BackFromNetwork()
    {
        if (PhotonNetwork.InRoom)
        {
            isReturningToConnectionPage = true;
            ClearRoomPlayersUI();
            SetStatus("Выход из комнаты...");
            PhotonNetwork.LeaveRoom();
            return;
        }

        ResetRoomState();
        ShowConnectionSelection();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Photon: подключено к мастер-серверу.");
        SetStatus("Photon подключен");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Photon: успешно вошли в лобби.");
        SetStatus("Photon подключен");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isRoomRequestInProgress = false;
        isSceneTransitionInProgress = false;
        ResetRoomState();
        SetStatus("Соединение потеряно");

        if (PlayerSession.IsAuthorized)
        {
            ShowMainMenu();
        }
        else
        {
            ShowLogin();
        }
    }

    public override void OnJoinedRoom()
    {
        isRoomRequestInProgress = false;
        isReturningToConnectionPage = false;
        joinedRoomAsHost = PhotonNetwork.IsMasterClient;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            { CharacterKey, 0 }
        });

        Debug.Log($"Photon: вы вошли в комнату {PhotonNetwork.CurrentRoom.Name}");
        SetRoomCode(PhotonNetwork.CurrentRoom.Name);
        SetStatus(PhotonNetwork.IsMasterClient
            ? "..."
            : "Комната найдена");

        if (PhotonNetwork.IsMasterClient)
        {
            ShowCreateRoomPage();
        }
        else
        {
            ShowJoinRoomPage();
        }
    }

    public override void OnLeftRoom()
    {
        ResetRoomState();

        if (isReturningToConnectionPage)
        {
            isReturningToConnectionPage = false;
            ShowConnectionSelection();
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        isRoomRequestInProgress = false;
        SetRoomCode("");
        SetStatus("Не удалось создать комнату: " + message);
        ShowCreateRoomPage();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        isRoomRequestInProgress = false;
        SetStatus("Комната не найдена");
        ShowJoinRoomPage();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        SetStatus("Оба игрока в комнате");
        UpdateRoomPlayersUI();
        UpdateOpenCharacterSelectionButtonState();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!joinedRoomAsHost)
        {
            LeaveRoomAfterHostDisconnects();
            return;
        }

        ClearLocalCharacterSelection();
        SetStatus("...");

        if (chooseCharacterPage != null && chooseCharacterPage.activeSelf)
        {
            ShowCreateRoomPage();
        }

        UpdateRoomPlayersUI();
        UpdateOpenCharacterSelectionButtonState();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!joinedRoomAsHost)
        {
            LeaveRoomAfterHostDisconnects();
        }
    }

    private void LeaveRoomAfterHostDisconnects()
    {
        SetStatus("Соединение потеряно");
        isReturningToConnectionPage = true;

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            ShowConnectionSelection();
        }
    }

    private void UpdateRoomPlayersUI()
    {
        Player[] players = PhotonNetwork.InRoom ? PhotonNetwork.PlayerList : new Player[0];

        SetRoomPlayersVisible(PhotonNetwork.InRoom);

        string firstPlayerName = players.Length > 0 ? players[0].NickName : "...";
        string secondPlayerName = players.Length > 1 ? players[1].NickName : "...";

        SetText(hostPlayer1Text, firstPlayerName);
        SetText(hostPlayer2Text, secondPlayerName);
        SetText(guestPlayer1Text, firstPlayerName);
        SetText(guestPlayer2Text, secondPlayerName);

        UpdateCharacterButtonsState();
    }

    public void SelectCharacter1()
    {
        SetCharacterProperty(1);
    }

    public void SelectCharacter2()
    {
        SetCharacterProperty(2);
    }

    private void SetCharacterProperty(int characterId)
    {
        if (!PhotonNetwork.InRoom || IsCharacterTakenByOtherPlayer(characterId))
            return;

        int currentSelection = GetSelectedCharacter(PhotonNetwork.LocalPlayer);
        int nextSelection = currentSelection == characterId ? 0 : characterId;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            { CharacterKey, nextSelection }
        });
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(CharacterKey))
        {
            UpdateCharacterButtonsState();
        }
    }

    private void UpdateCharacterButtonsState()
    {
        ResetCharacterVisuals();

        if (!PhotonNetwork.InRoom)
            return;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int selectedCharacter = GetSelectedCharacter(player);
            if (selectedCharacter == 0)
                continue;

            bool isHost = player.IsMasterClient;
            bool isLocalPlayer = player == PhotonNetwork.LocalPlayer;

            if (selectedCharacter == 1)
            {
                SetText(stormPlayerNameText, player.NickName);
                if (stormImage != null)
                {
                    stormImage.sprite = isHost ? stormRedSprite : stormBlueSprite;
                }

                if (stormButton != null)
                {
                    stormButton.transform.localScale = selectedScale;
                    stormButton.interactable = isLocalPlayer;
                }
            }

            if (selectedCharacter == 2)
            {
                SetText(starfyPlayerNameText, player.NickName);
                if (starfyImage != null)
                {
                    starfyImage.sprite = isHost ? starfyRedSprite : starfyBlueSprite;
                }

                if (starfyButton != null)
                {
                    starfyButton.transform.localScale = selectedScale;
                    starfyButton.interactable = isLocalPlayer;
                }
            }
        }

        SetText(player1NameText, GetPlayerName(0));
        SetText(player2NameText, GetPlayerName(1));
        UpdateStartGameButtonState();
    }

    private void ResetCharacterVisuals()
    {
        if (stormButton != null)
        {
            stormButton.interactable = true;
            stormButton.transform.localScale = normalScale;
        }

        if (starfyButton != null)
        {
            starfyButton.interactable = true;
            starfyButton.transform.localScale = normalScale;
        }

        if (stormImage != null)
        {
            stormImage.sprite = stormDefaultSprite;
        }

        if (starfyImage != null)
        {
            starfyImage.sprite = starfyDefaultSprite;
        }

        SetText(stormPlayerNameText, "");
        SetText(starfyPlayerNameText, "");
        UpdateStartGameButtonState();
    }

    private void UpdateOpenCharacterSelectionButtonState()
    {
        if (openCharacterSelectionButton == null)
            return;

        bool canOpen = PhotonNetwork.InRoom &&
            PhotonNetwork.IsMasterClient &&
            PhotonNetwork.PlayerList.Length == 2 &&
            !isSceneTransitionInProgress;

        openCharacterSelectionButton.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        openCharacterSelectionButton.interactable = canOpen;
    }

    private void UpdateStartGameButtonState()
    {
        if (startGameButton == null)
            return;

        bool characterPageIsOpen = chooseCharacterPage != null && chooseCharacterPage.activeSelf;
        bool isHost = characterPageIsOpen && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient;

        startGameButton.gameObject.SetActive(isHost);
        startGameButton.interactable = CanStartGameWithSelectedCharacters() && !isSceneTransitionInProgress;

        if (isHost && PhotonNetwork.PlayerList.Length == 2 && !startGameButton.interactable)
        {
            SetStatus("Выберите разных персонажей");
        }
    }

    private bool CanStartGameWithSelectedCharacters()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient || PhotonNetwork.PlayerList.Length != 2)
            return false;

        int firstSelection = GetSelectedCharacter(PhotonNetwork.PlayerList[0]);
        int secondSelection = GetSelectedCharacter(PhotonNetwork.PlayerList[1]);

        return firstSelection != 0 &&
            secondSelection != 0 &&
            firstSelection != secondSelection;
    }

    private bool IsCharacterTakenByOtherPlayer(int characterId)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == PhotonNetwork.LocalPlayer)
                continue;

            if (GetSelectedCharacter(player) == characterId)
                return true;
        }

        return false;
    }

    private int GetSelectedCharacter(Player player)
    {
        if (player != null &&
            player.CustomProperties.TryGetValue(CharacterKey, out object value) &&
            value is int selectedCharacter)
        {
            return selectedCharacter;
        }

        return 0;
    }

    private string GetPlayerName(int index)
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.PlayerList.Length <= index)
            return "";

        return PhotonNetwork.PlayerList[index].NickName;
    }

    private void ClearLocalCharacterSelection()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.LocalPlayer == null)
            return;

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
        {
            { CharacterKey, 0 }
        });
    }

    public void StartGame()
    {
        if (!CanStartGameWithSelectedCharacters())
            return;

        isSceneTransitionInProgress = true;
        if (startGameButton != null)
        {
            startGameButton.interactable = false;
        }

        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        PhotonNetwork.LoadLevel(LevelName);
    }

    public void OpenCharacterSelection()
    {
        if (!PhotonNetwork.InRoom || !PhotonNetwork.IsMasterClient || PhotonNetwork.PlayerList.Length != 2)
        {
            UpdateOpenCharacterSelectionButtonState();
            return;
        }

        isSceneTransitionInProgress = true;
        UpdateOpenCharacterSelectionButtonState();

        photonView.RPC(nameof(RPC_OpenCharacterSelection), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_OpenCharacterSelection()
    {
        isSceneTransitionInProgress = false;
        HideAllPages();
        SetActive(chooseCharacterPage, true);
        SetStatus("Выберите разных персонажей");
        UpdateCharacterButtonsState();
    }

    private void ResetRoomState()
    {
        joinedRoomAsHost = false;
        isRoomRequestInProgress = false;
        isSceneTransitionInProgress = false;
        SetRoomCode("");
        ClearRoomPlayersUI();
        UpdateOpenCharacterSelectionButtonState();
        UpdateStartGameButtonState();
    }

    private void ClearRoomPlayersUI()
    {
        SetText(hostPlayer1Text, "");
        SetText(hostPlayer2Text, "");
        SetText(guestPlayer1Text, "");
        SetText(guestPlayer2Text, "");
        SetText(player1NameText, "");
        SetText(player2NameText, "");
        SetRoomPlayersVisible(false);
    }

    private void SetRoomPlayersVisible(bool visible)
    {
        SetTextParentActive(hostPlayer1Text, visible);
        SetTextParentActive(guestPlayer1Text, visible);
    }

    private void SetTextParentActive(TMP_Text text, bool active)
    {
        if (text != null && text.transform.parent != null)
        {
            text.transform.parent.gameObject.SetActive(active);
        }
    }

    private void HideDeprecatedReadyButton()
    {
        if (readyButton != null)
        {
            readyButton.gameObject.SetActive(false);
            readyButton.interactable = false;
        }
    }

    private void SetStatus(string message)
    {
        SetText(networkStatusText, message);

        if (!string.IsNullOrEmpty(message))
        {
            Debug.Log(message);
        }
    }

    private void SetRoomCode(string code)
    {
        SetText(roomCodeText, string.IsNullOrEmpty(code) ? "" : code);
    }

    private void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }
}
