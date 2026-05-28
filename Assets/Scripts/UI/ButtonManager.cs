using System.Collections;
using TMPro;
using UnityEngine;

public class ButtonManager : MonoBehaviour
{
    public GameObject gamePanel;
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject pausePanel;

    public Animator pauseAnimator;
    public Animator winAnimator;
    public Animator loseAnimator;

    public TMP_Text textScore;

    private bool isPaused = false;
    private bool blockPause = false;

    private void Start()
    {
        Time.timeScale = 1f;

        gamePanel.SetActive(true);
        winPanel.SetActive(false);
        losePanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (blockPause) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ContinueGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (blockPause) return;

        isPaused = true;
        LevelManager.Instance?.SetLocalPlayerControl(false);
        pausePanel.SetActive(true);
        pauseAnimator.SetBool("isOpen", true);
        SoundManager.Instance?.PlayPause();
    }

    public void ContinueGame()
    {
        isPaused = false;
        LevelManager.Instance?.SetLocalPlayerControl(true);
        pauseAnimator.SetBool("isOpen", false);
        StartCoroutine(HidePausePanel());
        SoundManager.Instance?.PlayPause();
    }

    private IEnumerator HidePausePanel()
    {
        yield return new WaitForSeconds(0.4f);
        pausePanel.SetActive(false);
    }

    public void ShowWinPanel()
    {
        blockPause = true;
        isPaused = false;
        LevelManager.Instance?.SetLocalPlayerControl(false);

        textScore.text = "Score: " + LevelManager.Instance.GetScore().ToString();
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        losePanel.SetActive(false);
        winPanel.SetActive(true);
        winAnimator.SetBool("isOpen", true);
    }

    public void ShowLosePanel()
    {
        blockPause = true;
        isPaused = false;
        LevelManager.Instance?.SetLocalPlayerControl(false);

        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(true);
        loseAnimator.SetBool("isOpen", true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        LevelManager.Instance?.RequestRestartLevel();
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        LevelManager.Instance?.RequestReturnToMenu();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        LevelManager.Instance?.RequestRestartLevel();
    }
}
