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
    [SerializeField] TextMeshProUGUI highScoreText;

    [Header("Layout Manager")]
    [SerializeField] LobbyLayoutManager layoutManager;

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

    void Awake()
    {
        // maxChapterNum은 마지막 챕터의 인덱스여야 함 (배열 길이 - 1)
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

        UpdateHighScoreUI(chapterNum);
        UpdateButtonStates();
        UpdateStartButton();

        if (layoutManager != null)
        {
            layoutManager.RefreshLockPanels();
            layoutManager.UpdateChallengeSetVisibility(chapterNum);
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

        if (dir == -1) //이전 챕터로 이동
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
                layoutManager.UpdateChallengeSetVisibility(chapterNum);
            }

            SlideChapters(oldChapterNum, chapterNum);
        }
        else if (dir == 1) // 다음 챕터로 이동
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
                layoutManager.UpdateChallengeSetVisibility(chapterNum);
            }

            SlideChapters(oldChapterNum, chapterNum);
        }
    }

    void SlideChapters(int fromIndex, int toIndex)
    {
        // 배열 범위 체크
        if (fromIndex < 0 || fromIndex >= chapterImages.Length ||
            toIndex < 0 || toIndex >= chapterImages.Length)
        {
            Debug.LogError($"Invalid chapter index: fromIndex={fromIndex}, toIndex={toIndex}, arrayLength={chapterImages.Length}");
            return;
        }

        isAnimating = true;

        RectTransform fromRect = chapterImages[fromIndex].GetComponent<RectTransform>();
        RectTransform toRect = chapterImages[toIndex].GetComponent<RectTransform>();

        // 이동 방향 결정
        float fromEndX = toIndex > fromIndex ? -slideDistance : slideDistance;

        // 메인 Sequence 생성
        Sequence mainSequence = DOTween.Sequence();

        // 1단계: 중앙 이미지 스케일 축소
        mainSequence.Append(
            fromRect.DOScale(sideScale, scaleDuration).SetEase(scaleEase)
        );

        // 2단계: 위치 이동 애니메이션 (스케일 축소 후)
        mainSequence.Append(
            fromRect.DOAnchorPosX(fromEndX, slideDuration).SetEase(slideEase)
        );
        mainSequence.Join(
            toRect.DOAnchorPosX(0, slideDuration).SetEase(slideEase)
        );

        // 3단계: 중앙으로 온 이미지 스케일 확대 (이동 완료 후)
        mainSequence.Append(
            toRect.DOScale(centerScale, scaleDuration).SetEase(scaleEase)
        );

        mainSequence.OnComplete(() => isAnimating = false);
    }

    void UpdateChapterTextUI(int chapterNum)
    {
        if (chapterNum == 0) chapterText.text = "4x4";
        else if (chapterNum == 1) chapterText.text = "5x5";
    }

    void UpdateHighScoreUI(int chapterNum)
    {
        int gridSize = (chapterNum == 0) ? 4 : 5;
        string highScoreKey = $"HighScore_{gridSize}x{gridSize}";
        int highScore = PlayerPrefs.GetInt(highScoreKey, 0);

        if (highScoreText != null)
        {
            highScoreText.text = highScore.ToString();
        }
    }

    bool IsChapterLocked(int chapterNum)
    {
        if (chapterNum == 0) return false;

        int prevGridSize = (chapterNum - 1 == 0) ? 4 : 5;
        string prevHighScoreKey = $"HighScore_{prevGridSize}x{prevGridSize}";
        int prevHighScore = PlayerPrefs.GetInt(prevHighScoreKey, 0);

        return prevHighScore < 2000;
    }

    void UpdateStartButton()
    {
        bool isLocked = IsChapterLocked(chapterNum);

        if (startBtnImage != null && startSprites != null && startSprites.Length >= 2)
        {
            // 잠금 상태에 따라 스프라이트 변경 (0: unlock, 1: lock)
            startBtnImage.sprite = isLocked ? startSprites[1] : startSprites[0];
        }

        if (gameStartBtn != null)
        {
            // 잠긴 챕터는 버튼 비활성화
            gameStartBtn.interactable = !isLocked;
        }
    }

    void UpdateButtonStates()
    {
        // 이전 버튼 상태
        if (preChapterBtn != null)
        {
            preChapterBtn.interactable = (chapterNum > 0);
        }

        // 다음 버튼 상태
        if (nextChapterBtn != null)
        {
            nextChapterBtn.interactable = (chapterNum < maxChapterNum);
        }
    }

    void StartOnClick()
    {
        // 잠긴 챕터는 시작 불가
        if (IsChapterLocked(chapterNum))
        {
            Debug.Log("This chapter is locked!");
            return;
        }

        SoundManager.Instance.PlayUIBtnClickSFX();
        SceneManager.LoadScene(chapterNum + 1);
    }

    void OnDestroy()
    {
        // DOTween 정리
        DOTween.Kill(this);
    }
}