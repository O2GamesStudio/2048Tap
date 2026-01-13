using System.Security;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] Button preBtn, nextBtn, exitBtn;
    [SerializeField] Image tutorialImage;
    [SerializeField] Sprite[] tutorialSprites;

    private int currentIndex = 0;
    private const string TUTORIAL_KEY = "HasSeenTutorial";

    void Awake()
    {
        preBtn.onClick.AddListener(PreBtnOnClick);
        nextBtn.onClick.AddListener(NextBtnOnClick);
        exitBtn.onClick.AddListener(ExitOnClick);
    }

    void Start()
    {
        // 튜토리얼을 본 적이 없으면 자동으로 표시
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
    }

    void ExitOnClick()
    {
        // 튜토리얼을 본 것으로 표시
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

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayUIBtnClickSFX();
        }
    }

    void NextBtnOnClick()
    {
        if (currentIndex < tutorialSprites.Length - 1)
        {
            currentIndex++;
            UpdateTutorialImage();
            UpdateButtonStates();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayUIBtnClickSFX();
        }
    }

    void UpdateTutorialImage()
    {
        if (tutorialSprites != null && tutorialSprites.Length > 0 && tutorialImage != null)
        {
            tutorialImage.sprite = tutorialSprites[currentIndex];
        }
    }

    void UpdateButtonStates()
    {
        // 첫 번째 이미지일 때 이전 버튼 비활성화
        if (preBtn != null)
        {
            preBtn.interactable = currentIndex > 0;
        }

        // 마지막 이미지일 때 다음 버튼 비활성화
        if (nextBtn != null)
        {
            nextBtn.interactable = currentIndex < tutorialSprites.Length - 1;
        }
    }

    public void ShowTutorial()
    {
        gameObject.SetActive(true);
        currentIndex = 0;
        UpdateTutorialImage();
        UpdateButtonStates();
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

    // 튜토리얼을 다시 볼 수 있도록 리셋하는 함수 (설정에서 사용 가능)
    public static void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TUTORIAL_KEY);
        PlayerPrefs.Save();
    }
}