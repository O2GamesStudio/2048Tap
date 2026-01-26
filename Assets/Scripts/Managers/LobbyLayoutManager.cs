using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ChapterLayoutData
{
    public RectTransform backgroundRect;
    public GridLayoutGroup gridLayoutGroup;
    public RectTransform gridRect;
    public RectTransform lockPanel;
    public int gridSize = 4;
}

public class LobbyLayoutManager : MonoBehaviour
{
    [Header("Chapter Elements")]
    [SerializeField] ChapterLayoutData[] chapters;

    [Header("Common Elements")]
    [SerializeField] RectTransform leftArrowBtn;
    [SerializeField] RectTransform rightArrowBtn;
    [SerializeField] RectTransform topText;
    [SerializeField] RectTransform highScoreSet;
    [SerializeField] RectTransform challengeSet;
    [SerializeField] Canvas canvas;

    [Header("Background Settings")]
    [SerializeField] float screenHorizontalMargin = 200f;
    [SerializeField] float backgroundYOffset = 0f;

    [Header("Button Settings")]
    [SerializeField] float arrowDistanceFromBackground = 100f;
    [SerializeField] float arrowButtonYOffset = 0f;

    [Header("Text Settings")]
    [SerializeField] float topTextDistanceFromBackground = 150f;
    [SerializeField] float bottomTextDistanceFromBackground = 150f;

    [Header("Grid Settings")]
    [SerializeField] float cellSpacing = 10f;
    [SerializeField] float gridPadding = 20f;

    [Header("Configuration")]
    [SerializeField]
    ChapterUnlockConfig unlockConfig;

    private Vector2 lastCanvasSize;
    float timer = 0f;

    void Start()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        lastCanvasSize = canvasRect.rect.size;
        SetupAnchors();
        AdjustLayout();
        UpdateLockPanels();

        if (challengeSet != null)
        {
            challengeSet.gameObject.SetActive(false);
        }
    }

    void SetupAnchors()
    {
        foreach (var chapter in chapters)
        {
            if (chapter.backgroundRect != null)
            {
                chapter.backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
                chapter.backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
                chapter.backgroundRect.pivot = new Vector2(0.5f, 0.5f);
            }
        }

        if (leftArrowBtn != null)
        {
            leftArrowBtn.anchorMin = new Vector2(0.5f, 0.5f);
            leftArrowBtn.anchorMax = new Vector2(0.5f, 0.5f);
            leftArrowBtn.pivot = new Vector2(0.5f, 0.5f);
        }
        if (rightArrowBtn != null)
        {
            rightArrowBtn.anchorMin = new Vector2(0.5f, 0.5f);
            rightArrowBtn.anchorMax = new Vector2(0.5f, 0.5f);
            rightArrowBtn.pivot = new Vector2(0.5f, 0.5f);
        }
        if (topText != null)
        {
            topText.anchorMin = new Vector2(0.5f, 0.5f);
            topText.anchorMax = new Vector2(0.5f, 0.5f);
            topText.pivot = new Vector2(0.5f, 0.5f);
        }
        if (highScoreSet != null)
        {
            highScoreSet.anchorMin = new Vector2(0.5f, 0.5f);
            highScoreSet.anchorMax = new Vector2(0.5f, 0.5f);
            highScoreSet.pivot = new Vector2(0.5f, 0.5f);
        }
        if (challengeSet != null)
        {
            challengeSet.anchorMin = new Vector2(0.5f, 0.5f);
            challengeSet.anchorMax = new Vector2(0.5f, 0.5f);
            challengeSet.pivot = new Vector2(0.5f, 0.5f);
        }

    }

    void AdjustLayout()
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        float backgroundSize = canvasWidth - (screenHorizontalMargin * 2);

        float maxHeight = canvasHeight - (topTextDistanceFromBackground + bottomTextDistanceFromBackground + Mathf.Abs(backgroundYOffset));
        if (backgroundSize > maxHeight)
        {
            backgroundSize = maxHeight;
        }

        for (int i = 0; i < chapters.Length; i++)
        {
            AdjustChapter(chapters[i], backgroundSize);
        }

        AdjustArrowButtons(backgroundSize);
        AdjustTexts(backgroundSize);
    }

    void AdjustChapter(ChapterLayoutData chapter, float backgroundSize)
    {
        if (chapter.backgroundRect != null)
        {
            chapter.backgroundRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);
            Vector2 currentPos = chapter.backgroundRect.anchoredPosition;
            chapter.backgroundRect.anchoredPosition = new Vector2(currentPos.x, backgroundYOffset);
        }

        if (chapter.gridRect != null)
        {
            chapter.gridRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);
            Vector2 currentPos = chapter.gridRect.anchoredPosition;
            chapter.gridRect.anchoredPosition = new Vector2(currentPos.x, backgroundYOffset);
        }

        if (chapter.lockPanel != null)
        {
            chapter.lockPanel.sizeDelta = new Vector2(backgroundSize, backgroundSize);
            Vector2 currentPos = chapter.lockPanel.anchoredPosition;
            chapter.lockPanel.anchoredPosition = new Vector2(currentPos.x, backgroundYOffset);
        }

        if (chapter.gridLayoutGroup != null)
        {
            float gridAvailableSize = backgroundSize - (gridPadding * 2);
            float totalSpacing = cellSpacing * (chapter.gridSize - 1);
            float cellSize = (gridAvailableSize - totalSpacing) / chapter.gridSize;

            chapter.gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
            chapter.gridLayoutGroup.spacing = new Vector2(cellSpacing, cellSpacing);
            chapter.gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            chapter.gridLayoutGroup.constraintCount = chapter.gridSize;

            int padding = Mathf.RoundToInt(gridPadding);
            chapter.gridLayoutGroup.padding = new RectOffset(padding, padding, padding, padding);
        }
    }

    void AdjustArrowButtons(float backgroundSize)
    {
        if (leftArrowBtn != null)
        {
            float leftX = -(backgroundSize / 2) - arrowDistanceFromBackground;
            leftArrowBtn.anchoredPosition = new Vector2(leftX, arrowButtonYOffset);
        }

        if (rightArrowBtn != null)
        {
            float rightX = (backgroundSize / 2) + arrowDistanceFromBackground;
            rightArrowBtn.anchoredPosition = new Vector2(rightX, arrowButtonYOffset);
        }
    }

    void AdjustTexts(float backgroundSize)
    {
        if (topText != null)
        {
            float topY = backgroundYOffset + (backgroundSize / 2) + topTextDistanceFromBackground;
            topText.anchoredPosition = new Vector2(0, topY);
        }

        if (highScoreSet != null)
        {
            float bottomY = backgroundYOffset - (backgroundSize / 2) - bottomTextDistanceFromBackground;
            highScoreSet.anchoredPosition = new Vector2(0, bottomY);
        }

        if (challengeSet != null)
        {
            float bottomY = backgroundYOffset - (backgroundSize / 2) - bottomTextDistanceFromBackground - 250f;
            challengeSet.anchoredPosition = new Vector2(0, bottomY);
        }
    }

    void UpdateLockPanels()
    {
        for (int i = 0; i < chapters.Length; i++)
        {
            if (chapters[i].lockPanel != null)
            {
                bool isLocked = IsChapterLocked(i);
                chapters[i].lockPanel.gameObject.SetActive(isLocked);

                // 해상도에 따른 자식 요소 스케일 조정
                if (isLocked)
                {
                    Debug.Log($"[Chapter {i}] Adjusting lock panel children");
                    AdjustLockPanelChildren(chapters[i].lockPanel);
                }
            }
        }
    }

    void AdjustLockPanelChildren(RectTransform lockPanel)
    {
        // 실제 화면 해상도 사용
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        Debug.Log($"Screen Resolution: {screenWidth} x {screenHeight}");

        // 화면 비율 계산 (1080x1920 기준)
        float baseWidth = 1080f;
        float baseHeight = 1920f;
        float baseAspect = baseWidth / baseHeight; // 0.5625

        float currentAspect = screenWidth / screenHeight;

        Debug.Log($"Base Aspect: {baseAspect}, Current Aspect: {currentAspect}");

        // 가로가 더 좁으면 (세로가 더 길면) 가로 기준으로 스케일 조정
        float scaleFactor;
        if (currentAspect < baseAspect)
        {
            // 세로가 더 긴 경우 - 가로 기준으로 더 작게
            scaleFactor = screenWidth / baseWidth;
            Debug.Log($"Using width-based scale: {screenWidth} / {baseWidth} = {scaleFactor}");
        }
        else
        {
            // 가로가 더 넓은 경우 - 높이 기준
            scaleFactor = screenHeight / baseHeight;
            Debug.Log($"Using height-based scale: {screenHeight} / {baseHeight} = {scaleFactor}");
        }

        // LockPanel 크기 대비 조정 (추가 스케일 감소)
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float backgroundSize = canvasWidth - (screenHorizontalMargin * 2);

        // 기준 배경 크기 (1080x1920 기준)
        float baseBackgroundSize = 1080f - (screenHorizontalMargin * 2);
        float backgroundScale = backgroundSize / baseBackgroundSize;

        Debug.Log($"Background scale factor: {backgroundScale}");

        // 최종 스케일은 화면 비율과 배경 크기를 모두 고려
        scaleFactor = scaleFactor * backgroundScale * 0.8f; // 0.8배로 여유 공간 확보

        // 최소/최대 스케일 제한
        scaleFactor = Mathf.Clamp(scaleFactor, 0.5f, 1.5f);

        Debug.Log($"Final Scale Factor: {scaleFactor}");

        // 모든 자식 요소의 스케일 조정
        int childCount = 0;
        foreach (Transform child in lockPanel)
        {
            Vector3 oldScale = child.localScale;
            child.localScale = Vector3.one * scaleFactor;
            Debug.Log($"Child [{childCount}] '{child.name}': Scale {oldScale} → {child.localScale}");
            childCount++;
        }

        Debug.Log($"Total children adjusted: {childCount}");
    }


    bool IsChapterLocked(int chapterIndex)
    {
        if (chapterIndex == 0) return false;

        if (chapterIndex == 1)
        {
            string prevHighScoreKey = "HighScore_4x4";
            int prevHighScore = PlayerPrefs.GetInt(prevHighScoreKey, 0);
            return prevHighScore < unlockConfig.chapter1UnlockScore; // 수정
        }

        if (chapterIndex == 2)
        {
            string prevHighScoreKey = "HighScore_5x5";
            int prevHighScore = PlayerPrefs.GetInt(prevHighScoreKey, 0);
            return prevHighScore < unlockConfig.chapter2UnlockScore; // 수정
        }

        return false;
    }

    public void RefreshLockPanels()
    {
        UpdateLockPanels();
    }

    public void UpdateChallengeSetVisibility(int currentChapterNum)
    {
        if (challengeSet == null) return;

        if (currentChapterNum == 1)
        {
            string highScoreKey = "HighScore_5x5";
            int highScore = PlayerPrefs.GetInt(highScoreKey, 0);

            challengeSet.gameObject.SetActive(highScore >= 7000);
        }
        else
        {
            challengeSet.gameObject.SetActive(false);
        }
    }

    void OnRectTransformDimensionsChange()
    {
        if (chapters != null && chapters.Length > 0)
        {
            AdjustLayout();
        }
    }
}