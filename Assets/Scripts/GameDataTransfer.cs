using UnityEngine;

public class GameDataTransfer : MonoBehaviour
{
    public static int challengeNum = 0;

    // 로비에서 챌린지 번호를 설정하는 메서드
    public static void SetChallengeNum(int value)
    {
        challengeNum = value;
        Debug.Log($"Challenge Number set to: {challengeNum}");
    }

    // 게임에서 챌린지 번호를 가져오는 메서드
    public static int GetChallengeNum()
    {
        return challengeNum;
    }

    // 게임 시작 시 챌린지 번호 리셋 (필요한 경우)
    public static void ResetChallengeNum()
    {
        challengeNum = 0;
    }
}