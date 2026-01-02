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
    public Button[] NumBtnSet;
    bool[] isFilled = new bool[25];


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
    void Update()
    {
        // Todo : 버튼 클릭 후 처리하기
        UpdateInfo();
        UpdateTextColor();
    }
    void InitGame()
    {
        for (int a = 0; a < 25; a++)
        {
            NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().text = "";
            visited[a] = false;
            isFilled[a] = false;
        }

        for (int a = 0; a < 4; a++)
        {
            int val_a = Random.Range(0, 5);
            int val_b = Random.Range(0, 5);
            int val_nul = Random.Range(0, 8);

            SetNum(val_a, val_b, RanNumVal[val_nul]);
        }
        nextNum = 2;
        nowNum = 2;

        // 버튼 바인딩 - 인덱스를 캡처하도록 수정
        for (int i = 0; i < NumBtnSet.Length; i++)
        {
            int index = i; // 클로저 문제 방지를 위한 로컬 변수
            NumBtnSet[i].onClick.AddListener(() => BtnOnClicked(index));
        }
    }
    void UpdateTextColor()
    {
        for (int a = 0; a < 25; a++)
        {
            if (NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().text == "2")
            {
                NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().color = Color.black;
            }
            else if (NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().text == "4")
            {
                NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().color =
                    new Color(150 / 255f, 65 / 255f, 65 / 255f);
            }
            else if (NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().text == "8")
            {
                NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().color =
                    new Color(25 / 255f, 60 / 255f, 150 / 255f);
            }
            else if (NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().text == "16")
            {
                NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().color =
                    new Color(25 / 255f, 150 / 255f, 150 / 255f);
            }
            else if (NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().text == "32")
            {
                NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().color =
                    new Color(60 / 255f, 70 / 255f, 10 / 255f);
            }
            else if (NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().text == "64")
            {
                NumBtnSet[a].GetComponentInChildren<TextMeshProUGUI>().color =
                    new Color(100 / 255f, 30 / 255f, 170 / 255f);
            }
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

    void SetNum(int pos_x, int pos_y, int val)
    {
        int realPos = pos_x * 5 + pos_y;
        numSet[realPos] = val;
        NumBtnSet[realPos].GetComponentInChildren<TextMeshProUGUI>().text = val.ToString();
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
        if (NumBtnSet[index].GetComponentInChildren<TextMeshProUGUI>().text == null ||
            NumBtnSet[index].GetComponentInChildren<TextMeshProUGUI>().text == "")
        {
            NumBtnSet[index].GetComponentInChildren<TextMeshProUGUI>().text = nowNum.ToString();

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
    }


    void TestFind(int pos) // pos = 16
    {
        visited[pos] = true;
        int sameCountTemp = 0;
        int a = pos % 5; //a = 1
        if (pos - 1 >= 0 && a != 0 && !visited[pos - 1] &&
            NumBtnSet[pos - 1].GetComponentInChildren<TextMeshProUGUI>().text == nowNum.ToString())
        {
            sameCount++;
            sameCountTemp++;
            visited[pos - 1] = true;
            sameSpaceNum[sameCount] = pos - 1;
        }
        if (pos + 1 < 25 && a != 4 && !visited[pos + 1] &&
            NumBtnSet[pos + 1].GetComponentInChildren<TextMeshProUGUI>().text == nowNum.ToString())
        {
            sameCount++;
            sameCountTemp++;
            visited[pos + 1] = true;
            sameSpaceNum[sameCount] = pos + 1;
        }
        if (pos - 5 >= 0 && !visited[pos - 5] &&
           NumBtnSet[pos - 5].GetComponentInChildren<TextMeshProUGUI>().text == nowNum.ToString())
        {
            sameCount++;
            sameCountTemp++;
            visited[pos - 5] = true;
            sameSpaceNum[sameCount] = pos - 5;
        }
        if (pos + 5 < 25 && !visited[pos + 5] &&
          NumBtnSet[pos + 5].GetComponentInChildren<TextMeshProUGUI>().text == nowNum.ToString())
        {
            sameCount++;
            sameCountTemp++;
            visited[pos + 5] = true;
            sameSpaceNum[sameCount] = pos + 5;
        }
        //이어진 같은수 찾기
        for (int turn = 1; turn <= sameCountTemp; turn++)
        {
            TestFind(sameSpaceNum[turn]);
        }
    }

    void FindSameNum(int pos) //주변에 같은 숫자 3개가 있는지 확인 
    {
        TestFind(pos);
        while (sameCount >= 2)
        {
            for (int b = 0; b <= sameCount; b++)
            {
                numSet[sameSpaceNum[b]] = 0;
                NumBtnSet[sameSpaceNum[b]].GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
            nowNum *= 2;
            numSet[clickedPos] = nowNum;
            NumBtnSet[clickedPos].GetComponentInChildren<TextMeshProUGUI>().text = nowNum.ToString();

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

        //변수 초기화 해주기!!!
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
