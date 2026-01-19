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
    private string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111"; // 배너 테스트 ID
    // private string rewardedAdUnitId = "ca-app-pub-3490273194196393/8703866303"; // 실제 ID
    // private string bannerAdUnitId = "ca-app-pub-3490273194196393/4313652000"; // 실제 배너 ID로 변경
#else
    private string rewardedAdUnitId = "ca-app-pub-3940256099942544/5224354917"; 
    private string bannerAdUnitId = "ca-app-pub-3940256099942544/6300978111";
#endif

    private RewardedAd rewardedAd;
    private BannerView bannerView;
    private bool isAdLoaded = false;
    private bool isInitialized = false;
    private bool isLoadingAd = false;
    private bool isBannerLoaded = false;

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

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (!isAdLoaded && !isLoadingAd && isInitialized)
        {
            StartCoroutine(LoadAdWithDelay(0.5f));
        }
    }

    private void InitializeAds()
    {
        try
        {
            MobileAds.Initialize(initStatus =>
            {
                isInitialized = true;
                Debug.Log("AdMob 초기화 완료");
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

    #region 보상형 광고
    public void LoadRewardedAd()
    {
        if (isLoadingAd)
        {
            Debug.Log("이미 광고를 로딩 중입니다.");
            return;
        }

        if (!isInitialized)
        {
            Debug.Log("AdMob이 아직 초기화되지 않았습니다. 3초 후 재시도합니다.");
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

            Debug.Log("광고 로딩 시작...");

            RewardedAd.Load(rewardedAdUnitId, adRequest,
                (RewardedAd ad, LoadAdError error) =>
                {
                    isLoadingAd = false;

                    if (error != null || ad == null)
                    {
                        Debug.LogError($"광고 로드 실패: {error?.GetMessage()}");
                        isAdLoaded = false;
                        OnAdFailedToLoad?.Invoke();
                        StartCoroutine(LoadAdWithDelay(10f));
                        return;
                    }

                    rewardedAd = ad;
                    isAdLoaded = true;
                    Debug.Log("광고 로드 성공!");
                    RegisterEventHandlers(rewardedAd);
                });
        }
        catch (System.Exception e)
        {
            isLoadingAd = false;
            Debug.LogError($"광고 로드 예외: {e.Message}");
            StartCoroutine(LoadAdWithDelay(10f));
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
            isAdLoaded = false;
            OnAdClosed?.Invoke();
            StartCoroutine(LoadAdWithDelay(0.5f));
        };

        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError($"광고 표시 실패: {error.GetMessage()}");
            isAdLoaded = false;
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
                isAdLoaded = false;
                OnAdFailedToShow?.Invoke();
                StartCoroutine(LoadAdWithDelay(0.5f));
            }
        }
        else
        {
            Debug.Log("광고가 아직 로드되지 않았습니다.");
            OnAdFailedToShow?.Invoke();

            if (!isLoadingAd)
            {
                LoadRewardedAd();
            }
        }
    }
    #endregion

    #region 배너 광고
    public void LoadBannerAd()
    {
        if (!isInitialized)
        {
            Debug.Log("AdMob이 아직 초기화되지 않았습니다.");
            return;
        }

        // 기존 배너가 있다면 제거
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
        }

        // 배너 광고 생성 (하단 중앙)
        bannerView = new BannerView(bannerAdUnitId, AdSize.Banner, AdPosition.Bottom);

        // 배너 이벤트 등록
        RegisterBannerEvents();

        // 배너 광고 요청
        var adRequest = new AdRequest();
        bannerView.LoadAd(adRequest);

        Debug.Log("배너 광고 로딩 시작...");
    }

    private void RegisterBannerEvents()
    {
        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("배너 광고 로드 성공!");
            isBannerLoaded = true;
        };

        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError($"배너 광고 로드 실패: {error.GetMessage()}");
            isBannerLoaded = false;
        };

        bannerView.OnAdClicked += () =>
        {
            Debug.Log("배너 광고 클릭됨");
        };

        bannerView.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("배너 광고 전체 화면 열림");
        };

        bannerView.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("배너 광고 전체 화면 닫힘");
        };
    }

    public void ShowBanner()
    {
        if (bannerView != null)
        {
            bannerView.Show();
            Debug.Log("배너 광고 표시");
        }
    }

    public void HideBanner()
    {
        if (bannerView != null)
        {
            bannerView.Hide();
            Debug.Log("배너 광고 숨김");
        }
    }

    public void DestroyBanner()
    {
        if (bannerView != null)
        {
            bannerView.Destroy();
            bannerView = null;
            isBannerLoaded = false;
            Debug.Log("배너 광고 제거");
        }
    }

    public bool IsBannerLoaded()
    {
        return isBannerLoaded && bannerView != null;
    }
    #endregion

    public bool IsAdLoaded()
    {
        return isAdLoaded && rewardedAd != null;
    }

    public bool IsLoadingAd()
    {
        return isLoadingAd;
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

        if (bannerView != null)
        {
            bannerView.Destroy();
        }
    }
}