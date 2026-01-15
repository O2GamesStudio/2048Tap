using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Challenge : MonoBehaviour
{
    [SerializeField] Button plusBtn, minusBtn;
    [SerializeField] Button startGameBtn;
    [SerializeField] TextMeshProUGUI challengeText;
    public int challengeNum = 1;

    [Header("Challenge Settings")]
    [SerializeField] int maxChallengeNum = 4;
    [SerializeField] int minChallengeNum = 1;

    [Header("Visual Feedback")]
    [SerializeField] GameObject[] crackImages;
    [SerializeField] Sprite[] tileSprites;

    private LobbyManager lobbyManager;

    void Awake()
    {
        plusBtn.onClick.AddListener(PlusOnClick);
        minusBtn.onClick.AddListener(MinusOnClick);
    }

    void Start()
    {
        if (challengeNum < minChallengeNum)
        {
            challengeNum = minChallengeNum;
        }

        UpdateUI();
        UpdateButtonStates();
        UpdateCrackImages(); // 초기 크랙 이미지 설정
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
        UpdateCrackImages();
    }

    void UpdateCrackImages()
    {
        if (crackImages == null || crackImages.Length == 0) return;
        if (tileSprites == null || tileSprites.Length < 2)
        {
            Debug.LogWarning("tileSprites가 부족합니다. 최소 2개 필요.");
            return;
        }

        for (int i = 0; i < crackImages.Length; i++)
        {
            if (crackImages[i] == null || !crackImages[i])
                continue;

            Image imageComponent = crackImages[i].GetComponent<Image>();

            if (imageComponent == null || !imageComponent)
                continue;

            imageComponent.sprite = (i < challengeNum) ? tileSprites[1] : tileSprites[0];
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
            Debug.Log("Plus");
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
            Debug.Log("minus");
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
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayUIBtnClickSFX();
        }
    }
}