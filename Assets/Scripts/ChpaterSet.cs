using TMPro;
using UnityEngine;

public class ChapterSet : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI[] stageConditionText;
    [SerializeField] ChapterUnlockConfig unlockConfig;

    void Start()
    {
        UpdateConditionUI();
    }

    public void UpdateConditionUI()
    {
        if (stageConditionText == null || stageConditionText.Length < 2)
        {
            Debug.LogWarning("StageConditionText array is not properly set!");
            return;
        }

        bool isKorean = Application.systemLanguage == SystemLanguage.Korean;

        if (stageConditionText.Length > 0 && stageConditionText[0] != null)
        {
            int chapter1UnlockScore = unlockConfig != null ? unlockConfig.chapter1UnlockScore : 1000;

            if (isKorean)
            {
                stageConditionText[0].text = $"{chapter1UnlockScore}점 달성";
            }
            else
            {
                stageConditionText[0].text = $"Score {chapter1UnlockScore}";
            }
        }

        if (stageConditionText.Length > 1 && stageConditionText[1] != null)
        {
            int chapter2UnlockScore = unlockConfig != null ? unlockConfig.chapter2UnlockScore : 7000;

            if (isKorean)
            {
                stageConditionText[1].text = $"{chapter2UnlockScore}점 달성";
            }
            else
            {
                stageConditionText[1].text = $"Score {chapter2UnlockScore}";
            }
        }
    }
}