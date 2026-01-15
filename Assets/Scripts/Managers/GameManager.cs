using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour, INumberProvider
{
    public static GameManager Instance { get; private set; }
    SoundManager soundManager;
    UIManager uiManager;
    public Tutorial tutorial;

    [Header("Grid Settings")]
    [SerializeField] int gridSize = 5;
    private int totalCells;

    public Sprite[] numberSprites { get { return _numberSprites; } }
    public Sprite blockSprite;
    [SerializeField] private Sprite[] _numberSprites;

    [HideInInspector] public int nowNum { get; private set; }
    [HideInInspector] public int nextNum { get; private set; }
    [HideInInspector] public int nextNum2 { get; private set; }
    int nowScore = 0;
    int highScore = 8;

    int[] numSet;
    int[] RanNumVal = new int[] { 2, 2, 2, 2, 2, 4, 4, 8 };
    bool[] isFilled;

    [SerializeField] NumBtn[] numBtns;

    [Header("Items")]
    [SerializeField] Button eraseBtn;
    [SerializeField] Button restoreBtn;
    [HideInInspector] public int eraseCount { get; private set; }
    [HideInInspector] public int restoreCount { get; private set; }

    private int eraseAdWatchCount = 0;
    private int restoreAdWatchCount = 0;
    private const int MAX_AD_WATCH_PER_ITEM = 3;

    public int GetRemainingEraseAds() { return MAX_AD_WATCH_PER_ITEM - eraseAdWatchCount; }
    public int GetRemainingRestoreAds() { return MAX_AD_WATCH_PER_ITEM - restoreAdWatchCount; }

    private bool isEraseMode = false;
    private Stack<GameState> actionHistory = new Stack<GameState>();

    [Header("Animation Settings")]
    [SerializeField] float btnCombineTime = 0.2f;
    [SerializeField] float combineDelayTime = 0.05f;
    private bool isAnimating = false;

    [SerializeField] GameOverPanel gameOverPanel;
    private const int MAX_HISTORY_SIZE = 20;

    private int comboCount = 0;

    private struct GameState
    {
        public int[] numSetCopy;
        public int nowScore;
        public int highScore;
        public int nowNum;
        public int nextNum;
        public int nextNum2;
        public int eraseCount;
        public int gridSize;

        public GameState(int[] numSet, int score, int high, int now, int next, int next2, int erase, int size)
        {
            gridSize = size;
            int totalCells = size * size;
            numSetCopy = new int[totalCells];
            System.Array.Copy(numSet, numSetCopy, totalCells);
            nowScore = score;
            highScore = high;
            nowNum = now;
            nextNum = next;
            nextNum2 = next2;
            eraseCount = erase;
        }
    }

    bool[] visited;
    int[] sameSpaceNum;
    int sameCount = 0;
    int clickedPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        totalCells = gridSize * gridSize;
        numSet = new int[totalCells];
        isFilled = new bool[totalCells];
        visited = new bool[totalCells + 5];
        sameSpaceNum = new int[totalCells + 5];
    }

    void Start()
    {
        uiManager = UIManager.Instance;
        soundManager = SoundManager.Instance;
        eraseCount = 1;
        restoreCount = 3;

        if (numBtns.Length != totalCells)
        {
            Debug.LogError($"NumBtn 배열 크기({numBtns.Length})가 그리드 크기({totalCells})와 맞지 않습니다!");
            return;
        }

        if (eraseBtn != null)
            eraseBtn.onClick.AddListener(ToggleEraseMode);

        if (restoreBtn != null)
            restoreBtn.onClick.AddListener(RestoreLastAction);

        InitGame();

        int challengeNum = GameDataTransfer.GetChallengeNum();
        string highScoreKey;

        if (challengeNum > 0)
        {
            highScoreKey = $"HighScore_{gridSize}x{gridSize}_Challenge{challengeNum}";
        }
        else
        {
            highScoreKey = $"HighScore_{gridSize}x{gridSize}";
        }

        uiManager.highScoreTxt.text = PlayerPrefs.GetInt(highScoreKey, 0).ToString();
        Debug.Log($"Game started with key: {highScoreKey}");

        UpdateItemButtons();

        if (!GoogleAdsManager.Instance.IsAdLoaded() && !GoogleAdsManager.Instance.IsLoadingAd())
        {
            GoogleAdsManager.Instance.LoadRewardedAd();
        }
    }

    void InitGame()
    {
        nowScore = 0;

        eraseAdWatchCount = 0;
        restoreAdWatchCount = 0;

        for (int a = 0; a < totalCells; a++)
        {
            numBtns[a].UnlockButton(); // 모든 버튼 잠금 해제
            numBtns[a].SetNumText(0);
            visited[a] = false;
            isFilled[a] = false;
            numSet[a] = 0;
        }

        for (int a = 0; a < 4; a++)
        {
            int ranIndex;
            int value;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                ranIndex = Random.Range(0, totalCells);
                int val_nul = Random.Range(0, 8);
                value = RanNumVal[val_nul];
                attempts++;

                if (attempts > maxAttempts)
                {
                    Debug.LogWarning("초기 배치 시도 횟수 초과. 기본 배치로 전환합니다.");
                    for (int i = 0; i < totalCells; i++)
                    {
                        if (numSet[i] == 0 && !numBtns[i].IsLocked())
                        {
                            ranIndex = i;
                            break;
                        }
                    }
                    break;
                }
            }
            while (numSet[ranIndex] != 0 || numSet[ranIndex] == -1 || numBtns[ranIndex].IsLocked() || WouldCreateThreeInARow(ranIndex, value));

            SetNum(ranIndex, value);
        }

        nowNum = 2;
        nextNum = GenerateNextNum();
        nextNum2 = GenerateNextNum();

        for (int i = 0; i < numBtns.Length; i++)
        {
            int index = i;
            numBtns[i].GetComponentInChildren<Button>().onClick.AddListener(() => BtnOnClicked(index));
        }
        uiManager.UpdateUI();

        // Challenge 모드: challengeNum에 따라 버튼 잠금
        LockButtonsForChallenge();
    }

    void LockButtonsForChallenge()
    {
        int challengeNum = GameDataTransfer.GetChallengeNum();

        if (challengeNum <= 0) return; // 일반 모드면 리턴

        Debug.Log($"Locking {challengeNum} buttons for challenge mode");

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < totalCells; i++)
        {
            availableIndices.Add(i);
        }

        for (int i = 0; i < challengeNum && availableIndices.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, availableIndices.Count);
            int buttonIndex = availableIndices[randomIndex];

            numBtns[buttonIndex].LockButton();
            numSet[buttonIndex] = -1;

            availableIndices.RemoveAt(randomIndex);
            Debug.Log($"Locked button at index: {buttonIndex}");
        }
    }

    bool WouldCreateThreeInARow(int index, int value)
    {
        int row = index / gridSize;
        int col = index % gridSize;

        int leftCount = 0;
        for (int i = col - 1; i >= 0; i--)
        {
            if (numSet[row * gridSize + i] == value)
                leftCount++;
            else
                break;
        }

        int rightCount = 0;
        for (int i = col + 1; i < gridSize; i++)
        {
            if (numSet[row * gridSize + i] == value)
                rightCount++;
            else
                break;
        }

        if (leftCount + rightCount + 1 >= 3)
            return true;

        int upCount = 0;
        for (int i = row - 1; i >= 0; i--)
        {
            if (numSet[i * gridSize + col] == value)
                upCount++;
            else
                break;
        }

        int downCount = 0;
        for (int i = row + 1; i < gridSize; i++)
        {
            if (numSet[i * gridSize + col] == value)
                downCount++;
            else
                break;
        }

        if (upCount + downCount + 1 >= 3)
            return true;

        return false;
    }

    void UpdateInfo()
    {
        uiManager.UpdateUI();
        uiManager.nowScoreTxt.text = nowScore.ToString();

        int filledCount = 0;
        int lockedCount = 0;

        for (int a = 0; a < totalCells; a++)
        {
            if (numSet[a] == -1 || numBtns[a].IsLocked())
            {
                lockedCount++;
            }
            else if (numSet[a] != 0)
            {
                filledCount++;
            }
        }

        // 잠긴 칸을 제외한 모든 칸이 채워졌는지 확인
        if (filledCount + lockedCount >= totalCells)
        {
            GameOver();
        }
    }

    void SetNum(int index, int val)
    {
        numSet[index] = val;
        numBtns[index].SetNumText(val);
    }


    int GenerateNextNum()
    {
        int result = 2;

        if (highScore <= 8)
        {
            int a = Random.Range(0, 4);
            if (a == 3)
            {
                result = 4;
            }
            else result = 2;
        }
        else if (highScore == 16)
        {
            int a = Random.Range(0, 100);
            if (a <= 65) result = 2;
            else if (a > 65 && a <= 90) result = 4;
            else result = 8;
        }
        else if (highScore == 32)
        {
            int a = Random.Range(0, 100);
            if (a <= 50) result = 2;
            else if (a > 50 && a <= 87) result = 4;
            else if (a > 87 && a <= 97) result = 8;
            else result = 16;
        }
        else if (highScore == 64)
        {
            int a = Random.Range(0, 100);
            if (a <= 45) result = 2;
            else if (a > 45 && a <= 80) result = 4;
            else if (a > 80 && a <= 94) result = 8;
            else result = 16;
        }
        else if (highScore == 128)
        {
            int a = Random.Range(0, 100);
            if (a <= 40) result = 2;
            else if (a > 40 && a <= 75) result = 4;
            else if (a > 75 && a <= 91) result = 8;
            else if (a > 91 && a <= 98) result = 16;
            else result = 32;
        }
        else if (highScore == 256)
        {
            int a = Random.Range(0, 100);
            if (a <= 35) result = 2;
            else if (a > 35 && a <= 62) result = 4;
            else if (a > 62 && a <= 85) result = 8;
            else if (a > 85 && a <= 96) result = 16;
            else result = 32;
        }
        else if (highScore > 256)
        {
            int a = Random.Range(0, 100);
            if (a <= 40) result = 2;
            else if (a > 40 && a <= 62) result = 4;
            else if (a > 62 && a <= 79) result = 8;
            else if (a > 79 && a <= 92) result = 16;
            else result = 32;
        }

        return result;
    }

    void ShiftNextNums()
    {
        nowNum = nextNum;
        nextNum = nextNum2;
        nextNum2 = GenerateNextNum();

        if (uiManager != null)
        {
            uiManager.AnimateNumberShift();
        }
    }

    void ToggleEraseMode()
    {
        if (eraseCount <= 0)
        {
            ShowAdForErase();
            return;
        }

        isEraseMode = !isEraseMode;

        if (uiManager != null)
        {
            uiManager.SetEraseModeVisual(isEraseMode);
        }
    }

    void ShowAdForErase()
    {
        if (eraseAdWatchCount >= MAX_AD_WATCH_PER_ITEM)
        {
            Debug.Log("이번 판에서 더 이상 광고를 시청할 수 없습니다!");
            return;
        }

        if (GoogleAdsManager.Instance.IsAdLoaded())
        {
            GoogleAdsManager.Instance.OnRewardEarned += OnEraseAdRewardEarned;
            GoogleAdsManager.Instance.OnAdClosed += OnEraseAdClosed;
            GoogleAdsManager.Instance.OnAdFailedToShow += OnEraseAdFailed;

            GoogleAdsManager.Instance.ShowRewardedAd();
        }
        else
        {
            Debug.Log("광고를 로딩 중입니다. 잠시 후 다시 시도해주세요.");
            GoogleAdsManager.Instance.LoadRewardedAd();
        }
    }

    void OnEraseAdRewardEarned()
    {
        eraseCount++;
        eraseAdWatchCount++;
        UpdateItemButtons();

        // 이벤트 구독 해제
        GoogleAdsManager.Instance.OnRewardEarned -= OnEraseAdRewardEarned;
        GoogleAdsManager.Instance.OnAdClosed -= OnEraseAdClosed;
        GoogleAdsManager.Instance.OnAdFailedToShow -= OnEraseAdFailed;
    }

    void OnEraseAdClosed()
    {
        // 이벤트 구독 해제
        GoogleAdsManager.Instance.OnAdClosed -= OnEraseAdClosed;
        GoogleAdsManager.Instance.OnRewardEarned -= OnEraseAdRewardEarned;
        GoogleAdsManager.Instance.OnAdFailedToShow -= OnEraseAdFailed;
    }

    void OnEraseAdFailed()
    {
        // 이벤트 구독 해제
        GoogleAdsManager.Instance.OnRewardEarned -= OnEraseAdRewardEarned;
        GoogleAdsManager.Instance.OnAdClosed -= OnEraseAdClosed;
        GoogleAdsManager.Instance.OnAdFailedToShow -= OnEraseAdFailed;
    }

    float GetTileCountBonus(int tileCount)
    {
        if (tileCount >= 5) return 2f;
        if (tileCount == 4) return 1.6f;
        if (tileCount == 3) return 1.3f;
        return 1f;
    }

    float GetComboMultiplier(int combo)
    {
        if (combo >= 4) return 4f;
        if (combo == 3) return 2f;
        if (combo == 2) return 1.5f;
        return 1f;
    }

    void AddScore(int mergedNumber, int comboMultiplier, int tileCount)
    {
        float comboBonus = GetComboMultiplier(comboMultiplier);
        float tileBonus = GetTileCountBonus(tileCount);
        int scoreToAdd = Mathf.CeilToInt(mergedNumber * comboBonus * tileBonus);
        nowScore += scoreToAdd;

        // 합성 시에만 plusScoreTxt 값 업데이트
        if (uiManager != null)
        {
            uiManager.UpdatePlusScore(scoreToAdd);
        }
    }

    public void BtnOnClicked(int index)
    {
        if (isAnimating) return;

        // 잠긴 버튼은 클릭 불가
        if (numBtns[index].IsLocked() || numSet[index] == -1)
        {
            Debug.Log($"Button {index} is locked and cannot be clicked");
            return;
        }

        if (isEraseMode)
        {
            if (numBtns[index].ReturnNum() != 0)
            {
                SaveGameState();

                numBtns[index].EraseAnimationToZero(0.2f, () =>
                {
                    numSet[index] = 0;
                });

                eraseCount--;

                isEraseMode = false;

                if (uiManager != null)
                {
                    uiManager.SetEraseModeVisual(false);
                }

                if (soundManager != null)
                    soundManager.PlaySFX(soundManager.eraseClip);

                UpdateItemButtons();
                UpdateInfo();
            }
            else
            {
                Debug.Log("빈 칸입니다!");
            }
            return;
        }

        if (numBtns[index].ReturnNum() == 0)
        {
            SaveGameState();

            nowScore += nowNum;

            // 버튼 클릭 시에는 plusScoreTxt를 숨김
            if (uiManager != null)
            {
                uiManager.HidePlusScore();
            }

            numBtns[index].SetNumText(nowNum);
            clickedPos = index;
            numSet[index] = nowNum;
            sameSpaceNum[0] = clickedPos;

            StartCoroutine(FindSameNumCoroutine(clickedPos));

            UpdateItemButtons();

            if (soundManager != null)
                soundManager.PlayBtnClickSFX();
        }
        else
        {
            Debug.Log("이미 채워진 칸입니다!");
        }

        numBtns[index].UpdateBtnImage();
    }

    void SaveGameState()
    {
        GameState state = new GameState(numSet, nowScore, highScore, nowNum, nextNum, nextNum2, eraseCount, gridSize);
        actionHistory.Push(state);

        if (actionHistory.Count > MAX_HISTORY_SIZE)
        {
            var tempArray = actionHistory.ToArray();
            actionHistory.Clear();

            for (int i = MAX_HISTORY_SIZE - 1; i >= 0; i--)
            {
                actionHistory.Push(tempArray[i]);
            }
        }
    }

    void RestoreLastAction()
    {
        if (restoreCount <= 0)
        {
            ShowAdForRestore();
            return;
        }

        if (actionHistory.Count == 0)
        {
            Debug.Log("복원할 액션이 없습니다!");
            return;
        }

        restoreCount--;

        GameState lastState = actionHistory.Pop();

        if (lastState.gridSize != gridSize)
        {
            return;
        }

        System.Array.Copy(lastState.numSetCopy, numSet, totalCells);
        nowScore = lastState.nowScore;
        highScore = lastState.highScore;
        nowNum = lastState.nowNum;
        nextNum = lastState.nextNum;
        nextNum2 = lastState.nextNum2;

        for (int i = 0; i < totalCells; i++)
        {
            numBtns[i].SetNumText(numSet[i]);
        }

        if (soundManager != null)
            soundManager.PlaySFX(soundManager.restoreClip);

        UpdateInfo();
        UpdateItemButtons();
    }

    void ShowAdForRestore()
    {
        if (restoreAdWatchCount >= MAX_AD_WATCH_PER_ITEM)
        {
            Debug.Log("이번 판에서 더 이상 광고를 시청할 수 없습니다!");
            return;
        }

        if (GoogleAdsManager.Instance.IsAdLoaded())
        {
            GoogleAdsManager.Instance.OnRewardEarned += OnRestoreAdRewardEarned;
            GoogleAdsManager.Instance.OnAdClosed += OnRestoreAdClosed;
            GoogleAdsManager.Instance.OnAdFailedToShow += OnRestoreAdFailed;

            GoogleAdsManager.Instance.ShowRewardedAd();
        }
        else
        {
            Debug.Log("광고를 로딩 중입니다. 잠시 후 다시 시도해주세요.");
            GoogleAdsManager.Instance.LoadRewardedAd();
        }
    }

    void OnRestoreAdRewardEarned()
    {
        restoreCount++;
        restoreAdWatchCount++;
        UpdateItemButtons();

        // 이벤트 구독 해제
        GoogleAdsManager.Instance.OnRewardEarned -= OnRestoreAdRewardEarned;
        GoogleAdsManager.Instance.OnAdClosed -= OnRestoreAdClosed;
        GoogleAdsManager.Instance.OnAdFailedToShow -= OnRestoreAdFailed;
    }

    void OnRestoreAdClosed()
    {
        // 이벤트 구독 해제
        GoogleAdsManager.Instance.OnAdClosed -= OnRestoreAdClosed;
        GoogleAdsManager.Instance.OnRewardEarned -= OnRestoreAdRewardEarned;
        GoogleAdsManager.Instance.OnAdFailedToShow -= OnRestoreAdFailed;
    }

    void OnRestoreAdFailed()
    {
        // 이벤트 구독 해제
        GoogleAdsManager.Instance.OnRewardEarned -= OnRestoreAdRewardEarned;
        GoogleAdsManager.Instance.OnAdClosed -= OnRestoreAdClosed;
        GoogleAdsManager.Instance.OnAdFailedToShow -= OnRestoreAdFailed;
    }

    void UpdateItemButtons()
    {
        if (uiManager != null)
        {
            uiManager.UpdateItemCount(eraseCount, restoreCount, GetRemainingEraseAds(), GetRemainingRestoreAds());
            uiManager.UpdateAdCountUI(eraseCount, restoreCount, GetRemainingEraseAds(), GetRemainingRestoreAds());
        }

        if (restoreBtn != null)
        {
            bool hasRestoreItem = restoreCount > 0;
            bool canWatchRestoreAd = restoreAdWatchCount < MAX_AD_WATCH_PER_ITEM;
            bool hasHistory = actionHistory.Count > 0;

            if (!hasHistory)
            {
                restoreBtn.interactable = false;
            }
            else
            {
                restoreBtn.interactable = hasRestoreItem || canWatchRestoreAd;
            }
        }
    }

    void TestFind(int pos)
    {
        visited[pos] = true;
        int sameCountTemp = 0;
        int row = pos / gridSize;
        int col = pos % gridSize;

        // 왼쪽 체크 (잠긴 버튼 제외)
        if (col > 0 && !visited[pos - 1] && numBtns[pos - 1].ReturnNum() == nowNum && !numBtns[pos - 1].IsLocked() && numSet[pos - 1] != -1)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos - 1] = true;
            sameSpaceNum[sameCount] = pos - 1;
        }
        // 오른쪽 체크 (잠긴 버튼 제외)
        if (col < gridSize - 1 && !visited[pos + 1] && numBtns[pos + 1].ReturnNum() == nowNum && !numBtns[pos + 1].IsLocked() && numSet[pos + 1] != -1)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos + 1] = true;
            sameSpaceNum[sameCount] = pos + 1;
        }
        // 위쪽 체크 (잠긴 버튼 제외)
        if (row > 0 && !visited[pos - gridSize] && numBtns[pos - gridSize].ReturnNum() == nowNum && !numBtns[pos - gridSize].IsLocked() && numSet[pos - gridSize] != -1)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos - gridSize] = true;
            sameSpaceNum[sameCount] = pos - gridSize;
        }
        // 아래쪽 체크 (잠긴 버튼 제외)
        if (row < gridSize - 1 && !visited[pos + gridSize] && numBtns[pos + gridSize].ReturnNum() == nowNum && !numBtns[pos + gridSize].IsLocked() && numSet[pos + gridSize] != -1)
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
            comboCount = 0;
            yield return StartCoroutine(MergeSequence(pos));
        }
        else
        {
            if (highScore < nowNum)
            {
                highScore = nowNum;
            }

            ResetValue();
            clickedPos = 0;
        }

        ShiftNextNums();

        yield return new WaitForSeconds(uiManager.GetNumberShiftDuration());

        UpdateInfo();

        isAnimating = false;
    }

    IEnumerator MergeSequence(int pos)
    {
        while (sameCount >= 2)
        {
            comboCount++;

            int currentTileCount = sameCount + 1;

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

            if (soundManager != null)
                soundManager.PlaySFX(soundManager.slideClip);

            yield return new WaitForSeconds(btnCombineTime);

            numSet[clickedPos] = 0;
            numBtns[clickedPos].SetNumText(0);

            nowNum *= 2;
            numSet[clickedPos] = nowNum;
            numBtns[clickedPos].SetNumText(nowNum);

            AddScore(nowNum, comboCount, currentTileCount);

            if (comboCount >= 2 && uiManager != null)
            {
                uiManager.ShowCombo(nowNum, numBtns[clickedPos].transform.position);
            }

            if (soundManager != null)
                soundManager.PlaySFX(soundManager.combineComboClip);
            if (nowNum >= 16 && VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayCombineParticle(numBtns[clickedPos].transform, nowNum);
            }

            int tempClickedPos = clickedPos;
            ResetValue();
            sameSpaceNum[0] = tempClickedPos;

            TestFind(pos);

            if (sameCount < 2)
                break;

            yield return new WaitForSeconds(combineDelayTime);
        }

        if (highScore < nowNum)
        {
            highScore = nowNum;
        }

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

    void GameOver()
    {
        Debug.Log("GameOver");

        int challengeNum = GameDataTransfer.GetChallengeNum();
        string highScoreKey;

        if (challengeNum > 0)
        {
            highScoreKey = $"HighScore_{gridSize}x{gridSize}_Challenge{challengeNum}";
        }
        else
        {
            highScoreKey = $"HighScore_{gridSize}x{gridSize}";
        }

        Debug.Log($"Saving high score to key: {highScoreKey}");

        if (PlayerPrefs.GetInt(highScoreKey) <= nowScore)
        {
            PlayerPrefs.SetInt(highScoreKey, nowScore);
            PlayerPrefs.Save();
        }

        uiManager.finalScoreTxt.text = nowScore.ToString();

        gameOverPanel.gameObject.SetActive(true);
        gameOverPanel.UpdateGameOverUI(nowScore, gridSize);
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