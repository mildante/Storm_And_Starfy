using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayButton();
        }
    }
}