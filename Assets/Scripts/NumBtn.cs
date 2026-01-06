using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumBtn : MonoBehaviour
{
    Image bgImage;
    Image numImage;
    int num = 0;

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
        if (numImage == null) return;

        if (num == 0)
        {
            numImage.sprite = GameManager.Instance.numberSprites[0];
        }
        else if (num == 2)
        {
            numImage.sprite = GameManager.Instance.numberSprites[1];
        }
        else if (num == 4)
        {
            numImage.sprite = GameManager.Instance.numberSprites[2];
        }
        else if (num == 8)
        {
            numImage.sprite = GameManager.Instance.numberSprites[3];
        }
        else if (num == 16)
        {
            numImage.sprite = GameManager.Instance.numberSprites[4];
        }
        else if (num == 32)
        {
            numImage.sprite = GameManager.Instance.numberSprites[5];
        }
        else if (num == 64)
        {
            numImage.sprite = GameManager.Instance.numberSprites[6];
        }
        else if (num == 128)
        {
            numImage.sprite = GameManager.Instance.numberSprites[7];
        }
        else if (num == 256)
        {
            numImage.sprite = GameManager.Instance.numberSprites[8];
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
}