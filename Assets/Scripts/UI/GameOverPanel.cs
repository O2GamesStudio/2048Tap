using System.Security;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText, highScoreText;
    [SerializeField] Button retryBtn, toLobbyBtn;

    void Awake()
    {
        retryBtn.onClick.AddListener(RetryOnClick);
        toLobbyBtn.onClick.AddListener(ToLobbyOnClick);
    }

    public void UpdateGameOverUI(int score, int gridSize)
    {
        scoreText.text = score.ToString();

        string highScoreKey = $"HighScore_{gridSize}x{gridSize}";
        int highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        highScoreText.text = highScore.ToString();
    }

    void RetryOnClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ToLobbyOnClick()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}