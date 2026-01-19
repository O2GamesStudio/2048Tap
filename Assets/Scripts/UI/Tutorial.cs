using System.Security;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] Button preBtn, nextBtn, exitBtn;
    [SerializeField] Image tutorialImage;
    [SerializeField] Sprite[] tutorialSprites;
    [SerializeField] Sprite[] tutorialSprites_EG;
    [SerializeField] Image[] tutorialIndexImages;

    [Header("Index Indicator Colors")]
    [SerializeField] Color activeColor = Color.white;
    [SerializeField] Color inactiveColor = Color.gray;

    [Header("Test Settings (Editor Only)")]
    [SerializeField] bool overrideLanguageInEditor = false;
    [SerializeField] SystemLanguage testLanguage = SystemLanguage.English;

    private int currentIndex = 0;
    private const string TUTORIAL_KEY = "HasSeenTutorial";
    private Sprite[] currentTutorialSprites;

    void Awake()
    {
        // 언어에 따라 사용할 스프라이트 배열 결정
        currentTutorialSprites = IsKorean() ? tutorialSprites : tutorialSprites_EG;

        preBtn.onClick.AddListener(PreBtnOnClick);
        nextBtn.onClick.AddListener(NextBtnOnClick);
        exitBtn.onClick.AddListener(ExitOnClick);

        // 디버그 로그로 현재 언어 확인
        Debug.Log($"Current Language: {GetCurrentLanguage()}, Using Korean Sprites: {IsKorean()}");
    }

    // 현재 언어 가져오기 (에디터 테스트용 오버라이드 포함)
    SystemLanguage GetCurrentLanguage()
    {
#if UNITY_EDITOR
        if (overrideLanguageInEditor)
        {
            return testLanguage;
        }
#endif
        return Application.systemLanguage;
    }

    // 한국어인지 확인하는 메서드
    bool IsKorean()
    {
        return GetCurrentLanguage() == SystemLanguage.Korean;
    }

    void Start()
    {
        if (!HasSeenTutorial())
        {
            ShowTutorial();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        currentIndex = 0;
        UpdateTutorialImage();
        UpdateButtonStates();
        UpdateIndexIndicators();
    }

    void ExitOnClick()
    {
        MarkTutorialAsSeen();
        gameObject.SetActive(false);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIBtnClickSFX();
    }

    void PreBtnOnClick()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            UpdateTutorialImage();
            UpdateButtonStates();
            UpdateIndexIndicators();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayUIBtnClickSFX();
        }
    }

    void NextBtnOnClick()
    {
        if (currentIndex < currentTutorialSprites.Length - 1)
        {
            currentIndex++;
            UpdateTutorialImage();
            UpdateButtonStates();
            UpdateIndexIndicators();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayUIBtnClickSFX();
        }
    }

    void UpdateTutorialImage()
    {
        if (currentTutorialSprites != null && currentTutorialSprites.Length > 0 && tutorialImage != null)
        {
            tutorialImage.sprite = currentTutorialSprites[currentIndex];
        }
    }

    void UpdateButtonStates()
    {
        if (preBtn != null)
        {
            preBtn.interactable = currentIndex > 0;
        }

        if (nextBtn != null)
        {
            nextBtn.interactable = currentIndex < currentTutorialSprites.Length - 1;
        }
    }

    void UpdateIndexIndicators()
    {
        if (tutorialIndexImages == null || tutorialIndexImages.Length == 0)
            return;

        for (int i = 0; i < tutorialIndexImages.Length; i++)
        {
            if (tutorialIndexImages[i] != null)
            {
                tutorialIndexImages[i].color = (i == currentIndex) ? activeColor : inactiveColor;
            }
        }
    }

    public void ShowTutorial()
    {
        gameObject.SetActive(true);
        currentIndex = 0;
        UpdateTutorialImage();
        UpdateButtonStates();
        UpdateIndexIndicators();
    }

    bool HasSeenTutorial()
    {
        return PlayerPrefs.GetInt(TUTORIAL_KEY, 0) == 1;
    }

    void MarkTutorialAsSeen()
    {
        PlayerPrefs.SetInt(TUTORIAL_KEY, 1);
        PlayerPrefs.Save();
    }

    public static void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_KEY);
        PlayerPrefs.Save();
    }
}