using UnityEngine;
using UnityEngine.UI;

public class GridLayoutManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RectTransform backgroundRect; // 불투명한 배경
    [SerializeField] GridLayoutGroup gridLayoutGroup; // 버튼들의 GridLayoutGroup
    [SerializeField] RectTransform gridRect; // GridLayoutGroup이 있는 RectTransform

    [Header("Settings")]
    [SerializeField] float horizontalMargin = 50f; // 좌우 여백
    [SerializeField] int gridSize = 4; // 4x4 그리드
    [SerializeField] float cellSpacing = 10f; // 버튼 간 간격
    [SerializeField] float gridPadding = 20f; // ★ 배경과 그리드 사이의 여백

    void Start()
    {
        AdjustGridSize();
    }

    void AdjustGridSize()
    {
        float screenWidth = Screen.width;
        float backgroundWidth = screenWidth - (horizontalMargin * 2);
        float backgroundSize = backgroundWidth;
        backgroundRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);

        float gridAvailableSize = backgroundSize - (gridPadding * 2);

        gridRect.sizeDelta = new Vector2(backgroundSize, backgroundSize);

        float totalSpacing = cellSpacing * (gridSize - 1);
        float cellSize = (gridAvailableSize - totalSpacing) / gridSize;

        gridLayoutGroup.cellSize = new Vector2(cellSize, cellSize);
        gridLayoutGroup.spacing = new Vector2(cellSpacing, cellSpacing);
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = gridSize;

        int padding = Mathf.RoundToInt(gridPadding);
        gridLayoutGroup.padding = new RectOffset(padding, padding, padding, padding);

        Debug.Log($"Screen Width: {screenWidth}, Background Size: {backgroundSize}, Cell Size: {cellSize}, Padding: {padding}");
    }

    void OnRectTransformDimensionsChange()
    {
        if (backgroundRect != null && gridLayoutGroup != null)
        {
            AdjustGridSize();
        }
    }
}