using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    GameManager gameManager;

    [Header("Number Images")]
    [SerializeField] Image nowNumImage;
    [SerializeField] Image nextNumImage1; // 첫 번째 다음 숫자
    [SerializeField] Image nextNumImage2; // 두 번째 다음 숫자

    [Header("Texts")]
    [SerializeField] TextMeshProUGUI eraseCountText, restoreCountText;
    public TextMeshProUGUI nowScoreTxt;
    public TextMeshProUGUI finalScoreTxt;
    public TextMeshProUGUI highScoreTxt;

    [Header("Buttons")]
    [SerializeField] Button settingBtn;

    [SerializeField] GameObject settingPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        settingBtn.onClick.AddListener(SettingOnClick);
    }

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void UpdateUI()
    {
        // ★ 이미지 업데이트
        UpdateNumberImages();

        eraseCountText.text = gameManager.eraseCount.ToString();
        restoreCountText.text = gameManager.restoreCount.ToString();
    }

    // ★ 숫자 이미지 업데이트 메서드
    void UpdateNumberImages()
    {
        if (gameManager == null || gameManager.numberSprites == null) return;

        // nowNum 이미지 업데이트
        if (nowNumImage != null)
        {
            int nowIndex = gameManager.GetSpriteIndex(gameManager.nowNum);
            nowNumImage.sprite = gameManager.numberSprites[nowIndex];
        }

        // nextNum 이미지 업데이트
        if (nextNumImage1 != null)
        {
            int nextIndex1 = gameManager.GetSpriteIndex(gameManager.nextNum);
            nextNumImage1.sprite = gameManager.numberSprites[nextIndex1];
        }

        // nextNum2 이미지 업데이트
        if (nextNumImage2 != null)
        {
            int nextIndex2 = gameManager.GetSpriteIndex(gameManager.nextNum2);
            nextNumImage2.sprite = gameManager.numberSprites[nextIndex2];
        }
    }

    public void UpdateItemCount(int eraseCount, int restoreCount)
    {
        if (eraseCountText != null)
            eraseCountText.text = eraseCount.ToString();

        if (restoreCountText != null)
            restoreCountText.text = restoreCount.ToString();
    }

    void SettingOnClick()
    {
        settingPanel.SetActive(true);
    }
}