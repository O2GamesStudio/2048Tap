using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

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

    [Header("Ad Count UI")]
    [SerializeField] private GameObject eraseAdCountPanel;
    [SerializeField] private TextMeshProUGUI eraseAdCountText;
    [SerializeField] private GameObject restoreAdCountPanel;
    [SerializeField] private TextMeshProUGUI restoreAdCountText;

    [Header("Buttons")]
    [SerializeField] Button settingBtn;
    [SerializeField] Button eraseBtn;
    [SerializeField] Button restoreBtn;
    [SerializeField] Image eraseBtnImage;
    Image restoreBtnImage;
    [SerializeField] Color eraseToggleColor;

    [SerializeField] GameObject settingPanel;
    public GameObject numImageLayer;

    [Header("Animation Settings")]
    [SerializeField] float numberShiftDuration = 0.3f;
    private Vector3 nowNumOriginalPos;
    private Vector3 nextNum1OriginalPos;
    private Vector3 nextNum2OriginalPos;
    private Vector3 nowNumOriginalScale;
    private Vector3 nextNum1OriginalScale;
    private Vector3 nextNum2OriginalScale;

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


        nowNumOriginalPos = nowNumImage.transform.localPosition;
        nextNum1OriginalPos = nextNumImage1.transform.localPosition;
        nextNum2OriginalPos = nextNumImage2.transform.localPosition;

        nowNumOriginalScale = nowNumImage.transform.localScale;
        nextNum1OriginalScale = nextNumImage1.transform.localScale;
        nextNum2OriginalScale = nextNumImage2.transform.localScale;

        eraseBtnImage = eraseBtn.GetComponent<Image>();
        restoreBtnImage = restoreBtn.GetComponent<Image>();
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

    public float GetNumberShiftDuration()
    {
        return numberShiftDuration;
    }
    public void AnimateNumberShift()
    {
        Sequence sequence = DOTween.Sequence();

        // 1. nowNumImage 페이드 아웃
        sequence.Append(nowNumImage.DOFade(0f, numberShiftDuration));

        // 2. nextNumImage1이 nowNumImage 위치로 이동하면서 1.5배 커짐
        sequence.Join(nextNumImage1.transform.DOLocalMove(nowNumOriginalPos, numberShiftDuration));
        sequence.Join(nextNumImage1.transform.DOScale(nowNumOriginalScale * 1.5f, numberShiftDuration));

        // 3. nextNumImage2가 nextNumImage1 위치로 이동
        sequence.Join(nextNumImage2.transform.DOLocalMove(nextNum1OriginalPos, numberShiftDuration));

        // 4. 애니메이션 완료 후 처리
        sequence.OnComplete(() =>
        {
            // 스프라이트 업데이트
            UpdateNumberImages();

            // 원래 위치로 복원
            nowNumImage.transform.localPosition = nowNumOriginalPos;
            nextNumImage1.transform.localPosition = nextNum1OriginalPos;
            nextNumImage2.transform.localPosition = nextNum2OriginalPos;

            // 스케일 복원
            nowNumImage.transform.localScale = nowNumOriginalScale;
            nextNumImage1.transform.localScale = nextNum1OriginalScale;
            nextNumImage2.transform.localScale = nextNum2OriginalScale;

            // 알파값 복원
            nowNumImage.color = new Color(1, 1, 1, 1);
        });
    }

    void ResetImagePositions()
    {
        nowNumImage.color = new Color(1, 1, 1, 1);
    }

    public void UpdateItemCount(int eraseCount, int restoreCount, int remainingEraseAds, int remainingRestoreAds)
    {
        if (eraseCountText != null)
            eraseCountText.text = eraseCount.ToString();

        if (restoreCountText != null)
            restoreCountText.text = restoreCount.ToString();

        if (eraseBtn != null)
        {
            eraseBtn.interactable = eraseCount > 0 || remainingEraseAds > 0;
        }

        if (restoreBtn != null)
        {
            restoreBtn.interactable = restoreCount > 0 || remainingRestoreAds > 0;
        }

        if (eraseBtnImage != null)
        {
            Color color = eraseBtnImage.color;
            color.a = eraseCount > 0 ? 1f : 0.2f;
            eraseBtnImage.color = color;
        }

        if (restoreBtnImage != null)
        {
            Color color = restoreBtnImage.color;
            color.a = restoreCount > 0 ? 1f : 0.2f;
            restoreBtnImage.color = color;
        }
    }

    public void UpdateAdCountUI(int eraseCount, int restoreCount, int remainingEraseAds, int remainingRestoreAds)
    {
        if (eraseAdCountPanel != null && eraseAdCountText != null)
        {
            if (eraseCount == 0 && remainingEraseAds > 0)
            {
                eraseAdCountPanel.SetActive(true);
                eraseAdCountText.text = $"{remainingEraseAds}/3";

                if (eraseCountText != null)
                    eraseCountText.gameObject.SetActive(false);
            }
            else
            {
                eraseAdCountPanel.SetActive(false);
                if (eraseCountText != null)
                    eraseCountText.gameObject.SetActive(true);
            }
        }

        if (restoreAdCountPanel != null && restoreAdCountText != null)
        {
            if (restoreCount == 0 && remainingRestoreAds > 0)
            {
                restoreAdCountPanel.SetActive(true);
                restoreAdCountText.text = $"{remainingRestoreAds}/3";

                if (restoreCountText != null)
                    restoreCountText.gameObject.SetActive(false);
            }
            else
            {
                restoreAdCountPanel.SetActive(false);

                if (restoreCountText != null)
                    restoreCountText.gameObject.SetActive(true);
            }
        }
    }

    void SettingOnClick()
    {
        settingPanel.SetActive(true);
        SoundManager.Instance.PlayUIBtnClickSFX();
    }

    public void SetEraseModeVisual(bool isActive)
    {
        if (eraseBtnImage != null)
        {
            eraseBtnImage.color = isActive ? eraseToggleColor : Color.white;
        }
    }
}