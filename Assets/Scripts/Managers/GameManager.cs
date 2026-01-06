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

    [Header("Grid Settings")]
    [SerializeField] int gridSize = 5;
    private int totalCells;

    public Sprite[] numberSprites { get { return _numberSprites; } }
    [SerializeField] private Sprite[] _numberSprites;

    [HideInInspector] public int nowNum { get; private set; }
    [HideInInspector] public int nextNum { get; private set; }
    [HideInInspector] public int nextNum2 { get; private set; }
    int nowScore = 2;
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

    private bool isEraseMode = false;
    private Stack<GameState> actionHistory = new Stack<GameState>();

    [Header("Animation Settings")]
    [SerializeField] float btnCombineTime = 0.2f;
    [SerializeField] float combineDelayTime = 0.05f;
    private bool isAnimating = false;

    [SerializeField] GameOverPanel gameOverPanel;
    private const int MAX_HISTORY_SIZE = 20;


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
        restoreCount = 1000;

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
        uiManager.highScoreTxt.text = PlayerPrefs.GetInt($"HighScore_{gridSize}x{gridSize}").ToString();

        UpdateItemButtons();
    }

    void InitGame()
    {
        for (int a = 0; a < totalCells; a++)
        {
            numBtns[a].SetNumText(0);
            visited[a] = false;
            isFilled[a] = false;
            numSet[a] = 0;
        }

        for (int a = 0; a < 4; a++)
        {
            int ranIndex = Random.Range(0, totalCells);
            while (numSet[ranIndex] != 0)
            {
                ranIndex = Random.Range(0, totalCells);
            }
            int val_nul = Random.Range(0, 8);
            SetNum(ranIndex, RanNumVal[val_nul]);
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
    }

    void UpdateInfo()
    {
        uiManager.UpdateUI();
        uiManager.nowScoreTxt.text = nowScore.ToString();

        int filledCount = 0;
        for (int a = 0; a < totalCells; a++)
        {
            if (numSet[a] != 0)
            {
                filledCount++;
            }
        }

        if (filledCount >= totalCells)
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
    }

    void ToggleEraseMode()
    {
        if (eraseCount <= 0)
        {
            Debug.Log("지우기 아이템이 없습니다!");
            return;
        }

        isEraseMode = !isEraseMode;
        if (eraseBtn != null)
        {
            ColorBlock colors = eraseBtn.colors;
            if (isEraseMode)
            {
                colors.normalColor = new Color(1f, 0.6f, 0.6f);
            }
            else
            {
                colors.normalColor = Color.white;
            }
            eraseBtn.colors = colors;
        }
    }

    public void BtnOnClicked(int index)
    {
        if (isAnimating) return;

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
                if (eraseBtn != null)
                {
                    ColorBlock colors = eraseBtn.colors;
                    colors.normalColor = Color.white;
                    eraseBtn.colors = colors;
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
            Debug.Log("복원 아이템이 없습니다!");
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
            Debug.LogError("저장된 그리드 크기와 현재 그리드 크기가 다릅니다!");
            return;
        }

        System.Array.Copy(lastState.numSetCopy, numSet, totalCells);
        nowScore = lastState.nowScore;
        highScore = lastState.highScore;
        nowNum = lastState.nowNum;
        nextNum = lastState.nextNum;
        nextNum2 = lastState.nextNum2;

        Debug.Log($"[복원] Score:{nowScore}, nowNum:{nowNum}, nextNum:{nextNum}, nextNum2:{nextNum2}, Stack:{actionHistory.Count}");

        for (int i = 0; i < totalCells; i++)
        {
            numBtns[i].SetNumText(numSet[i]);
        }

        if (soundManager != null)
            soundManager.PlaySFX(soundManager.restoreClip);

        UpdateInfo();
        UpdateItemButtons();
    }

    void UpdateItemButtons()
    {
        if (restoreBtn != null)
        {
            restoreBtn.interactable = actionHistory.Count > 0 && restoreCount > 0;
        }

        if (eraseBtn != null)
        {
            eraseBtn.interactable = eraseCount > 0;
        }

        if (uiManager != null)
        {
            uiManager.UpdateItemCount(eraseCount, restoreCount);
        }
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

            if (soundManager != null)
                soundManager.PlaySFX(soundManager.slideClip);

            yield return new WaitForSeconds(btnCombineTime);

            numSet[clickedPos] = 0;
            numBtns[clickedPos].SetNumText(0);

            nowNum *= 2;
            numSet[clickedPos] = nowNum;
            numBtns[clickedPos].SetNumText(nowNum);

            if (nowNum >= 16 && VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayCombineParticle(numBtns[clickedPos].transform, nowNum);
                soundManager.PlaySFX(soundManager.combineComboClip);
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

    void GameOver()
    {
        Debug.Log("GameOver");
        string highScoreKey = $"HighScore_{gridSize}x{gridSize}";
        if (PlayerPrefs.GetInt(highScoreKey) <= nowScore)
        {
            PlayerPrefs.SetInt(highScoreKey, nowScore);
        }
        uiManager.finalScoreTxt.text = nowScore.ToString();

        gameOverPanel.gameObject.SetActive(true);
        gameOverPanel.UpdateGameOverUI(nowScore);
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