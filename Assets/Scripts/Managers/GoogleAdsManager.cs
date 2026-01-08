using System;
using UnityEngine;
using GoogleMobileAds.Api;

public class GoogleAdsManager : MonoBehaviour
{
    private static GoogleAdsManager instance;
    public static GoogleAdsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GoogleAdsManager");
                instance = go.AddComponent<GoogleAdsManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    // 플랫폼별 광고 ID
#if UNITY_ANDROID
    private string rewardedAdUnitId = "ca-app-pub-3490273194196393/8703866303";
#else
    private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; 
#endif

    private RewardedAd rewardedAd;
    private bool isAdLoaded = false;

    // 보상 콜백
    public event Action OnRewardEarned;
    public event Action OnAdClosed;
    public event Action OnAdFailedToLoad;
    public event Action OnAdFailedToShow;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAds()
    {
        // AdMob 초기화
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("AdMob 초기화 완료");
            LoadRewardedAd();
        });
    }

    /// <summary>
    /// 보상형 광고 로드
    /// </summary>
    public void LoadRewardedAd()
    {
        // 기존 광고가 있다면 제거
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
            rewardedAd = null;
        }

        Debug.Log("보상형 광고 로딩 중...");

        // 광고 요청 생성
        var adRequest = new AdRequest();

        // 보상형 광고 로드
        RewardedAd.Load(rewardedAdUnitId, adRequest,
            (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError("보상형 광고 로드 실패: " + error);
                    isAdLoaded = false;
                    OnAdFailedToLoad?.Invoke();
                    return;
                }

                Debug.Log("보상형 광고 로드 성공");
                rewardedAd = ad;
                isAdLoaded = true;

                RegisterEventHandlers(rewardedAd);
            });
    }

    /// <summary>
    /// 광고 이벤트 핸들러 등록
    /// </summary>
    private void RegisterEventHandlers(RewardedAd ad)
    {
        // 광고가 전체 화면 콘텐츠를 표시했을 때
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("보상형 광고가 열렸습니다.");
        };

        // 광고가 전체 화면 콘텐츠를 닫았을 때
        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("보상형 광고가 닫혔습니다.");
            OnAdClosed?.Invoke();

            // 광고를 다시 로드
            LoadRewardedAd();
        };

        // 광고 표시 실패 시
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("보상형 광고 표시 실패: " + error);
            OnAdFailedToShow?.Invoke();

            // 광고를 다시 로드
            LoadRewardedAd();
        };
    }

    /// <summary>
    /// 보상형 광고 표시
    /// </summary>
    public void ShowRewardedAd()
    {
        if (rewardedAd != null && isAdLoaded)
        {
            rewardedAd.Show((Reward reward) =>
            {
                Debug.Log($"보상 지급: {reward.Type}, 수량: {reward.Amount}");
                OnRewardEarned?.Invoke();
            });

            isAdLoaded = false;
        }
        else
        {
            Debug.LogWarning("보상형 광고가 아직 로드되지 않았습니다.");
            OnAdFailedToShow?.Invoke();

            // 광고가 없으면 다시 로드 시도
            LoadRewardedAd();
        }
    }

    /// <summary>
    /// 광고가 로드되었는지 확인
    /// </summary>
    public bool IsAdLoaded()
    {
        return isAdLoaded && rewardedAd != null;
    }

    /// <summary>
    /// 현재 사용 중인 광고 ID 반환 (디버깅용)
    /// </summary>
    public string GetAdUnitId()
    {
        return rewardedAdUnitId;
    }

    private void OnDestroy()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
        }
    }
}