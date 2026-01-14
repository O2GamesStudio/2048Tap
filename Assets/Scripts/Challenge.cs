using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Challenge : MonoBehaviour
{
    [SerializeField] Button plusBtn, minusBtn;
    [SerializeField] TextMeshProUGUI challengeText;
    public int challengeNum;

    [Header("Challenge Settings")]
    [SerializeField] int maxChallengeNum = 4;
    [SerializeField] int minChallengeNum = 0;

    void Awake()
    {
        plusBtn.onClick.AddListener(PlusOnClick);
        minusBtn.onClick.AddListener(MinusOnClick);
    }

    void Start()
    {
        // 초기 UI 업데이트
        UpdateUI();
        UpdateButtonStates();
    }

    void UpdateUI()
    {
        if (challengeText != null)
        {
            challengeText.text = $"x{challengeNum}";
        }
    }

    void UpdateButtonStates()
    {
        if (plusBtn != null)
        {
            plusBtn.interactable = (challengeNum < maxChallengeNum);
        }

        if (minusBtn != null)
        {
            minusBtn.interactable = (challengeNum > minChallengeNum);
        }
    }

    void PlusOnClick()
    {
        if (challengeNum < maxChallengeNum)
        {
            challengeNum++;
            UpdateUI();
            UpdateButtonStates();

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayUIBtnClickSFX();
            }
        }
    }

    void MinusOnClick()
    {
        if (challengeNum > minChallengeNum)
        {
            challengeNum--;
            UpdateUI();
            UpdateButtonStates();

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayUIBtnClickSFX();
            }
        }
    }
    public void SetChallengeNum(int value)
    {
        challengeNum = Mathf.Clamp(value, minChallengeNum, maxChallengeNum);
        UpdateUI();
        UpdateButtonStates();
    }

    public int GetChallengeNum()
    {
        return challengeNum;
    }
}