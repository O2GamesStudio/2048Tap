using System;
using UnityEngine;
using GoogleMobileAds.Api;
using System.Collections;

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

#if UNITY_ANDROID
    private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; // 테스트 ID (출시 시 실제 ID로 변경)
    // private string rewardedAdUnitId = "ca-app-pub-3490273194196393/8703866303"; // 실제 ID
#else
    private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; 
#endif

    private RewardedAd rewardedAd;
    private bool isAdLoaded = false;
    private bool isInitialized = false;
    private bool isLoadingAd = false;

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
        try
        {
            MobileAds.Initialize(initStatus =>
            {
                isInitialized = true;
                StartCoroutine(LoadAdWithDelay(0.5f));
            });
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AdMob 초기화 실패: {e.Message}");
        }
    }

    private IEnumerator LoadAdWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadRewardedAd();
    }

    public void LoadRewardedAd()
    {
        if (isLoadingAd)
        {
            return;
        }

        if (!isInitialized)
        {
            StartCoroutine(LoadAdWithDelay(3f));
            return;
        }

        isLoadingAd = true;

        if (rewardedAd != null)
        {
            try
            {
                rewardedAd.Destroy();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"광고 제거 실패: {e.Message}");
            }
            rewardedAd = null;
        }

        try
        {
            var adRequest = new AdRequest();

            RewardedAd.Load(rewardedAdUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    isLoadingAd = false;

                    if (error != null || ad == null)
                    {
                        isAdLoaded = false;
                        OnAdFailedToLoad?.Invoke();
                        StartCoroutine(LoadAdWithDelay(10f));
                        return;
                    }

                    rewardedAd = ad;
                    isAdLoaded = true;
                    RegisterEventHandlers(rewardedAd);
                });
        }
        catch (System.Exception e)
        {
            isLoadingAd = false;
            Debug.LogError($"광고 로드 예외: {e.Message}");
        }
    }

    private void RegisterEventHandlers(RewardedAd ad)
    {
        ad.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("광고 열림");
        };

        ad.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("광고 닫힘");
            OnAdClosed?.Invoke();
            StartCoroutine(LoadAdWithDelay(0.5f));
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"광고 표시 실패: {error.GetMessage()}");
            OnAdFailedToShow?.Invoke();
            StartCoroutine(LoadAdWithDelay(0.5f));
        };
    }

    public void ShowRewardedAd()
    {
        if (rewardedAd != null && isAdLoaded)
        {
            try
            {
                rewardedAd.Show((Reward reward) =>
                {
                    Debug.Log($"보상 획득: {reward.Type}, {reward.Amount}");
                    OnRewardEarned?.Invoke();
                });

                isAdLoaded = false;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"광고 표시 예외: {e.Message}");
                OnAdFailedToShow?.Invoke();
            }
        }
        else
        {
            OnAdFailedToShow?.Invoke();

            if (!isLoadingAd)
            {
                LoadRewardedAd();
            }
        }
    }

    public bool IsAdLoaded()
    {
        return isAdLoaded && rewardedAd != null;
    }

    public string GetAdUnitId()
    {
        return rewardedAdUnitId;
    }

    public bool IsInitialized()
    {
        return isInitialized;
    }

    private void OnDestroy()
    {
        if (rewardedAd != null)
        {
            rewardedAd.Destroy();
        }
    }
}