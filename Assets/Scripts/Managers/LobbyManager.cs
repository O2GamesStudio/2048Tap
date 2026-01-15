using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("Button UI")]
    [SerializeField] Button gameStartBtn;
    [SerializeField] Button preChapterBtn, nextChapterBtn;
    [SerializeField] Button settingBtn;

    [Header("Image UI")]
    [SerializeField] Image[] chapterImages;
    [SerializeField] Image startBtnImage;
    [SerializeField] Sprite[] startSprites;

    [Header("Text UI")]
    [SerializeField] TextMeshProUGUI chapterText;
    [SerializeField] TextMeshProUGUI challengeText;
    [SerializeField] TextMeshProUGUI highScoreText;

    [Header("Layout Manager")]
    [SerializeField] LobbyLayoutManager layoutManager;

    [Header("Challenge")]
    [SerializeField] Challenge challenge;

    [Header("Animation Settings")]
    [SerializeField] float slideDistance = 1000f;
    [SerializeField] float slideDuration = 0.5f;
    [SerializeField] Ease slideEase = Ease.OutCubic;
    [SerializeField] float scaleDuration = 0.3f;
    [SerializeField] float centerScale = 1f;
    [SerializeField] float sideScale = 0.7f;
    [SerializeField] Ease scaleEase = Ease.InOutSine;

    int chapterNum = 0;
    [SerializeField] GameObject settingPanel;
    [SerializeField] int maxChapterNum = 1;
    bool isAnimating = false;

    [Header("Configuration")]
    [SerializeField] ChapterUnlockConfig unlockConfig;

    void Awake()
    {
        maxChapterNum = chapterImages.Length - 1;

        gameStartBtn.onClick.AddListener(() => StartOnClick());
        preChapterBtn.onClick.AddListener(() => ChapterMoveOnClick(-1));
        nextChapterBtn.onClick.AddListener(() => ChapterMoveOnClick(1));
        settingBtn.onClick.AddListener(SettingOnClick);

        InitializeChapterPositions();
    }

    void Start()
    {
        SoundManager.Instance.PlayBGM();

        if (challenge != null)
        {
            challenge.SetLobbyManager(this);
        }

        UpdateHighScoreUI(chapterNum);
        UpdateButtonStates();
        UpdateStartButton();

        if (layoutManager != null)
        {
            layoutManager.RefreshLockPanels();
            layoutManager.UpdateChallengeSetVisibility(chapterNum == 2 ? 1 : 0);
        }
    }

    void SettingOnClick()
    {
        settingPanel.SetActive(true);
        SoundManager.Instance.PlayUIBtnClickSFX();
    }

    void InitializeChapterPositions()
    {
        for (int i = 0; i < chapterImages.Length; i++)
        {
            RectTransform rect = chapterImages[i].GetComponent<RectTransform>();
            if (i == chapterNum)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one * centerScale;
            }
            else if (i < chapterNum)
            {
                rect.anchoredPosition = new Vector2(-slideDistance, 0);
                rect.localScale = Vector3.one * sideScale;
            }
            else
            {
                rect.anchoredPosition = new Vector2(slideDistance, 0);
                rect.localScale = Vector3.one * sideScale;
            }
        }
    }

    IEnumerator BtnClickAnim(int dir)
    {
        if (dir == -1)
        {
            preChapterBtn.transform.DOScale(new Vector3(-0.8f, 0.8f, 0.8f), 0.1f);
            yield return new WaitForSeconds(0.1f);
            preChapterBtn.transform.DOScale(new Vector3(-1f, 1f, 1f), 0.1f);
        }
        else
        {
            nextChapterBtn.transform.DOScale(Vector3.one * 0.8f, 0.1f);
            yield return new WaitForSeconds(0.1f);
            nextChapterBtn.transform.DOScale(Vector3.one, 0.1f);
        }
    }

    void ChapterMoveOnClick(int dir)
    {
        if (isAnimating) return;

        if (dir == -1)
        {
            if (chapterNum <= 0) return;

            SoundManager.Instance.PlayUIBtnClickSFX();
            StartCoroutine(BtnClickAnim(dir));

            int oldChapterNum = chapterNum;
            chapterNum--;

            UpdateChapterTextUI(chapterNum);
            UpdateHighScoreUI(chapterNum);
            UpdateButtonStates();
            UpdateStartButton();

            if (layoutManager != null)
            {
                layoutManager.RefreshLockPanels();
                // Challenge UI는 Chapter 2에서만 표시
                layoutManager.UpdateChallengeSetVisibility(chapterNum == 2 ? 1 : 0);
            }

            SlideChapters(oldChapterNum, chapterNum);
        }
        else if (dir == 1)
        {
            if (chapterNum >= maxChapterNum) return;

            SoundManager.Instance.PlayUIBtnClickSFX();
            StartCoroutine(BtnClickAnim(dir));

            int oldChapterNum = chapterNum;
            chapterNum++;

            UpdateChapterTextUI(chapterNum);
            UpdateHighScoreUI(chapterNum);
            UpdateButtonStates();
            UpdateStartButton();

            if (layoutManager != null)
            {
                layoutManager.RefreshLockPanels();
                layoutManager.UpdateChallengeSetVisibility(chapterNum == 2 ? 1 : 0);
            }

            SlideChapters(oldChapterNum, chapterNum);
        }
    }
    void SlideChapters(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= chapterImages.Length ||
            toIndex < 0 || toIndex >= chapterImages.Length)
        {
            Debug.LogError($"Invalid chapter index: fromIndex={fromIndex}, toIndex={toIndex}, arrayLength={chapterImages.Length}");
            return;
        }

        isAnimating = true;

        RectTransform fromRect = chapterImages[fromIndex].GetComponent<RectTransform>();
        RectTransform toRect = chapterImages[toIndex].GetComponent<RectTransform>();

        float fromEndX = toIndex > fromIndex ? -slideDistance : slideDistance;

        Sequence mainSequence = DOTween.Sequence();

        mainSequence.Append(
            fromRect.DOScale(sideScale, scaleDuration).SetEase(scaleEase)
        );

        mainSequence.Append(
            fromRect.DOAnchorPosX(fromEndX, slideDuration).SetEase(slideEase)
        );
        mainSequence.Join(
            toRect.DOAnchorPosX(0, slideDuration).SetEase(slideEase)
        );

        mainSequence.Append(
            toRect.DOScale(centerScale, scaleDuration).SetEase(scaleEase)
        );

        mainSequence.OnComplete(() => isAnimating = false);
    }

    void UpdateChapterTextUI(int chapterNum)
    {
        if (chapterNum == 0)
        {
            chapterText.text = "4x4";
            challengeText.gameObject.SetActive(false);
        }
        else if (chapterNum == 1)
        {
            chapterText.text = "5x5";
            challengeText.gameObject.SetActive(false);
        }
        else if (chapterNum == 2)
        {
            chapterText.text = "5x5";
            challengeText.gameObject.SetActive(true);
        }
    }

    void UpdateHighScoreUI(int chapterNum)
    {
        int gridSize = (chapterNum == 0) ? 4 : 5;
        string highScoreKey;

        if (chapterNum == 2 && challenge != null)
        {
            int challengeNum = challenge.GetChallengeNum();
            highScoreKey = $"HighScore_5x5_Challenge{challengeNum}";
        }
        else
        {
            highScoreKey = $"HighScore_{gridSize}x{gridSize}";
        }

        int highScore = PlayerPrefs.GetInt(highScoreKey, 0);
        Debug.Log($"Loading high score for chapter {chapterNum}: {highScoreKey} = {highScore}");

        if (highScoreText != null)
        {
            highScoreText.text = highScore.ToString();
        }
    }


    bool IsChapterLocked(int chapterNum)
    {
        if (chapterNum == 0) return false;

        if (chapterNum == 1)
        {
            string prevHighScoreKey = "HighScore_4x4";
            int prevHighScore = PlayerPrefs.GetInt(prevHighScoreKey, 0);
            return prevHighScore < unlockConfig.chapter1UnlockScore;
        }

        if (chapterNum == 2)
        {
            string prevHighScoreKey = "HighScore_5x5";
            int prevHighScore = PlayerPrefs.GetInt(prevHighScoreKey, 0);
            return prevHighScore < unlockConfig.chapter2UnlockScore;
        }

        return false;
    }

    void UpdateStartButton()
    {
        bool isLocked = IsChapterLocked(chapterNum);

        if (startBtnImage != null && startSprites != null && startSprites.Length >= 2)
        {
            startBtnImage.sprite = isLocked ? startSprites[1] : startSprites[0];
        }

        if (gameStartBtn != null)
        {
            gameStartBtn.interactable = !isLocked;
        }
    }

    void UpdateButtonStates()
    {
        if (preChapterBtn != null)
        {
            preChapterBtn.interactable = (chapterNum > 0);
        }

        if (nextChapterBtn != null)
        {
            nextChapterBtn.interactable = (chapterNum < maxChapterNum);
        }
    }

    void StartOnClick()
    {
        if (IsChapterLocked(chapterNum))
        {
            Debug.Log("This chapter is locked!");
            return;
        }

        if (chapterNum == 2 && challenge != null)
        {
            challenge.OnStartGameClick();
        }
        else
        {
            GameDataTransfer.SetChallengeNum(0);
        }

        SoundManager.Instance.PlayUIBtnClickSFX();

        int sceneIndex = (chapterNum == 0) ? 1 : 2;
        SceneManager.LoadScene(sceneIndex);
    }

    void OnDestroy()
    {
        DOTween.Kill(this);
    }

    public void OnChallengeChanged()
    {
        UpdateHighScoreUI(chapterNum);
    }
}