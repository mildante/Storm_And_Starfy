using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCheckpoint : MonoBehaviour
{
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private float waitTime = 2f;

    private HashSet<GameObject> playersInside = new();
    private bool transitionStarted;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Starfy") && !other.CompareTag("Storm"))
            return;

        playersInside.Add(other.gameObject);

        if (!transitionStarted && playersInside.Count >= 2)
        {
            StartCoroutine(StartTransition());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Starfy") && !other.CompareTag("Storm"))
            return;

        playersInside.Remove(other.gameObject);
    }

    private IEnumerator StartTransition()
    {
        transitionStarted = true;

        yield return new WaitForSeconds(waitTime);

        if (playersInside.Count >= 2)
        {
            storyPanel.SetActive(true);
        }
        else
        {
            transitionStarted = false;
        }
    }
}