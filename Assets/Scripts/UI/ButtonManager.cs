using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        pauseAnimator.SetBool("isOpen", true);
    }

    public void ContinueGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pauseAnimator.SetBool("isOpen", false);
        StartCoroutine(HidePausePanel());
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
        Time.timeScale = 0f;

        textScore.text = "Очки: " + LevelManager.Instance.GetScore().ToString();
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
        Time.timeScale = 0f;

        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(true);
        loseAnimator.SetBool("isOpen", true);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadNextLevel()
    {
        Time.timeScale = 1f;

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(nextSceneIndex);
        else
            SceneManager.LoadScene("MainMenu");
    }

}