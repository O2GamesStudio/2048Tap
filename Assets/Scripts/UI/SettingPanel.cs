using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] Button retryBtn, toLobbyBtn, closeBtn;

    void Awake()
    {
        closeBtn.onClick.AddListener(CloseOnClick);
        retryBtn.onClick.AddListener(RetryOnClick);
        toLobbyBtn.onClick.AddListener(ToLobbyOnClick);
    }
    void CloseOnClick()
    {
        this.gameObject.SetActive(false);
    }
    void RetryOnClick()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
    void ToLobbyOnClick()
    {
        SceneManager.LoadScene(0);
    }
}
