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

    void OnEnable()
    {
        // 이벤트 구독
        GoogleAdsManager.Instance.OnAdClosed += OnAdClosedHandler;
        GoogleAdsManager.Instance.OnAdFailedToShow += OnAdFailedHandler;
    }

    void OnDisable()
    {
        // 이벤트 구독 해제
        if (GoogleAdsManager.Instance != null)
        {
            GoogleAdsManager.Instance.OnAdClosed -= OnAdClosedHandler;
            GoogleAdsManager.Instance.OnAdFailedToShow -= OnAdFailedHandler;
        }
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
        // 광고가 로드되어 있으면 광고 표시
        if (GoogleAdsManager.Instance.IsAdLoaded())
        {
            GoogleAdsManager.Instance.ShowRewardedAd();
        }
        else
        {
            RestartScene();
        }
    }

    void OnAdClosedHandler()
    {
        RestartScene();
    }

    void OnAdFailedHandler()
    {
        RestartScene();
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ToLobbyOnClick()
    {
        SceneManager.LoadScene("LobbyScene");
    }
}