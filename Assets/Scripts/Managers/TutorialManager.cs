using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour, INumberProvider
{
    public static TutorialManager Instance { get; private set; }
    UIManager uiManager;

    [Header("Grid Settings")]
    [SerializeField] int gridSize = 5;
    private int totalCells;

    public Sprite[] numberSprites { get { return _numberSprites; } }
    [SerializeField] private Sprite[] _numberSprites;

    public int nowNum { get; private set; }
    public int nextNum { get; private set; }
    public int nextNum2 { get; private set; }
    [HideInInspector] public int eraseCount { get; private set; }
    [HideInInspector] public int restoreCount { get; private set; }
    int nowScore = 2;
    int highScore = 8;

    int[] numSet;
    bool[] isFilled;

    [SerializeField] NumBtn[] numBtns;

    [Header("Animation Settings")]
    [SerializeField] float btnCombineTime = 0.2f;
    [SerializeField] float combineDelayTime = 0.05f;

    bool[] visited;
    int[] sameSpaceNum;
    int sameCount = 0;
    int clickedPos;
    private bool isAnimating = false;

    [Header("Tutorial UI")]
    [SerializeField] GameObject tutorialOverlay;
    [SerializeField] TextMeshProUGUI tutorialText;
    [SerializeField] GameObject fingerIcon;
    [SerializeField] Image[] buttonHighlights;
    [SerializeField] CanvasGroup blockingPanel;

    private int currentStep = 0;
    private int[] fixedStartLayout = new int[]
    {
        2, 0, 0, 0,
        2, 0, 0, 0,
        0, 0, 0, 0,
        0, 0, 0, 0
    };

    private struct TutorialStep
    {
        public string message;
        public int[] allowedClickIndices;
        public bool showFinger;
        public int fingerTargetIndex;
        public int[] highlightIndices;
        public bool waitForAction;
        public float autoDelayTime;

        public TutorialStep(string msg, int[] allowedIndices = null, bool finger = false, int fingerTarget = -1, int[] highlights = null, bool waitAction = true, float autoDelay = 0f)
        {
            message = msg;
            allowedClickIndices = allowedIndices;
            showFinger = finger;
            fingerTargetIndex = fingerTarget;
            highlightIndices = highlights;
            waitForAction = waitAction;
            autoDelayTime = autoDelay;
        }
    }

    private List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        totalCells = gridSize * gridSize;
        numSet = new int[totalCells];
        isFilled = new bool[totalCells];
        visited = new bool[totalCells + 5];
        sameSpaceNum = new int[totalCells + 5];

        InitTutorialSteps();
    }

    void Start()
    {
        uiManager = UIManager.Instance;

        if (numBtns.Length != totalCells)
        {
            Debug.LogError($"NumBtn 배열 크기({numBtns.Length})가 그리드 크기({totalCells})와 맞지 않습니다!");
            return;
        }

        InitTutorial();
        ShowTutorialStep();
    }

    void InitTutorialSteps()
    {
        tutorialSteps.Add(new TutorialStep(
            "같은 숫자 3개를 붙여서 합치는 게임입니다!",
            null, false, -1, null, false, 2.5f
        ));

        tutorialSteps.Add(new TutorialStep(
        "2가 2개 있는 옆에 2를 놓아보세요!",
        new int[] { 1 }, true, 1, new int[] { 1 }, true, 0f
        ));

        tutorialSteps.Add(new TutorialStep(
            "잘했어요! 3개가 모여서 4가 되었습니다!",
            null, false, -1, null, false, 2f
        ));

        tutorialSteps.Add(new TutorialStep(
            "이번엔 원하는 곳에 놓아보세요!",
            new int[] { 1, 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, false, -1, new int[] { 1, 3, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 }, true, 0f
        ));

        tutorialSteps.Add(new TutorialStep(
            "훌륭해요! 상단의 다음 숫자를 확인하면\n더 전략적으로 플레이할 수 있어요!",
            null, false, -1, null, false, 3f
        ));

        tutorialSteps.Add(new TutorialStep(
            "튜토리얼 완료!\n이제 본 게임을 시작해보세요!",
            null, false, -1, null, false, 2f
        ));
    }

    void InitTutorial()
    {
        for (int a = 0; a < totalCells; a++)
        {
            numSet[a] = fixedStartLayout[a];
            numBtns[a].SetNumText(fixedStartLayout[a]);
            visited[a] = false;
            isFilled[a] = fixedStartLayout[a] != 0;
        }

        nowNum = 2;
        nextNum = 2;
        nextNum2 = 4;

        for (int i = 0; i < numBtns.Length; i++)
        {
            int index = i;
            numBtns[i].GetComponentInChildren<Button>().onClick.AddListener(() => BtnOnClicked(index));
        }

        if (uiManager != null)
            uiManager.UpdateUI();
    }

    void ShowTutorialStep()
    {
        if (currentStep >= tutorialSteps.Count)
        {
            EndTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[currentStep];

        foreach (var btn in numBtns)
        {
            btn.gameObject.SetActive(true);
        }

        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(false);

        if (tutorialText != null)
        {
            tutorialText.gameObject.SetActive(true);
            tutorialText.text = step.message;
        }

        // ★ 오버레이는 첫 단계에서만 활성화
        if (tutorialOverlay != null)
        {
            if (currentStep == 0)
                tutorialOverlay.SetActive(true);
            else
                tutorialOverlay.SetActive(false);
        }

        if (fingerIcon != null)
        {
            fingerIcon.SetActive(step.showFinger);
            if (step.showFinger && step.fingerTargetIndex >= 0)
            {
                fingerIcon.transform.position = numBtns[step.fingerTargetIndex].transform.position + Vector3.up * 100f;
                StartCoroutine(AnimateFinger());
            }
        }

        ClearHighlights();
        if (step.highlightIndices != null)
        {
            foreach (int index in step.highlightIndices)
            {
                if (index >= 0 && index < buttonHighlights.Length)
                {
                    buttonHighlights[index].gameObject.SetActive(true);
                }
            }
        }

        SetButtonsInteractable(step.allowedClickIndices);

        if (!step.waitForAction && step.autoDelayTime > 0f)
        {
            StartCoroutine(AutoProgressStep(step.autoDelayTime));
        }
    }

    void SetButtonsInteractable(int[] allowedIndices)
    {
        if (allowedIndices == null)
        {
            foreach (var btn in numBtns)
            {
                btn.GetComponentInChildren<Button>().interactable = false;
            }
        }
        else
        {
            foreach (var btn in numBtns)
            {
                btn.GetComponentInChildren<Button>().interactable = true;
            }
        }
    }

    IEnumerator AnimateFinger()
    {
        Vector3 startPos = fingerIcon.transform.position;
        float time = 0f;

        while (fingerIcon.activeSelf)
        {
            time += Time.deltaTime * 2f;
            float offset = Mathf.Sin(time) * 20f;
            fingerIcon.transform.position = startPos + Vector3.up * offset;
            yield return null;
        }
    }

    IEnumerator AutoProgressStep(float delay)
    {
        yield return new WaitForSeconds(delay);
        NextTutorialStep();
    }

    void ClearHighlights()
    {
        if (buttonHighlights != null)
        {
            foreach (var highlight in buttonHighlights)
            {
                highlight.gameObject.SetActive(false);
            }
        }
    }

    void NextTutorialStep()
    {
        currentStep++;
        ShowTutorialStep();
    }

    void EndTutorial()
    {
        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(false);

        ClearHighlights();

        UnityEngine.SceneManagement.SceneManager.LoadScene("LobbyScene");
    }

    public void BtnOnClicked(int index)
    {
        if (isAnimating) return;

        TutorialStep step = tutorialSteps[currentStep];

        if (step.allowedClickIndices != null && !System.Array.Exists(step.allowedClickIndices, x => x == index))
        {
            Debug.Log("이곳은 클릭할 수 없습니다!");
            return;
        }

        if (numBtns[index].ReturnNum() == 0)
        {
            numBtns[index].SetNumText(nowNum);
            clickedPos = index;
            numSet[index] = nowNum;
            sameSpaceNum[0] = clickedPos;

            StartCoroutine(FindSameNumCoroutine(clickedPos));

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayBtnClickSFX();

            if (step.waitForAction)
            {
                StartCoroutine(WaitForMergeAndProgress());
            }
        }
        else
        {
            Debug.Log("이미 채워진 칸입니다!");
        }

        numBtns[index].UpdateBtnImage();
    }

    IEnumerator WaitForMergeAndProgress()
    {
        yield return new WaitUntil(() => !isAnimating);
        yield return new WaitForSeconds(0.5f);
        NextTutorialStep();
    }

    void UpdateInfo()
    {
        if (uiManager != null)
        {
            uiManager.UpdateUI();
            uiManager.nowScoreTxt.text = nowScore.ToString();
        }
    }

    void ShiftNextNums()
    {
        nowNum = nextNum;
        nextNum = nextNum2;
        nextNum2 = 2;
    }

    void TestFind(int pos)
    {
        visited[pos] = true;
        int sameCountTemp = 0;
        int row = pos / gridSize;
        int col = pos % gridSize;

        if (col > 0 && !visited[pos - 1] && numBtns[pos - 1].ReturnNum() == nowNum)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos - 1] = true;
            sameSpaceNum[sameCount] = pos - 1;
        }
        if (col < gridSize - 1 && !visited[pos + 1] && numBtns[pos + 1].ReturnNum() == nowNum)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos + 1] = true;
            sameSpaceNum[sameCount] = pos + 1;
        }
        if (row > 0 && !visited[pos - gridSize] && numBtns[pos - gridSize].ReturnNum() == nowNum)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos - gridSize] = true;
            sameSpaceNum[sameCount] = pos - gridSize;
        }
        if (row < gridSize - 1 && !visited[pos + gridSize] && numBtns[pos + gridSize].ReturnNum() == nowNum)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos + gridSize] = true;
            sameSpaceNum[sameCount] = pos + gridSize;
        }

        for (int turn = 1; turn <= sameCountTemp; turn++)
        {
            TestFind(sameSpaceNum[turn]);
        }
    }

    IEnumerator FindSameNumCoroutine(int pos)
    {
        isAnimating = true;

        TestFind(pos);

        if (sameCount >= 2)
        {
            yield return StartCoroutine(MergeSequence(pos));
        }
        else
        {
            if (highScore < nowNum)
            {
                highScore = nowNum;
                if (uiManager != null)
                    uiManager.nowScoreTxt.text = nowNum.ToString();
            }
            nowScore += nowNum;

            ResetValue();
            clickedPos = 0;
        }

        ShiftNextNums();
        UpdateInfo();

        isAnimating = false;
    }

    IEnumerator MergeSequence(int pos)
    {
        bool wasMerged = false;

        while (sameCount >= 2)
        {
            wasMerged = true;

            Vector3 targetPos = numBtns[clickedPos].transform.position;

            for (int b = 0; b <= sameCount; b++)
            {
                int currentIndex = sameSpaceNum[b];

                if (currentIndex != clickedPos)
                {
                    numBtns[currentIndex].MergeAnimationToTarget(targetPos, btnCombineTime, () =>
                    {
                        numSet[currentIndex] = 0;
                    });
                }
            }

            yield return new WaitForSeconds(btnCombineTime);

            numSet[clickedPos] = 0;
            numBtns[clickedPos].SetNumText(0);

            nowNum *= 2;
            numSet[clickedPos] = nowNum;
            numBtns[clickedPos].SetNumText(nowNum);

            int tempClickedPos = clickedPos;
            ResetValue();
            sameSpaceNum[0] = tempClickedPos;

            TestFind(pos);

            if (sameCount < 2)
                break;

            yield return new WaitForSeconds(combineDelayTime);
        }

        if (wasMerged && nowNum >= 16 && VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayCombineParticle(numBtns[clickedPos].transform, nowNum);
        }

        if (highScore < nowNum)
        {
            highScore = nowNum;
            if (uiManager != null)
                uiManager.nowScoreTxt.text = nowNum.ToString();
        }
        nowScore += nowNum;

        ResetValue();
        clickedPos = 0;
    }

    void ResetValue()
    {
        for (int a = 0; a < totalCells + 5; a++)
        {
            sameSpaceNum[a] = 0;
            visited[a] = false;
        }
        sameCount = 0;
    }

    public int GetSpriteIndex(int number)
    {
        switch (number)
        {
            case 2: return 1;
            case 4: return 2;
            case 8: return 3;
            case 16: return 4;
            case 32: return 5;
            case 64: return 6;
            case 128: return 7;
            case 256: return 8;
            default: return 0;
        }
    }
}