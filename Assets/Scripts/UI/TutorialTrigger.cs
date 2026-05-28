using TMPro;
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea]
    public string message;

    public TMP_Text tutorialText;

    public string[] targetTags;

    private bool wasShown = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (wasShown) return;

        bool correctTag = false;

        foreach (string tag in targetTags)
        {
            if (other.CompareTag(tag))
            {
                correctTag = true;
                break;
            }
        }

        if (!correctTag) return;

        wasShown = true;

        tutorialText.gameObject.SetActive(true);
        tutorialText.text = message;

        CancelInvoke(nameof(HideText));
        Invoke(nameof(HideText), 4f);
    }

    private void HideText()
    {
        tutorialText.gameObject.SetActive(false);
    }
}