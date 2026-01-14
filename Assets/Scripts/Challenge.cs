using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Challenge : MonoBehaviour
{
    [SerializeField] Button plusBtn, minusBtn;
    [SerializeField] Button startGameBtn;
    [SerializeField] TextMeshProUGUI challengeText;
    public int challengeNum;

    [Header("Challenge Settings")]
    [SerializeField] int maxChallengeNum = 4;
    [SerializeField] int minChallengeNum = 0;

    private LobbyManager lobbyManager;

    void Awake()
    {
        plusBtn.onClick.AddListener(PlusOnClick);
        minusBtn.onClick.AddListener(MinusOnClick);

        if (startGameBtn != null)
        {
            startGameBtn.onClick.AddListener(OnStartGameClick);
        }
    }

    void Start()
    {
        lobbyManager = FindObjectOfType<LobbyManager>();
        UpdateUI();
        UpdateButtonStates();
    }

    public void SetLobbyManager(LobbyManager manager)
    {
        lobbyManager = manager;
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

            if (lobbyManager != null)
            {
                lobbyManager.OnChallengeChanged();
            }

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

            if (lobbyManager != null)
            {
                lobbyManager.OnChallengeChanged();
            }

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

        if (lobbyManager != null)
        {
            lobbyManager.OnChallengeChanged();
        }
    }

    public int GetChallengeNum()
    {
        return challengeNum;
    }

    public void OnStartGameClick()
    {
        GameDataTransfer.SetChallengeNum(challengeNum);
        Debug.Log($"Starting game with challenge number: {challengeNum}");

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIBtnClickSFX();
        }
    }
}