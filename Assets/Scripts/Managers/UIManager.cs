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
    [SerializeField] private GameObject eraseAdCountPanel;      // 지우기 광고 카운트 패널
    [SerializeField] private TextMeshProUGUI eraseAdCountText;  // 지우기 광고 카운트 텍스트
    [SerializeField] private GameObject restoreAdCountPanel;    // 복원 광고 카운트 패널
    [SerializeField] private TextMeshProUGUI restoreAdCountText;// 복원 광고 카운트 텍스트

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
    [SerializeField] private float comboYOffset = 100f; // 버튼 위에서 시작할 Y 오프셋
    [SerializeField] private float comboMoveDistance = 200f; // 위로 이동할 거리

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

        // 콤보 이미지 초기화
        InitializeComboImages();
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

        // Erase 버튼 처리
        if (eraseBtn != null)
        {
            // 아이템이 있거나 광고를 볼 수 있으면 활성화
            eraseBtn.interactable = eraseCount > 0 || remainingEraseAds > 0;
        }

        // Restore 버튼 처리
        if (restoreBtn != null)
        {
            // 아이템이 있거나 광고를 볼 수 있으면 활성화
            restoreBtn.interactable = restoreCount > 0 || remainingRestoreAds > 0;
        }

        // Erase 버튼 이미지 알파값 조정
        if (eraseBtnImage != null)
        {
            Color color = eraseBtnImage.color;
            // 아이템이 있으면 불투명(1.0), 없으면 반투명(0.3)
            color.a = eraseCount > 0 ? 1f : 0.3f;
            eraseBtnImage.color = color;
        }

        // Restore 버튼 이미지 알파값 조정
        if (restoreBtnImage != null)
        {
            Color color = restoreBtnImage.color;
            // 아이템이 있으면 불투명(1.0), 없으면 반투명(0.3)
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

    public void ShowCombo(int comboNumber, Vector3 worldPosition)
    {
        Image targetComboImage = GetComboImage(comboNumber);

        if (targetComboImage == null)
        {
            Debug.LogWarning($"Combo image for {comboNumber} not assigned!");
            return;
        }

        // 월드 좌표를 스크린 좌표로 변환
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);

        // 스크린 좌표를 Canvas의 로컬 좌표로 변환
        RectTransform canvasRect = targetComboImage.canvas.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            Camera.main,
            out localPoint
        );

        // Y 오프셋 적용
        localPoint.y += comboYOffset;

        // 콤보 이미지 초기화
        RectTransform comboRect = targetComboImage.GetComponent<RectTransform>();
        comboRect.localPosition = localPoint;
        comboRect.localScale = Vector3.zero; // 0부터 시작
        comboRect.localRotation = Quaternion.Euler(0, 0, 0);
        targetComboImage.color = new Color(1, 1, 1, 1);
        targetComboImage.gameObject.SetActive(true);

        // 애니메이션 시퀀스
        Sequence sequence = DOTween.Sequence();

        comboRect.DOLocalMoveY(localPoint.y + comboMoveDistance, 1.5f).SetEase(Ease.OutExpo);
        sequence.Append(comboRect.DOScale(1.3f, 0.15f).SetEase(Ease.OutBack));
        sequence.Append(comboRect.DOScale(1.0f, 0.1f).SetEase(Ease.InOutSine));

        // 페이드 아웃 (마지막 0.5초 동안)
        sequence.Insert(sequence.Duration() - 0.5f, targetComboImage.DOFade(0f, 1.2f).SetEase(Ease.InQuad));
        // 애니메이션 완료 후 비활성화
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