using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    INumberProvider numberProvider;

    [Header("Number Images")]
    [SerializeField] Image nowNumImage;
    [SerializeField] Image nextNumImage1;
    [SerializeField] Image nextNumImage2;

    [Header("Texts")]
    [SerializeField] TextMeshProUGUI eraseCountText, restoreCountText;
    public TextMeshProUGUI nowScoreTxt;
    public TextMeshProUGUI finalScoreTxt;
    public TextMeshProUGUI highScoreTxt;

    [Header("Buttons")]
    [SerializeField] Button settingBtn;

    [SerializeField] GameObject settingPanel;
    public GameObject numImageLayer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        settingBtn.onClick.AddListener(SettingOnClick);
    }

    void Start()
    {
        if (GameManager.Instance != null)
            numberProvider = GameManager.Instance;
        else if (TutorialManager.Instance != null)
            numberProvider = TutorialManager.Instance;
    }

    public void UpdateUI()
    {
        UpdateNumberImages();

        if (numberProvider != null)
        {
            eraseCountText.text = numberProvider.eraseCount.ToString();
            restoreCountText.text = numberProvider.restoreCount.ToString();
        }
    }

    void UpdateNumberImages()
    {
        if (numberProvider == null || numberProvider.numberSprites == null) return;

        if (nowNumImage != null)
        {
            int nowIndex = numberProvider.GetSpriteIndex(numberProvider.nowNum);
            nowNumImage.sprite = numberProvider.numberSprites[nowIndex];
        }

        if (nextNumImage1 != null)
        {
            int nextIndex1 = numberProvider.GetSpriteIndex(numberProvider.nextNum);
            nextNumImage1.sprite = numberProvider.numberSprites[nextIndex1];
        }

        if (nextNumImage2 != null)
        {
            int nextIndex2 = numberProvider.GetSpriteIndex(numberProvider.nextNum2);
            nextNumImage2.sprite = numberProvider.numberSprites[nextIndex2];
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
        SoundManager.Instance.PlayUIBtnClickSFX();
    }
}