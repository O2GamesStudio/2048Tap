using System;
using UnityEngine;
using UnityEngine.Advertisements;
using System.Collections;

public class UnityAdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    private static UnityAdsManager instance;
    public static UnityAdsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("UnityAdsManager");
                instance = go.AddComponent<UnityAdsManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Unity Ads Settings")]
    [SerializeField] private string androidGameId = "YOUR_ANDROID_GAME_ID";
    [SerializeField] private string iOSGameId = "YOUR_IOS_GAME_ID";
    [SerializeField] private string androidRewardedAdUnitId = "Rewarded_Android";
    [SerializeField] private string iOSRewardedAdUnitId = "Rewarded_iOS";
    [SerializeField] private string androidBannerAdUnitId = "Banner_Android";
    [SerializeField] private string iOSBannerAdUnitId = "Banner_iOS";
    [SerializeField] private bool testMode = true; // 출시 시 false로 변경
    [SerializeField] private BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;

    // Unity 공식 테스트 ID (에디터 테스트용)
    private const string EDITOR_TEST_GAME_ID = "14851"; // Unity 공식 테스트 Game ID
    private const string EDITOR_TEST_REWARDED_AD_UNIT_ID = "Rewarded_Android";
    private const string EDITOR_TEST_BANNER_AD_UNIT_ID = "Banner_Android";

    private string gameId;
    private string rewardedAdUnitId;
    private string bannerAdUnitId;

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

            // 플랫폼별 ID 설정
#if UNITY_ANDROID
            gameId = androidGameId;
            rewardedAdUnitId = androidRewardedAdUnitId;
            bannerAdUnitId = androidBannerAdUnitId;
#elif UNITY_IOS
            gameId = iOSGameId;
            rewardedAdUnitId = iOSRewardedAdUnitId;
            bannerAdUnitId = iOSBannerAdUnitId;
#else
            // 에디터에서는 Unity 공식 테스트 ID 사용
            gameId = EDITOR_TEST_GAME_ID;
            rewardedAdUnitId = EDITOR_TEST_REWARDED_AD_UNIT_ID;
            bannerAdUnitId = EDITOR_TEST_BANNER_AD_UNIT_ID;
            Debug.Log("Unity Editor: Using official test IDs for testing");
#endif

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
            if (!Advertisement.isInitialized && Advertisement.isSupported)
            {
                Advertisement.Initialize(gameId, testMode, this);
                Debug.Log("Unity Ads 초기화 시작...");
            }
            else if (Advertisement.isInitialized)
            {
                isInitialized = true;
                Debug.Log("Unity Ads 이미 초기화됨");
                StartCoroutine(LoadAdWithDelay(0.5f));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Unity Ads 초기화 실패: {e.Message}");
        }
    }

    // IUnityAdsInitializationListener 구현
    public void OnInitializationComplete()
    {
        isInitialized = true;
        Debug.Log("Unity Ads 초기화 완료");
        StartCoroutine(LoadAdWithDelay(0.5f));
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads 초기화 실패: {error.ToString()} - {message}");
        isInitialized = false;
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
            Debug.Log("Unity Ads가 아직 초기화되지 않았습니다. 3초 후 재시도합니다.");
            StartCoroutine(LoadAdWithDelay(3f));
            return;
        }

        isLoadingAd = true;
        isAdLoaded = false;

        try
        {
            Debug.Log("보상형 광고 로딩 시작...");
            Advertisement.Load(rewardedAdUnitId, this);
        }
        catch (System.Exception e)
        {
            isLoadingAd = false;
            Debug.LogError($"광고 로드 예외: {e.Message}");
            OnAdFailedToLoad?.Invoke();
            StartCoroutine(LoadAdWithDelay(10f));
        }
    }

    // IUnityAdsLoadListener 구현
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId.Equals(rewardedAdUnitId))
        {
            isLoadingAd = false;
            isAdLoaded = true;
            Debug.Log("보상형 광고 로드 성공!");
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        if (adUnitId.Equals(rewardedAdUnitId))
        {
            isLoadingAd = false;
            isAdLoaded = false;
            Debug.LogError($"보상형 광고 로드 실패: {error.ToString()} - {message}");
            OnAdFailedToLoad?.Invoke();
            StartCoroutine(LoadAdWithDelay(10f));
        }
    }

    public void ShowRewardedAd()
    {
        if (isAdLoaded && isInitialized)
        {
            try
            {
                Debug.Log("보상형 광고 표시 시도...");
                Advertisement.Show(rewardedAdUnitId, this);
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

    // IUnityAdsShowListener 구현
    public void OnUnityAdsShowStart(string adUnitId)
    {
        if (adUnitId.Equals(rewardedAdUnitId))
        {
            Debug.Log("보상형 광고 시작");
        }
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        if (adUnitId.Equals(rewardedAdUnitId))
        {
            Debug.Log("보상형 광고 클릭됨");
        }
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(rewardedAdUnitId))
        {
            if (showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
                Debug.Log("보상형 광고 시청 완료 - 보상 지급");
                OnRewardEarned?.Invoke();
            }
            else
            {
                Debug.Log($"보상형 광고 미완료: {showCompletionState}");
            }

            Debug.Log("광고 닫힘");
            OnAdClosed?.Invoke();
            StartCoroutine(LoadAdWithDelay(0.5f));
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        if (adUnitId.Equals(rewardedAdUnitId))
        {
            Debug.LogError($"보상형 광고 표시 실패: {error.ToString()} - {message}");
            isAdLoaded = false;
            OnAdFailedToShow?.Invoke();
            StartCoroutine(LoadAdWithDelay(0.5f));
        }
    }
    #endregion

    #region 배너 광고
    public void LoadBannerAd()
    {
        if (!isInitialized)
        {
            Debug.Log("Unity Ads가 아직 초기화되지 않았습니다. 배너 로드 대기 중...");
            StartCoroutine(LoadBannerAfterInit());
            return;
        }

        try
        {
            // 배너 위치 설정
            Advertisement.Banner.SetPosition(bannerPosition);

            // 배너 로드 옵션 설정
            BannerLoadOptions options = new BannerLoadOptions
            {
                loadCallback = OnBannerLoaded,
                errorCallback = OnBannerError
            };

            Debug.Log($"배너 광고 로딩 시작... (Ad Unit: {bannerAdUnitId}, Position: {bannerPosition})");
            Advertisement.Banner.Load(bannerAdUnitId, options);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"배너 광고 로드 예외: {e.Message}");
        }
    }

    private IEnumerator LoadBannerAfterInit()
    {
        // 초기화 완료 대기 (최대 10초)
        float waitTime = 0f;
        while (!isInitialized && waitTime < 10f)
        {
            yield return new WaitForSeconds(0.5f);
            waitTime += 0.5f;
        }

        if (isInitialized)
        {
            Debug.Log("Unity Ads 초기화 완료 - 배너 로드 시작");
            LoadBannerAd();
        }
        else
        {
            Debug.LogError("Unity Ads 초기화 타임아웃 - 배너 로드 실패");
        }
    }

    private void OnBannerLoaded()
    {
        Debug.Log("배너 광고 로드 성공!");
        isBannerLoaded = true;
    }

    private void OnBannerError(string message)
    {
        Debug.LogError($"배너 광고 로드 실패: {message}");
        isBannerLoaded = false;
    }

    public void ShowBanner()
    {
        if (isBannerLoaded)
        {
            BannerOptions options = new BannerOptions
            {
                clickCallback = OnBannerClicked,
                hideCallback = OnBannerHidden,
                showCallback = OnBannerShown
            };

            Advertisement.Banner.Show(bannerAdUnitId, options);
            Debug.Log("배너 광고 표시");
        }
        else
        {
            Debug.Log("배너가 로드되지 않았습니다. 로드를 시도합니다.");
            LoadBannerAd();
        }
    }

    public void HideBanner()
    {
        Advertisement.Banner.Hide();
        Debug.Log("배너 광고 숨김");
    }

    public void DestroyBanner()
    {
        Advertisement.Banner.Hide();
        isBannerLoaded = false;
        Debug.Log("배너 광고 제거");
    }

    private void OnBannerClicked()
    {
        Debug.Log("배너 광고 클릭됨");
    }

    private void OnBannerShown()
    {
        Debug.Log("배너 광고 표시됨");
    }

    private void OnBannerHidden()
    {
        Debug.Log("배너 광고 숨겨짐");
    }

    public bool IsBannerLoaded()
    {
        return isBannerLoaded;
    }

    public void SetBannerPosition(BannerPosition position)
    {
        bannerPosition = position;
        Advertisement.Banner.SetPosition(position);
        Debug.Log($"배너 위치 변경: {position}");
    }
    #endregion

    public bool IsAdLoaded()
    {
        return isAdLoaded;
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

    public string GetGameId()
    {
        return gameId;
    }

    public bool IsTestMode()
    {
        return testMode;
    }

    private void OnDestroy()
    {
        // Unity Ads는 자동으로 정리되므로 별도 처리 불필요
        Debug.Log("UnityAdsManager 제거됨");
    }
}