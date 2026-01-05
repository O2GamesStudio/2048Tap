using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq; // ★ 추가

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    UIManager uiManager;

    [Header("Grid Settings")]
    [SerializeField] int gridSize = 5;
    private int totalCells;

    public Sprite[] numberSprites;
    public int nowNum, nextNum;
    int nowScore = 2;
    int highScore = 8;

    int[] numSet;
    int[] RanNumVal = new int[] { 2, 2, 2, 2, 2, 4, 4, 8 };
    bool[] isFilled;

    [SerializeField] NumBtn[] numBtns;

    [Header("Items")]
    [SerializeField] Button eraseBtn;
    [SerializeField] Button restoreBtn;
    [HideInInspector] public int eraseCount, restoreCount;
    private bool isEraseMode = false;
    private Stack<GameState> actionHistory = new Stack<GameState>();

    // ★ 히스토리 최대 크기 설정
    private const int MAX_HISTORY_SIZE = 20;

    private struct GameState
    {
        public int[] numSetCopy;
        public int nowScore;
        public int highScore;
        public int nowNum;
        public int nextNum;
        public int eraseCount;
        public int gridSize;

        public GameState(int[] numSet, int score, int high, int now, int next, int erase, int size)
        {
            gridSize = size;
            int totalCells = size * size;
            numSetCopy = new int[totalCells];
            System.Array.Copy(numSet, numSetCopy, totalCells);
            nowScore = score;
            highScore = high;
            nowNum = now;
            nextNum = next;
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
        eraseCount = 3;
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

        nextNum = 2;
        nowNum = 2;

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
        if (filledCount >= totalCells) GameOver();
    }

    void SetNum(int index, int val)
    {
        numSet[index] = val;
        numBtns[index].SetNumText(val);
    }

    void SetNextNum()
    {
        if (highScore <= 8)
        {
            int a = Random.Range(0, 4);
            if (a == 3)
            {
                nextNum = 4;
            }
            else nextNum = 2;
        }
        else if (highScore == 16)
        {
            int a = Random.Range(0, 100);
            if (a <= 65) nextNum = 2;
            else if (a > 65 && a <= 90) nextNum = 4;
            else nextNum = 8;
        }
        else if (highScore == 32)
        {
            int a = Random.Range(0, 100);
            if (a <= 50) nextNum = 2;
            else if (a > 50 && a <= 87) nextNum = 4;
            else if (a > 87 && a <= 97) nextNum = 8;
            else nextNum = 16;
        }
        else if (highScore == 64)
        {
            int a = Random.Range(0, 100);
            if (a <= 45) nextNum = 2;
            else if (a > 45 && a <= 80) nextNum = 4;
            else if (a > 80 && a <= 94) nextNum = 8;
            else nextNum = 16;
        }
        else if (highScore == 128)
        {
            int a = Random.Range(0, 100);
            if (a <= 40) nextNum = 2;
            else if (a > 40 && a <= 75) nextNum = 4;
            else if (a > 75 && a <= 91) nextNum = 8;
            else if (a > 91 && a <= 98) nextNum = 16;
            else nextNum = 32;
        }
        else if (highScore == 256)
        {
            int a = Random.Range(0, 100);
            if (a <= 35) nextNum = 2;
            else if (a > 35 && a <= 62) nextNum = 4;
            else if (a > 62 && a <= 85) nextNum = 8;
            else if (a > 85 && a <= 96) nextNum = 16;
            else nextNum = 32;
        }
        else if (highScore > 256)
        {
            int a = Random.Range(0, 100);
            if (a <= 40) nextNum = 2;
            else if (a > 40 && a <= 62) nextNum = 4;
            else if (a > 62 && a <= 79) nextNum = 8;
            else if (a > 79 && a <= 92) nextNum = 16;
            else nextNum = 32;
        }
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
        if (isEraseMode)
        {
            if (numBtns[index].ReturnNum() != 0)
            {
                SaveGameState();

                numSet[index] = 0;
                numBtns[index].SetNumText(0);
                eraseCount--;

                isEraseMode = false;
                if (eraseBtn != null)
                {
                    ColorBlock colors = eraseBtn.colors;
                    colors.normalColor = Color.white;
                    eraseBtn.colors = colors;
                }

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

            FindSameNum(clickedPos);

            nowNum = nextNum;
            SetNextNum();

            UpdateItemButtons();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayBtnClickSFX();

            UpdateInfo();
        }
        else
        {
            Debug.Log("이미 채워진 칸입니다!");
        }

        numBtns[index].UpdateBtnImage();
    }

    // ★ 수정된 SaveGameState - 올바른 히스토리 제한
    void SaveGameState()
    {
        GameState state = new GameState(numSet, nowScore, highScore, nowNum, nextNum, eraseCount, gridSize);
        actionHistory.Push(state);

        Debug.Log($"[저장] Score:{nowScore}, nowNum:{nowNum}, nextNum:{nextNum}, Stack:{actionHistory.Count}");

        // ★ 히스토리가 최대 크기를 초과하면 가장 오래된 것 제거
        if (actionHistory.Count > MAX_HISTORY_SIZE)
        {
            // Stack을 배열로 변환 (인덱스 0이 최신)
            var tempArray = actionHistory.ToArray();
            actionHistory.Clear();

            // 최신 MAX_HISTORY_SIZE개만 다시 추가 (역순으로 Push)
            for (int i = MAX_HISTORY_SIZE - 1; i >= 0; i--)
            {
                actionHistory.Push(tempArray[i]);
            }

            Debug.Log($"[히스토리 정리] {MAX_HISTORY_SIZE}개로 제한됨");
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
        eraseCount = lastState.eraseCount;

        Debug.Log($"[복원] Score:{nowScore}, nowNum:{nowNum}, nextNum:{nextNum}, Stack:{actionHistory.Count}");

        for (int i = 0; i < totalCells; i++)
        {
            numBtns[i].SetNumText(numSet[i]);
        }

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

    void FindSameNum(int pos)
    {
        TestFind(pos);

        bool wasMerged = false;

        while (sameCount >= 2)
        {
            wasMerged = true;

            for (int b = 0; b <= sameCount; b++)
            {
                numSet[sameSpaceNum[b]] = 0;
                numBtns[sameSpaceNum[b]].SetNumText(0);
            }
            nowNum *= 2;
            numSet[clickedPos] = nowNum;
            numBtns[clickedPos].SetNumText(nowNum);

            int tempClickedPos = clickedPos;
            ResetValue();
            sameSpaceNum[0] = tempClickedPos;

            TestFind(pos);
        }

        if (wasMerged && nowNum >= 16 && VFXManager.Instance != null)
        {
            VFXManager.Instance.PlayCombineParticle(numBtns[clickedPos].transform, nowNum);
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
        string highScoreKey = $"HighScore_{gridSize}x{gridSize}";
        if (PlayerPrefs.GetInt(highScoreKey) <= nowScore)
        {
            PlayerPrefs.SetInt(highScoreKey, nowScore);
        }
        uiManager.finalScoreTxt.text = nowScore.ToString();
    }
}