using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Security;

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
    public TextMeshProUGUI plusScoreTxt;
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

    [Header("Combo System")]
    [SerializeField] private Image combo8Image;
    [SerializeField] private Image combo16Image;
    [SerializeField] private Image combo32Image;
    [SerializeField] private Image combo64Image;
    [SerializeField] private Image combo128Image;
    [SerializeField] private Image combo256Image;
    [SerializeField] private float comboDuration = 1.5f;
    [SerializeField] private float comboYOffset = 100f;
    [SerializeField] private float comboMoveDistance = 200f;

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

        InitializeComboImages();

        // plusScoreTxt 초기 비활성화
        if (plusScoreTxt != null)
        {
            plusScoreTxt.gameObject.SetActive(false);
        }
    }

    void InitializeComboImages()
    {
        if (combo8Image != null) combo8Image.gameObject.SetActive(false);
        if (combo16Image != null) combo16Image.gameObject.SetActive(false);
        if (combo32Image != null) combo32Image.gameObject.SetActive(false);
        if (combo64Image != null) combo64Image.gameObject.SetActive(false);
        if (combo128Image != null) combo128Image.gameObject.SetActive(false);
        if (combo256Image != null) combo256Image.gameObject.SetActive(false);
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

        sequence.Append(nowNumImage.DOFade(0f, numberShiftDuration));

        sequence.Join(nextNumImage1.transform.DOLocalMove(nowNumOriginalPos, numberShiftDuration));
        sequence.Join(nextNumImage1.transform.DOScale(nowNumOriginalScale * 1.5f, numberShiftDuration));

        sequence.Join(nextNumImage2.transform.DOLocalMove(nextNum1OriginalPos, numberShiftDuration));

        sequence.OnComplete(() =>
        {
            UpdateNumberImages();

            nowNumImage.transform.localPosition = nowNumOriginalPos;
            nextNumImage1.transform.localPosition = nextNum1OriginalPos;
            nextNumImage2.transform.localPosition = nextNum2OriginalPos;

            nowNumImage.transform.localScale = nowNumOriginalScale;
            nextNumImage1.transform.localScale = nextNum1OriginalScale;
            nextNumImage2.transform.localScale = nextNum2OriginalScale;

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
            color.a = eraseCount > 0 ? 1f : 0.3f;
            eraseBtnImage.color = color;
        }

        if (restoreBtnImage != null)
        {
            Color color = restoreBtnImage.color;
            color.a = restoreCount > 0 ? 1f : 0.3f;
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
            eraseBtnImage.color = isActive ? Color.red : Color.white;
        }
    }

    public void UpdatePlusScore(int score)
    {
        if (plusScoreTxt != null)
        {
            plusScoreTxt.text = "+" + score.ToString();
            plusScoreTxt.gameObject.SetActive(true);
        }
    }

    public void HidePlusScore()
    {
        if (plusScoreTxt != null)
        {
            plusScoreTxt.gameObject.SetActive(false);
        }
    }

    public void ShowCombo(int comboNumber, Vector3 worldPosition)
    {
        Image targetComboImage = GetComboImage(comboNumber);

        if (targetComboImage == null)
        {
            Debug.LogWarning($"Combo image for {comboNumber} not assigned!");
            return;
        }

        RectTransform comboRect = targetComboImage.GetComponent<RectTransform>();
        comboRect.DOKill();
        targetComboImage.DOKill();

        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransform canvasRect = targetComboImage.canvas.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            Camera.main,
            out localPoint
        );

        localPoint.y += comboYOffset;

        comboRect.localPosition = localPoint;
        comboRect.localScale = Vector3.zero;
        comboRect.localRotation = Quaternion.Euler(0, 0, 0);
        targetComboImage.color = new Color(1, 1, 1, 1);
        targetComboImage.gameObject.SetActive(true);

        comboRect.DOLocalMoveY(localPoint.y + comboMoveDistance, comboDuration).SetEase(Ease.OutQuad);
        Sequence sequence = DOTween.Sequence();

        sequence.Append(comboRect.DOScale(1.5f, 0.15f).SetEase(Ease.OutBack));
        sequence.Append(comboRect.DOScale(1.0f, 0.2f).SetEase(Ease.InOutSine));

        sequence.Insert(sequence.Duration(), targetComboImage.DOFade(0f, 0.3f).SetEase(Ease.InQuad));

        sequence.OnComplete(() => targetComboImage.gameObject.SetActive(false));
    }

    Image GetComboImage(int number)
    {
        switch (number)
        {
            case 8: return combo8Image;
            case 16: return combo16Image;
            case 32: return combo32Image;
            case 64: return combo64Image;
            case 128: return combo128Image;
            case 256: return combo256Image;
            default: return null;
        }
    }
}