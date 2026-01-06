using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumBtn : MonoBehaviour
{
    Image bgImage;
    Image numImage;
    int num = 0;
    INumberProvider numberProvider;

    // 모든 NumBtn의 numImage를 추적하기 위한 static 리스트
    private static Transform numImageLayer;

    void Awake()
    {
        bgImage = GetComponent<Image>();

        Image[] images = GetComponentsInChildren<Image>();
        if (images.Length >= 2)
        {
            numImage = images[1];
        }
        else
        {
            Debug.LogError("NumBtn에 자식 Image가 없습니다!");
        }

        if (GameManager.Instance != null)
            numberProvider = GameManager.Instance;
        else if (TutorialManager.Instance != null)
            numberProvider = TutorialManager.Instance;
    }

    void Start()
    {
        // 첫 실행 시 numImage 레이어 생성
        if (numImageLayer == null)
        {
            GameObject layerObj = new GameObject("NumImageLayer");
            numImageLayer = layerObj.transform;
            numImageLayer.SetParent(transform.parent.parent); // Grid의 부모로 설정

            RectTransform layerRect = layerObj.AddComponent<RectTransform>();
            layerRect.anchorMin = Vector2.zero;
            layerRect.anchorMax = Vector2.one;
            layerRect.sizeDelta = Vector2.zero;
            layerRect.anchoredPosition = Vector2.zero;

            // 맨 위로 이동
            numImageLayer.SetAsLastSibling();
        }

        // numImage를 numImageLayer로 이동
        if (numImage != null)
        {
            RectTransform numRect = numImage.GetComponent<RectTransform>();
            Vector3 worldPos = numRect.position;

            numRect.SetParent(numImageLayer, true);
            numRect.position = worldPos; // 위치 유지
        }
    }

    public int ReturnNum()
    {
        return num;
    }

    public void SetNumText(int _num)
    {
        num = _num;
        UpdateBtnImage();
    }

    public void UpdateBtnImage()
    {
        if (numberProvider == null)
        {
            if (GameManager.Instance != null)
                numberProvider = GameManager.Instance;
            else if (TutorialManager.Instance != null)
                numberProvider = TutorialManager.Instance;
        }

        if (numImage == null || numberProvider == null) return;

        if (num == 0)
        {
            numImage.sprite = numberProvider.numberSprites[0];
        }
        else if (num == 2)
        {
            numImage.sprite = numberProvider.numberSprites[1];
        }
        else if (num == 4)
        {
            numImage.sprite = numberProvider.numberSprites[2];
        }
        else if (num == 8)
        {
            numImage.sprite = numberProvider.numberSprites[3];
        }
        else if (num == 16)
        {
            numImage.sprite = numberProvider.numberSprites[4];
        }
        else if (num == 32)
        {
            numImage.sprite = numberProvider.numberSprites[5];
        }
        else if (num == 64)
        {
            numImage.sprite = numberProvider.numberSprites[6];
        }
        else if (num == 128)
        {
            numImage.sprite = numberProvider.numberSprites[7];
        }
        else if (num == 256)
        {
            numImage.sprite = numberProvider.numberSprites[8];
        }
    }

    public void MergeAnimationToTarget(Vector3 targetWorldPos, float duration = 0.3f, System.Action onComplete = null)
    {
        if (numImage == null) return;

        RectTransform numRect = numImage.GetComponent<RectTransform>();
        Vector3 startPos = numRect.position;

        // 애니메이션 중 살짝 뒤로 (형제 순서)
        numRect.SetAsFirstSibling();

        numRect.DOMove(targetWorldPos, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                numRect.position = startPos;

                // 원래 순서로 복원
                numRect.SetAsLastSibling();

                num = 0;
                UpdateBtnImage();

                onComplete?.Invoke();
            });
    }

    public void EraseAnimationToZero(float duration = 0.2f, System.Action onComplete = null)
    {
        if (numImage == null) return;

        Color originalColor = numImage.color;
        numImage.DOFade(0f, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                num = 0;
                UpdateBtnImage();

                numImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

                onComplete?.Invoke();
            });
    }
}