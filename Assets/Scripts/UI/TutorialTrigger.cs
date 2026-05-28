using Photon.Pun;
using TMPro;
using UnityEngine;
using System.Collections;

public class TutorialTrigger : MonoBehaviour
{
    [TextArea]
    public string message;

    public TMP_Text tutorialText;
    public GameObject tutorialPanel;

    public string[] targetTags;

    private static int currentMessageID = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PhotonView pv = other.GetComponent<PhotonView>();

        if (pv == null || !pv.IsMine)
            return;

        bool correctTag = false;

        foreach (string tag in targetTags)
        {
            if (other.CompareTag(tag))
            {
                correctTag = true;
                break;
            }
        }

        if (!correctTag)
            return;

        tutorialPanel.SetActive(true);

        tutorialText.text = message;

        currentMessageID++;

        StartCoroutine(HideRoutine(currentMessageID));
    }

    private IEnumerator HideRoutine(int messageID)
    {
        yield return new WaitForSeconds(4f);

        if (messageID == currentMessageID)
        {
            tutorialPanel.SetActive(false);
        }
    }
}