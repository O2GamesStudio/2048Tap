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
    }

    void UpdateLockPanels()
    {
        for (int i = 0; i < chapters.Length; i++)
        {
            if (chapters[i].lockPanel != null)
            {
                bool isLocked = IsChapterLocked(i);
                chapters[i].lockPanel.gameObject.SetActive(isLocked);
            }
        }
    }

    bool IsChapterLocked(int chapterIndex)
    {
        // 첫 번째 챕터(0)는 항상 unlock
        if (chapterIndex == 0) return false;

        // 이전 챕터의 최고 점수가 1000점 이상이어야 다음 챕터 unlock
        int prevGridSize = (chapterIndex - 1 == 0) ? 4 : 5;
        string prevHighScoreKey = $"HighScore_{prevGridSize}x{prevGridSize}";
        int prevHighScore = PlayerPrefs.GetInt(prevHighScoreKey, 0);

        // 이전 챕터에서 1000점 이상을 얻어야 unlock
        return prevHighScore < 1000;
    }

    public void RefreshLockPanels()
    {
        UpdateLockPanels();
    }

    void OnRectTransformDimensionsChange()
    {
        if (chapters != null && chapters.Length > 0)
        {
            AdjustLayout();
        }
    }
}