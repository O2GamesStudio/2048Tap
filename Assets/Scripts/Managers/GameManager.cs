using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    UIManager uiManager;


    public int nowNum, nextNum;
    //점수 변수
    int nowScore = 2;
    int highScore = 8;

    int[] numSet = new int[25];
    int[] RanNumVal = new int[] { 2, 2, 2, 2, 2, 4, 4, 8 };
    bool[] isFilled = new bool[25];

    [SerializeField] NumBtn[] numBtns;

    [Header("Items")]
    [SerializeField] Button eraseBtn;
    [SerializeField] Button restoreBtn;
    [HideInInspector] public int eraseCount, restoreCount;
    private bool isEraseMode = false;
    private Stack<GameState> actionHistory = new Stack<GameState>();

    // 게임 상태 저장용 구조체
    private struct GameState
    {
        public int[] numSetCopy;
        public int nowScore;
        public int highScore;
        public int nowNum;
        public int nextNum;
        public int eraseCount;
        // restoreCount는 저장하지 않음 (복원 시 자동으로 처리)

        public GameState(int[] numSet, int score, int high, int now, int next, int erase)
        {
            numSetCopy = new int[25];
            System.Array.Copy(numSet, numSetCopy, 25);
            nowScore = score;
            highScore = high;
            nowNum = now;
            nextNum = next;
            eraseCount = erase;
        }
    }

    // 클릭 후 순회 로직
    bool[] visited = new bool[30];
    int[] sameSpaceNum = new int[30];
    int sameCount = 0;
    int clickedPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        uiManager = UIManager.Instance;
        eraseCount = 3;
        restoreCount = 3;

        if (eraseBtn != null)
            eraseBtn.onClick.AddListener(ToggleEraseMode);

        if (restoreBtn != null)
            restoreBtn.onClick.AddListener(RestoreLastAction);

        InitGame();
        uiManager.highScoreTxt.text = PlayerPrefs.GetInt("HighScore").ToString();

        UpdateItemButtons();
    }

    void InitGame()
    {
        for (int a = 0; a < 25; a++)
        {
            numBtns[a].SetNumText(0);
            visited[a] = false;
            isFilled[a] = false;
        }

        for (int a = 0; a < 4; a++)
        {
            int ranIndex = Random.Range(0, 25);
            while (numSet[ranIndex] != 0)
            {
                ranIndex = Random.Range(0, 25);
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
        for (int a = 0; a < 25; a++)
        {
            if (numSet[a] != 0)
            {
                filledCount++;
            }
        }
        if (filledCount >= 25) GameOver();
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

        Debug.Log("지우기 모드: " + (isEraseMode ? "활성화" : "비활성화"));
    }

    public void BtnOnClicked(int index)
    {
        if (isEraseMode)
        {
            if (numBtns[index].ReturnNum() != 0)
            {
                // ★ 지우기 전 상태 저장
                SaveGameState();

                numSet[index] = 0;
                numBtns[index].SetNumText(0);
                eraseCount--;

                Debug.Log($"칸 {index} 지워짐 - 남은 지우기: {eraseCount}");

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

        // 일반 모드
        if (numBtns[index].ReturnNum() == 0)
        {
            // ★ 숫자 배치 전 상태 저장
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

        numBtns[index].UpdateColor();
    }

    // ★ 현재 상태를 "액션 실행 전"에 저장
    void SaveGameState()
    {
        // 현재 상태를 그대로 저장 (restoreCount 제외)
        GameState state = new GameState(numSet, nowScore, highScore, nowNum, nextNum, eraseCount);
        actionHistory.Push(state);

        Debug.Log($"[저장] nowNum:{nowNum}, nextNum:{nextNum}, eraseCount:{eraseCount}, Stack Count:{actionHistory.Count}");

        // 히스토리 제한
        if (actionHistory.Count > 20)
        {
            var tempList = new List<GameState>(actionHistory);
            actionHistory.Clear();
            for (int i = 0; i < 20; i++)
            {
                actionHistory.Push(tempList[i]);
            }
        }
    }

    // ★ 저장된 상태로 복원
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

        // restoreCount 먼저 감소
        restoreCount--;

        GameState lastState = actionHistory.Pop();

        // 저장된 상태 그대로 복원
        System.Array.Copy(lastState.numSetCopy, numSet, 25);
        nowScore = lastState.nowScore;
        highScore = lastState.highScore;
        nowNum = lastState.nowNum;
        nextNum = lastState.nextNum;
        eraseCount = lastState.eraseCount;

        Debug.Log($"[복원] nowNum:{nowNum}, nextNum:{nextNum}, eraseCount:{eraseCount}, Stack Count:{actionHistory.Count}");

        // UI 업데이트
        for (int i = 0; i < 25; i++)
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
        int a = pos % 5;

        if (pos - 1 >= 0 && a != 0 && !visited[pos - 1] && numBtns[pos - 1].ReturnNum() == nowNum)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos - 1] = true;
            sameSpaceNum[sameCount] = pos - 1;
        }
        if (pos + 1 < 25 && a != 4 && !visited[pos + 1] && numBtns[pos + 1].ReturnNum() == nowNum)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos + 1] = true;
            sameSpaceNum[sameCount] = pos + 1;
        }
        if (pos - 5 >= 0 && !visited[pos - 5] && numBtns[pos - 5].ReturnNum() == nowNum)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos - 5] = true;
            sameSpaceNum[sameCount] = pos - 5;
        }
        if (pos + 5 < 25 && !visited[pos + 5] && numBtns[pos + 5].ReturnNum() == nowNum)
        {
            sameCount++;
            sameCountTemp++;
            visited[pos + 5] = true;
            sameSpaceNum[sameCount] = pos + 5;
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
            Debug.Log($"Play Particle - Final Number: {nowNum}");
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
        for (int a = 0; a < 30; a++)
        {
            sameSpaceNum[a] = 0;
            visited[a] = false;
        }
        sameCount = 0;
    }

    void GameOver()
    {
        if (PlayerPrefs.GetInt("HighScore") <= nowScore)
        {
            PlayerPrefs.SetInt("HighScore", nowScore);
        }
        uiManager.finalScoreTxt.text = nowScore.ToString();
    }
}