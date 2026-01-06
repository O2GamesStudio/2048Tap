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

        // ★ Awake에서 numberProvider 초기화
        if (GameManager.Instance != null)
            numberProvider = GameManager.Instance;
        else if (TutorialManager.Instance != null)
            numberProvider = TutorialManager.Instance;
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
        // ★ numberProvider가 없으면 다시 찾기
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

        Vector3 originalLocalPos = numRect.localPosition;
        Vector3 targetLocalPos = numRect.parent.InverseTransformPoint(targetWorldPos);

        numImage.transform.SetAsFirstSibling();
        numRect.DOLocalMove(targetLocalPos, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                numRect.localPosition = originalLocalPos;

                numImage.transform.SetAsLastSibling();

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