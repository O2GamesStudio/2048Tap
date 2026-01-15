using UnityEngine;
using UnityEngine.UI;

public class GridLayoutManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RectTransform backgroundRect;
    [SerializeField] GridLayoutGroup gridLayoutGroup;
    [SerializeField] RectTransform gridRect;
    [SerializeField] Canvas canvas; // ★ Canvas 참조 추가

    [Header("Settings")]
    [SerializeField] float horizontalMargin = 50f;
    [SerializeField] float verticalMargin = 200f;
    [SerializeField] int gridSize = 4;
    [SerializeField] float cellSpacing = 10f;
    [SerializeField] float gridPadding = 20f;

    [Header("Offset")]
    [SerializeField] float gridYOffset = -100f;

    private float lastUpdateTime;
    private const float updateInterval = 0.5f;

    void Start()
    {
        // ★ Canvas 자동 찾기
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        SetupAnchors();
        AdjustGridSize();
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            AdjustGridSize();
            lastUpdateTime = Time.time;
        }
    }

    void SetupAnchors()
    {
        if (backgroundRect != null)
        {
            backgroundRect.anchorMin = new Vector2(0.5f, 0.5f);
            backgroundRect.anchorMax = new Vector2(0.5f, 0.5f);
            backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        }

        if (gridRect != null)
        {
            gridRect.anchorMin = new Vector2(0.5f, 0.5f);
            gridRect.anchorMax = new Vector2(0.5f, 0.5f);
            gridRect.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    void AdjustGridSize()
    {
        // ★ Canvas Rect 사용 (Screen 대신)
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // ★ 로비와 동일한 방식
        float backgroundSize = canvasWidth - (horizontalMargin * 2);

        // 세로 제한 확인
        float maxHeight = canvasHeight - (verticalMargin * 2) - Mathf.Abs(gridYOffset);
        if (backgroundSize > maxHeight)
        {
            backgroundSize = maxHeight;
        }

        if (backgroundRect != null)
        {
            backgroundRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);
            backgroundRect.anchoredPosition = new Vector2(0, gridYOffset);
        }

        float gridAvailableSize = backgroundSize - (gridPadding * 2);

        if (gridRect != null)
        {
            gridRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);
            gridRect.anchoredPosition = new Vector2(0, 0);
        }

        if (gridLayoutGroup != null)
        {
            float totalSpacing = cellSpacing * (gridSize - 1);
            float cellSize = (gridAvailableSize - totalSpacing) / gridSize;

            gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
            gridLayoutGroup.spacing = new Vector2(cellSpacing, cellSpacing);
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = gridSize;

            int padding = Mathf.RoundToInt(gridPadding);
            gridLayoutGroup.padding = new RectOffset(padding, padding, padding, padding);
        }

        AdjustChildImages(gridAvailableSize);

        Canvas.ForceUpdateCanvases();

        StartCoroutine(UpdateNumImagePositions());

        Debug.Log($"Canvas: {canvasWidth}x{canvasHeight}, Background Size: {backgroundSize}, " +
                  $"실제 좌우 여백: {(canvasWidth - backgroundSize) / 2f}px (목표: {horizontalMargin}px)");
    }

    void AdjustChildImages(float gridAvailableSize)
    {
        float totalSpacing = cellSpacing * (gridSize - 1);
        float cellSize = (gridAvailableSize - totalSpacing) / gridSize;

        foreach (Transform button in gridLayoutGroup.transform)
        {
            foreach (Transform child in button)
            {
                RectTransform childRect = child.GetComponent<RectTransform>();
                if (childRect != null)
                {
                    childRect.sizeDelta = new Vector2(cellSize, cellSize);
                    childRect.anchoredPosition = Vector2.zero;
                }
            }
        }
    }

    System.Collections.IEnumerator UpdateNumImagePositions()
    {
        yield return null;

        foreach (Transform button in gridLayoutGroup.transform)
        {
            NumBtn numBtn = button.GetComponent<NumBtn>();
            if (numBtn != null)
            {
                numBtn.UpdateImagePosition();
            }
        }
    }

    void OnRectTransformDimensionsChange()
    {
        if (backgroundRect != null && gridLayoutGroup != null)
        {
            AdjustGridSize();
        }
    }
}