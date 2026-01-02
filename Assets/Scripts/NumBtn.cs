using System.Security;
using DG.Tweening;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;

public class NumBtn : MonoBehaviour
{
    TextMeshProUGUI numText;
    [SerializeField] Image bgImage;
    int num = 0;

    void Awake()
    {
        numText = GetComponentInChildren<TextMeshProUGUI>();
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

        if (num == 2)
        {

        }
        else if (num == 4)
        {

        }
        else if (num == 8)
        {

        }
        else if (num == 16)
        {

        }
        else if (num == 32)
        {

        }
        else if (num == 64)
        {

        }
        else if (num == 128)
        {

        }
        else if (num == 256)
        {

        }
    }
}
