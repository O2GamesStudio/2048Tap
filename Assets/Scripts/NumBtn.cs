using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumBtn : MonoBehaviour
{
    Image bgImage;
    int num = 0;
    void Awake()
    {
        bgImage = GetComponent<Image>();
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
        if (num == 0)
        {
            bgImage.sprite = GameManager.Instance.numberSprites[0];
        }
        else if (num == 2)
        {
            bgImage.sprite = GameManager.Instance.numberSprites[1];
        }
        else if (num == 4)
        {
            bgImage.sprite = GameManager.Instance.numberSprites[2];
        }
        else if (num == 8)
        {
            bgImage.sprite = GameManager.Instance.numberSprites[3];
        }
        else if (num == 16)
        {
            bgImage.sprite = GameManager.Instance.numberSprites[4];
        }
        else if (num == 32)
        {
            bgImage.sprite = GameManager.Instance.numberSprites[5];
        }
        else if (num == 64)
        {
            bgImage.sprite = GameManager.Instance.numberSprites[6];
        }
        else if (num == 128)
        {
            bgImage.sprite = GameManager.Instance.numberSprites[7];
        }
        else if (num == 256)
        {
            bgImage.sprite = GameManager.Instance.numberSprites[8];
        }
    }
}
