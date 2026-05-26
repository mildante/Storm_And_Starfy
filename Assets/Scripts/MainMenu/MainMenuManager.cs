using Photon.Pun;
using Photon.Realtime;

using System.Collections;
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
            StartCoroutine(ValidateSavedSession());
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

    private IEnumerator ValidateSavedSession()
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
        if (mainMenuPage != null) mainMenuPage.SetActive(false);
        if (settingsPage != null) settingsPage.SetActive(false);
        if (registerPage != null) registerPage.SetActive(false);
        if (loginPage != null) loginPage.SetActive(false);
        if (chooseConnectionPage != null) chooseConnectionPage.SetActive(false);
        if (createRoomPage != null) createRoomPage.SetActive(false);
        if (joinRoomPage != null) joinRoomPage.SetActive(false);
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

    public void OnAuthenticated()
    {
        ShowMainMenu();

        if (!string.IsNullOrEmpty(PlayerSession.Name))
        {
            PhotonNetwork.NickName = PlayerSession.Name;
        }

        ConnectToPhoton();
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
        string code = GenerateRoomCode(6);

        roomCodeText.text = code;

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
        Debug.Log("Photon: подключено к мастер-серверу.");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Photon: успешно вошли в лобби. Сеть готова.");
    }

    public override void OnJoinedRoom()
    {
        Hashtable props = new Hashtable
        {
            { CHAR_KEY, 0 },
            { READY_KEY, false }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        Debug.Log($"Photon: вы вошли в комнату {PhotonNetwork.CurrentRoom.Name}");

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
        Debug.LogError($"Photon: ошибка входа в комнату: {message}");
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

    public void StartGame()
    {
        PhotonNetwork.LoadLevel("Level1");
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
