using System.Collections;
using UnityEngine;

public class StoryPanelController : MonoBehaviour
{
    [SerializeField] private GameObject[] storyTexts;

    [SerializeField] private GameObject nextButton;
    [SerializeField] private Animator buttonAnimator;

    [SerializeField] private float textDelay = 5f;
    [SerializeField] private float loadDelay = 1f;

    private void Start()
    {
        foreach (GameObject text in storyTexts)
            text.SetActive(false);

        nextButton.SetActive(false);

        StartCoroutine(ShowTexts());
    }

    private IEnumerator ShowTexts()
    {
        foreach (GameObject text in storyTexts)
        {
            text.SetActive(true);
            yield return new WaitForSeconds(textDelay);
        }

        nextButton.SetActive(true);
    }

    public void ContinueGame()
    {
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        buttonAnimator.SetBool("isVisible", false);

        yield return new WaitForSeconds(loadDelay);

        LevelManager.Instance?.FinishLevel();
    }
}
