using System.Security;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] Button retryBtn, toLobbyBtn;

    public void UpdateGameOverUI(int score)
    {
        scoreText.text = score.ToString();
    }
}
