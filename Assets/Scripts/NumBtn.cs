using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NumBtn : MonoBehaviour
{
    Image bgImage;
    Image numImage;
    int num = 0;
    INumberProvider numberProvider;
    private bool isLocked = false;

    private static Transform numImageLayer;

    private RectTransform originalParent;
    private RectTransform numRect;
    private Vector2 originalSizeDelta;

    void Awake()
    {
        Image[] images = GetComponentsInChildren<Image>();
        if (images.Length >= 2)
        {
            bgImage = images[1];
            numImage = images[2];
        }
        else
        {
            Debug.LogError("NumBtn에 자식 Image가 없습니다!");
        }

        if (GameManager.Instance != null)
            numberProvider = GameManager.Instance;
        else if (TutorialManager.Instance != null)
            numberProvider = TutorialManager.Instance;

        if (numImage != null)
        {
            numRect = numImage.GetComponent<RectTransform>();
            originalParent = numImage.transform.parent as RectTransform;
            originalSizeDelta = numRect.sizeDelta;
        }
    }

    void Start()
    {
        numImageLayer = UIManager.Instance.numImageLayer.transform;

        if (numImage != null && numImageLayer != null)
        {
            MoveToLayerImmediate();
        }

        if (numImageLayer != null)
        {
            numImageLayer.SetAsLastSibling();
        }
    }

    void MoveToLayerImmediate()
    {
        Canvas.ForceUpdateCanvases();

        Vector3 worldPos = numRect.position;
        Vector3 worldScale = numRect.lossyScale;
        Vector2 sizeDelta = numRect.sizeDelta;

        numRect.SetParent(numImageLayer, false);

        numRect.sizeDelta = sizeDelta;
        numRect.position = worldPos;

        Vector3 localPos = numRect.localPosition;
        numRect.localPosition = new Vector3(localPos.x, localPos.y, -1f);

        if (numImageLayer.lossyScale != Vector3.zero)
        {
            Vector3 localScaleAdjustment = new Vector3(
                worldScale.x / numImageLayer.lossyScale.x,
                worldScale.y / numImageLayer.lossyScale.y,
                worldScale.z / numImageLayer.lossyScale.z
            );
            numRect.localScale = localScaleAdjustment;
        }

        StartCoroutine(VerifyPositionNextFrame());
    }

    IEnumerator VerifyPositionNextFrame()
    {
        yield return null;

        if (originalParent != null)
        {
            numRect.position = originalParent.position;

            Vector3 localPos = numRect.localPosition;
            numRect.localPosition = new Vector3(localPos.x, localPos.y, -1f);
        }
    }

    public void UpdateImagePosition()
    {
        if (numImage == null || originalParent == null) return;

        numRect.sizeDelta = originalParent.sizeDelta;
        numRect.position = originalParent.position;

        Vector3 localPos = numRect.localPosition;
        numRect.localPosition = new Vector3(localPos.x, localPos.y, -1f);

        if (numImageLayer != null && numImageLayer.lossyScale != Vector3.zero)
        {
            Vector3 worldScale = originalParent.lossyScale;
            Vector3 localScaleAdjustment = new Vector3(
                worldScale.x / numImageLayer.lossyScale.x,
                worldScale.y / numImageLayer.lossyScale.y,
                worldScale.z / numImageLayer.lossyScale.z
            );
            numRect.localScale = localScaleAdjustment;
        }
    }

    public int ReturnNum()
    {
        return num;
    }

    public bool IsLocked()
    {
        return isLocked;
    }

    public void LockButton()
    {
        isLocked = true;
        num = -1;
        UpdateBtnImage();
    }

    public void UnlockButton()
    {
        isLocked = false;
        num = 0;
        UpdateBtnImage();
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

        // 잠긴 버튼인 경우 blockSprite 사용
        if (isLocked || num == -1)
        {
            if (GameManager.Instance != null && GameManager.Instance.blockSprite != null)
            {
                numImage.sprite = GameManager.Instance.blockSprite;
            }
            return;
        }

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

        Vector3 startPos = numRect.position;

        numRect.SetAsFirstSibling();

        Vector3 localPos = numRect.localPosition;
        numRect.localPosition = new Vector3(localPos.x, localPos.y, -2f);

        Sequence moveSequence = DOTween.Sequence();
        moveSequence.Append(numRect.DOMoveX(targetWorldPos.x, duration));
        moveSequence.Join(numRect.DOMoveY(targetWorldPos.y, duration));
        moveSequence.SetEase(Ease.InOutQuad);
        moveSequence.OnComplete(() =>
        {
            if (originalParent != null)
            {
                numRect.position = originalParent.position;

                Vector3 finalPos = numRect.localPosition;
                numRect.localPosition = new Vector3(finalPos.x, finalPos.y, -1f);
            }

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