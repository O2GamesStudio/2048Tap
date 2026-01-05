using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    GameManager gameManager;

    [Header("Texts")]
    [SerializeField] TextMeshProUGUI nowNumText;
    [SerializeField] TextMeshProUGUI nextNumText;
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
        nowNumText.text = gameManager.nowNum.ToString();
        nextNumText.text = gameManager.nextNum.ToString();

        eraseCountText.text = gameManager.eraseCount.ToString();
        restoreCountText.text = gameManager.restoreCount.ToString();
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
