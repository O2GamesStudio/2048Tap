using System.Security;
using UnityEngine;
using UnityEngine.UI;

public class Tutorial : MonoBehaviour
{
    [SerializeField] Button preBtn, nextBtn, exitBtn;
    [SerializeField] Image tutorialImage;
    [SerializeField] Sprite[] tutorialSprites;
    [SerializeField] Image[] tutorialIndexImages;

    [Header("Index Indicator Colors")]
    [SerializeField] Color activeColor = Color.white;
    [SerializeField] Color inactiveColor = Color.gray;

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
        if (currentIndex < tutorialSprites.Length - 1)
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
        if (tutorialSprites != null && tutorialSprites.Length > 0 && tutorialImage != null)
        {
            tutorialImage.sprite = tutorialSprites[currentIndex];
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
            nextBtn.interactable = currentIndex < tutorialSprites.Length - 1;
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