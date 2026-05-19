using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPage;
    [SerializeField] private GameObject settingsPage;
    [SerializeField] private GameObject levelsPage;
    [SerializeField] private GameObject registerPage;
    [SerializeField] private GameObject loginPage;
    [SerializeField] private GameObject leaderBoardPage;

    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;
    [SerializeField] private Button level3Button;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private bool soundOn = true;

    private void Start()
    {
        PlayerSession.LoadSession();

        Time.timeScale = 1f;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayMenuMusic();
        }

        if (PlayerSession.IsAuthorized)
        {
            ShowMainMenu();
            StartCoroutine(LoadProgress());
        }
        else
        {
            ShowLogin();
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
        SceneManager.LoadScene("Level1");
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
}