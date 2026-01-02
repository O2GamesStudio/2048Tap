using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("Button UI")]
    [SerializeField] Button gameStartBtn;
    [SerializeField] Button preChapterBtn, nextChapterBtn;

    [Header("Image UI")]
    [SerializeField] Image[] chapterImages;

    [Header("Text UI")]
    [SerializeField] TextMeshProUGUI chapterText;

    [Header("Animation Settings")]
    [SerializeField] float slideDistance = 1000f;
    [SerializeField] float slideDuration = 0.5f;
    [SerializeField] Ease slideEase = Ease.OutCubic;
    [SerializeField] float scaleDuration = 0.3f;
    [SerializeField] float centerScale = 1f;
    [SerializeField] float sideScale = 0.7f;
    [SerializeField] Ease scaleEase = Ease.InOutSine;

    int chapterNum = 0;
    [SerializeField] int maxChapterNum = 1;

    bool isAnimating = false;

    void Awake()
    {
        gameStartBtn.onClick.AddListener(() => StartOnClick());
        preChapterBtn.onClick.AddListener(() => ChapterMoveOnClick(-1));
        nextChapterBtn.onClick.AddListener(() => ChapterMoveOnClick(1));

        // 초기 위치 및 스케일 설정
        InitializeChapterPositions();
    }

    void InitializeChapterPositions()
    {
        for (int i = 0; i < chapterImages.Length; i++)
        {
            RectTransform rect = chapterImages[i].GetComponent<RectTransform>();
            if (i == chapterNum)
            {
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one * centerScale;
            }
            else if (i < chapterNum)
            {
                rect.anchoredPosition = new Vector2(-slideDistance, 0);
                rect.localScale = Vector3.one * sideScale;
            }
            else
            {
                rect.anchoredPosition = new Vector2(slideDistance, 0);
                rect.localScale = Vector3.one * sideScale;
            }
        }
    }

    void ChapterMoveOnClick(int dir)
    {
        if (isAnimating) return;

        if (dir == -1) //이전 챕터로 이동
        {
            if (chapterNum == 0) return;
            SlideChapters(chapterNum, chapterNum - 1);
            chapterNum--;
            UpdateChapterTextUI(chapterNum);
        }
        else if (dir == 1) // 다음 챕터로 이동
        {
            if (chapterNum == maxChapterNum) return;
            SlideChapters(chapterNum, chapterNum + 1);
            chapterNum++;
            UpdateChapterTextUI(chapterNum);
        }
    }

    void SlideChapters(int fromIndex, int toIndex)
    {
        isAnimating = true;

        RectTransform fromRect = chapterImages[fromIndex].GetComponent<RectTransform>();
        RectTransform toRect = chapterImages[toIndex].GetComponent<RectTransform>();

        // 이동 방향 결정
        float fromEndX = toIndex > fromIndex ? -slideDistance : slideDistance;

        // 메인 Sequence 생성
        Sequence mainSequence = DOTween.Sequence();

        // 1단계: 중앙 이미지 스케일 축소
        mainSequence.Append(
            fromRect.DOScale(sideScale, scaleDuration).SetEase(scaleEase)
        );

        // 2단계: 위치 이동 애니메이션 (스케일 축소 후)
        mainSequence.Append(
            fromRect.DOAnchorPosX(fromEndX, slideDuration).SetEase(slideEase)
        );
        mainSequence.Join(
            toRect.DOAnchorPosX(0, slideDuration).SetEase(slideEase)
        );

        // 3단계: 중앙으로 온 이미지 스케일 확대 (이동 완료 후)
        mainSequence.Append(
            toRect.DOScale(centerScale, scaleDuration).SetEase(scaleEase)
        );

        mainSequence.OnComplete(() => isAnimating = false);
    }

    void UpdateChapterTextUI(int chapterNum)
    {
        if (chapterNum == 0) chapterText.text = "5x5";
        else if (chapterNum == 1) chapterText.text = "4x4";
    }

    void StartOnClick()
    {
        SceneManager.LoadScene(chapterNum + 1);
    }

    void OnDestroy()
    {
        // DOTween 정리
        DOTween.Kill(this);
    }
}