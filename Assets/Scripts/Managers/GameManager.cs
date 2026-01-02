using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

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
    //[SerializeField] Button[] NumBtnSet;
    bool[] isFilled = new bool[25];

    [SerializeField] NumBtn[] numBtns;


    // 클릭 후 순회 로직
    bool[] visited = new bool[30];
    int[] sameSpaceNum = new int[30]; //지워저야할 숫자 칸
    int sameCount = 0; //같은 숫자가 몇개 있는지
    int clickedPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        uiManager = UIManager.Instance;

        InitGame();
        uiManager.highScoreTxt.text = PlayerPrefs.GetInt("HighScore").ToString();
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
            int val_nul = Random.Range(0, 8);

            SetNum(ranIndex, RanNumVal[val_nul]);
        }
        nextNum = 2;
        nowNum = 2;

        for (int i = 0; i < numBtns.Length; i++)
        {
            int index = i;
            //NumBtnSet[i].onClick.AddListener(() => BtnOnClicked(index));
            numBtns[i].GetComponentInChildren<Button>().onClick.AddListener(() => BtnOnClicked(index));
        }
    }

    void UpdateInfo()
    {
        uiManager.UpdateUI();
        uiManager.nowScoreTxt.text = nowScore.ToString();

        int filledCount = 0;
        for (int a = 0; a < 25; a++)
        {
            //칸 순회하면서 있는지 없는지 확인하기
            //꽉 차 있으면 GameOver
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
            if (a <= 50) nextNum = 2;
            else if (a > 50 && a <= 80) nextNum = 4;
            else if (a > 80 && a <= 94) nextNum = 8;
            else nextNum = 16;
        }
        else if (highScore == 128)
        {
            int a = Random.Range(0, 100);
            if (a <= 45) nextNum = 2;
            else if (a > 45 && a <= 75) nextNum = 4;
            else if (a > 75 && a <= 91) nextNum = 8;
            else if (a > 91 && a <= 98) nextNum = 16;
            else nextNum = 32;
        }
        else if (highScore == 256)
        {
            int a = Random.Range(0, 100);
            if (a <= 43) nextNum = 2;
            else if (a > 43 && a <= 65) nextNum = 4;
            else if (a > 65 && a <= 85) nextNum = 8;
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

    public void BtnOnClicked(int index)
    {
        if (numBtns[index].ReturnNum() == 0)
        {
            numBtns[index].SetNumText(nowNum);

            clickedPos = index;
            numSet[index] = nowNum;
            sameSpaceNum[0] = clickedPos;

            Debug.Log("Clicked Position: " + clickedPos);

            FindSameNum(clickedPos);

            nowNum = nextNum;
            SetNextNum();
        }
        else
        {
            Debug.Log("이미 채워진 칸입니다!");
        }


        UpdateInfo();
        numBtns[index].UpdateColor();
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
        while (sameCount >= 2)
        {
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
