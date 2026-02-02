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
        // UnityAdsManager null 체크
        if (UnityAdsManager.Instance != null)
        {
            // 이벤트 구독
            UnityAdsManager.Instance.OnAdClosed += OnAdClosedHandler;
            UnityAdsManager.Instance.OnAdFailedToShow += OnAdFailedHandler;
        }
    }

    void OnDisable()
    {
        // 이벤트 구독 해제
        if (UnityAdsManager.Instance != null)
        {
            UnityAdsManager.Instance.OnAdClosed -= OnAdClosedHandler;
            UnityAdsManager.Instance.OnAdFailedToShow -= OnAdFailedHandler;
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
        // UnityAdsManager가 있고 광고가 로드되어 있으면 광고 표시
        if (UnityAdsManager.Instance != null && UnityAdsManager.Instance.IsAdLoaded())
        {
            UnityAdsManager.Instance.ShowRewardedAd();
        }
        else
        {
            // 광고가 없으면 바로 재시작
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