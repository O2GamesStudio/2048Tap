using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Challenge : MonoBehaviour
{
    [SerializeField] Button plusBtn, minusBtn;
    [SerializeField] Button startGameBtn; // 게임 시작 버튼 (옵션, Inspector에서 설정)
    [SerializeField] TextMeshProUGUI challengeText;
    public int challengeNum;

    [Header("Challenge Settings")]
    [SerializeField] int maxChallengeNum = 4;
    [SerializeField] int minChallengeNum = 0;

    void Awake()
    {
        plusBtn.onClick.AddListener(PlusOnClick);
        minusBtn.onClick.AddListener(MinusOnClick);

        // 게임 시작 버튼이 있으면 리스너 추가
        if (startGameBtn != null)
        {
            startGameBtn.onClick.AddListener(OnStartGameClick);
        }
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

    // 게임 시작 버튼 클릭 시 호출되는 메서드
    // 다른 스크립트에서도 이 메서드를 호출할 수 있습니다
    public void OnStartGameClick()
    {
        // GameDataTransfer에 challengeNum 저장
        GameDataTransfer.SetChallengeNum(challengeNum);

        Debug.Log($"Starting game with challenge number: {challengeNum}");

        // 게임 씬으로 전환 (씬 이름은 프로젝트에 맞게 수정하세요)
        // SceneManager.LoadScene("GameScene");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIBtnClickSFX();
        }
    }
}