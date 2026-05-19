using TMPro;
using UnityEngine;

public class LeaderBoardItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text placeText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text scoreText;

    public void Setup(int place, string playerName, int score)
    {
        placeText.text = place.ToString();
        nameText.text = playerName;
        scoreText.text = score.ToString();
    }
}