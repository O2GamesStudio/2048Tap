using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpdatePopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button updateButton;
    private string storeUrl = "https://play.google.com/store/apps/details?id=com.o2studio.tap2048";

    private void Awake()
    {
        SetupButton();
    }
    public void Initialize(string url)
    {
        Debug.Log("초기화 호출됨");
        storeUrl = url;
        SetupButton();
    }

    private void SetupButton()
    {
        if (updateButton != null)
        {
            updateButton.onClick.RemoveAllListeners();
            updateButton.onClick.AddListener(OnUpdateButtonClicked);
            Debug.Log("리스너 달림");
        }
    }

    private void OnUpdateButtonClicked()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIBtnClickSFX();
        }

        Application.OpenURL(storeUrl);

        // 앱 종료 (강제 업데이트이므로)
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
        {
            updateButton.onClick.RemoveAllListeners();
        }
    }
}