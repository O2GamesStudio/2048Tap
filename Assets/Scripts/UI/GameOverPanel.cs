using System.Security;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] Button retryBtn, toLobbyBtn;

    void Awake()
    {
        retryBtn.onClick.AddListener(RetryOnClick);
        toLobbyBtn.onClick.AddListener(ToLobbyOnClick);
    }
    public void UpdateGameOverUI(int score)
    {
        scoreText.text = score.ToString();
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
