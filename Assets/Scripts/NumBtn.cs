using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NumBtn : MonoBehaviour
{
    TextMeshProUGUI numText;
    Image bgImage;

    int num = 0;
    Color bgColor;
    void Awake()
    {
        numText = GetComponentInChildren<TextMeshProUGUI>();
        bgImage = GetComponent<Image>();
    }
    public int ReturnNum()
    {
        return num;
    }
    public void SetNumText(int _num)
    {
        num = _num;
        if (_num == 0) numText.text = "";
        else numText.text = _num.ToString();

        UpdateColor();
    }
    public void UpdateColor()
    {
        if (num == 0) numText.transform.DOScale(0, 0f);
        else numText.transform.DOScale(1, 0f);

        if (num == 0)
        {
            if (ColorUtility.TryParseHtmlString("#D6CDC4", out bgColor)) bgImage.color = bgColor;
        }
        else if (num == 2)
        {
            if (ColorUtility.TryParseHtmlString("#EEE4DA", out bgColor)) bgImage.color = bgColor;
        }
        else if (num == 4)
        {
            if (ColorUtility.TryParseHtmlString("#EDE0C8", out bgColor)) bgImage.color = bgColor;
        }
        else if (num == 8)
        {
            if (ColorUtility.TryParseHtmlString("#F2B178", out bgColor)) bgImage.color = bgColor;
        }
        else if (num == 16)
        {
            if (ColorUtility.TryParseHtmlString("#F59563", out bgColor)) bgImage.color = bgColor;
        }
        else if (num == 32)
        {
            if (ColorUtility.TryParseHtmlString("#F67B5F", out bgColor)) bgImage.color = bgColor;
        }
        else if (num == 64)
        {
            if (ColorUtility.TryParseHtmlString("#F65E3B", out bgColor)) bgImage.color = bgColor;
        }
        else if (num == 128)
        {
            if (ColorUtility.TryParseHtmlString("#EDCF71", out bgColor)) bgImage.color = bgColor;
        }
        else if (num == 256)
        {
            if (ColorUtility.TryParseHtmlString("#EDCC61", out bgColor)) bgImage.color = bgColor;
        }
    }
}
