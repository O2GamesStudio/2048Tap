using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdatePopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button updateButton;
    private string storeUrl;


    public void Initialize(string url)
    {
        storeUrl = url;

        SetupButton();
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