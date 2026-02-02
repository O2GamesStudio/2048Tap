using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdatePopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button updateButton;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Messages")]
    [SerializeField] private string titleMessage = "업데이트 필요";
    [SerializeField] private string bodyMessage = "새로운 버전이 출시되었습니다.\n게임을 계속하려면 업데이트가 필요합니다.";

    private string storeUrl;

    /// <summary>
    /// 팝업 초기화
    /// </summary>
    /// <param name="url">구글 플레이 스토어 URL</param>
    public void Initialize(string url)
    {
        storeUrl = url;

        SetupUI();
        SetupButton();
    }

    private void SetupUI()
    {
        // 텍스트 설정
        if (titleText != null)
            titleText.text = titleMessage;

        if (messageText != null)
            messageText.text = bodyMessage;
    }

    private void SetupButton()
    {
        if (updateButton != null)
        {
            updateButton.onClick.RemoveAllListeners();
            updateButton.onClick.AddListener(OnUpdateButtonClicked);
        }
        else
        {
            Debug.LogError("Update Button이 할당되지 않았습니다!");
        }
    }

    private void OnUpdateButtonClicked()
    {
        Debug.Log($"스토어로 이동: {storeUrl}");

        // 효과음 재생 (SoundManager가 있다면)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIBtnClickSFX();
        }

        // 구글 플레이 스토어로 이동
        Application.OpenURL(storeUrl);

        // 앱 종료 (강제 업데이트이므로)
        Debug.Log("업데이트 버튼 클릭 - 앱 종료");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        // 버튼 리스너 정리
        if (updateButton != null)
            updateButton.onClick.RemoveAllListeners();
    }
}