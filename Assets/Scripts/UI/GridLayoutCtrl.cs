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

        // ★ 버튼들의 자식 이미지 크기 조정
        AdjustChildImages(cellSize);

        // Canvas 강제 업데이트
        Canvas.ForceUpdateCanvases();

        // ★★ NumBtn의 numImage 위치 업데이트 (한 프레임만 대기)
        StartCoroutine(UpdateNumImagePositions());

        Debug.Log($"Screen Width: {screenWidth}, Background Size: {backgroundSize}, Cell Size: {cellSize}, Padding: {padding}");
    }

    void AdjustChildImages(float cellSize)
    {
        // GridLayoutGroup의 모든 자식(버튼들)을 순회
        foreach (Transform button in gridLayoutGroup.transform)
        {
            // 버튼의 모든 자식 이미지를 순회
            foreach (Transform child in button)
            {
                RectTransform childRect = child.GetComponent<RectTransform>();
                if (childRect != null)
                {
                    // 자식 이미지의 크기를 부모(버튼)와 동일하게 설정
                    childRect.sizeDelta = new Vector2(cellSize, cellSize);

                    // 자식이 부모 중앙에 위치하도록 설정 (선택사항)
                    childRect.anchoredPosition = Vector2.zero;
                }
            }
        }
    }

    // ★★ NumBtn의 numImage들이 numImageLayer로 이동한 후 위치를 업데이트
    System.Collections.IEnumerator UpdateNumImagePositions()
    {
        // 한 프레임만 대기 (훨씬 빠름)
        yield return null;

        // 모든 NumBtn의 위치 업데이트
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