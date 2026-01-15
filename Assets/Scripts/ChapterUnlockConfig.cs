using UnityEngine;

[CreateAssetMenu(fileName = "ChapterUnlockConfig", menuName = "Game/Chapter Unlock Config")]
public class ChapterUnlockConfig : ScriptableObject
{
    [Header("Chapter Unlock Requirements")]
    [Tooltip("Chapter 1 (5x5 일반) 해금에 필요한 4x4 최고점수")]
    public int chapter1UnlockScore = 2000;

    [Tooltip("Chapter 2 (5x5 챌린지) 해금에 필요한 5x5 최고점수")]
    public int chapter2UnlockScore = 10000;
    public int GetUnlockScore(int chapterIndex)
    {
        switch (chapterIndex)
        {
            case 0: return 0;
            case 1: return chapter1UnlockScore;
            case 2: return chapter2UnlockScore;
            default: return 0;
        }
    }
}