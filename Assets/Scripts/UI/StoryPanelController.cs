using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class StoryPanelController : MonoBehaviour
{
    [SerializeField] private GameObject[] storyTexts;

    [SerializeField] private GameObject nextButton;
    [SerializeField] private Animator buttonAnimator;

    [SerializeField] private float textDelay = 5f;
    [SerializeField] private float loadDelay = 1f;

    private bool continueRequested;

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
        UpdateContinueButtonVisibility();
    }

    public void ContinueGame()
    {
        if (continueRequested || !CanShowContinueButton())
            return;

        continueRequested = true;
        SetContinueButtonInteractable(false);
        StartCoroutine(LoadNextScene());
    }

    private IEnumerator LoadNextScene()
    {
        if (buttonAnimator != null)
        {
            buttonAnimator.SetBool("isVisible", false);
        }

        yield return new WaitForSeconds(loadDelay);

        LevelManager.Instance?.RequestLoadNextLevel();
    }

    private void UpdateContinueButtonVisibility()
    {
        if (nextButton != null)
        {
            nextButton.SetActive(CanShowContinueButton());
        }
    }

    private bool CanShowContinueButton()
    {
        return !PhotonNetwork.InRoom || PhotonNetwork.IsMasterClient;
    }

    private void SetContinueButtonInteractable(bool isInteractable)
    {
        if (nextButton == null)
            return;

        Button button = nextButton.GetComponent<Button>();
        if (button != null)
        {
            button.interactable = isInteractable;
        }
    }
}
