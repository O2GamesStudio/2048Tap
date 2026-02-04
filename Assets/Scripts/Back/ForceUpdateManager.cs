using UnityEngine;
using Firebase;
using Firebase.RemoteConfig;
using Firebase.Extensions;
using System;
using System.Collections.Generic;

public class ForceUpdateManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject updatePopup;
    [SerializeField] private string currentVersion = "1.0.0";

    [Header("Remote Config Keys")]
    private const string KEY_LATEST_VERSION = "latest_version";
    private const string KEY_UPDATE_URL = "update_url";

    // 버전 체크 완료 콜백
    public event Action OnVersionCheckCompleted;

    private void Start()
    {
        // Application.version을 사용하려면 이 줄의 주석을 해제하세요
        // currentVersion = Application.version;

        CheckFirebaseAndVersion();
    }

    private void CheckFirebaseAndVersion()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase 초기화 성공");
                InitializeRemoteConfig();
            }
            else
            {
                Debug.LogWarning($"Firebase 초기화 실패: {task.Result}. 버전 체크 없이 게임 계속 진행");
                OnVersionCheckCompleted?.Invoke();
            }
        });
    }

    private void InitializeRemoteConfig()
    {
        // Remote Config 기본값 설정 (오프라인 대비)
        Dictionary<string, object> defaults = new Dictionary<string, object>
        {
            { KEY_LATEST_VERSION, "1.0.0" },
            { KEY_UPDATE_URL, "https://play.google.com/store/apps/details?id=com.o2studio.tap2048" }
        };

        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
            .ContinueWithOnMainThread(task =>
            {
                Debug.Log("Remote Config 기본값 설정 완료");
                FetchRemoteConfig();
            });
    }

    private void FetchRemoteConfig()
    {
        // Remote Config 데이터 가져오기 (캐시 시간: 0초로 항상 최신 데이터)
        // 프로덕션에서는 TimeSpan.FromHours(12) 등으로 설정 권장
        FirebaseRemoteConfig.DefaultInstance.FetchAsync(TimeSpan.Zero)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    Debug.Log("Remote Config Fetch 성공");
                    FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                        .ContinueWithOnMainThread(activateTask =>
                        {
                            Debug.Log("Remote Config Activate 완료");
                            CheckVersion();
                        });
                }
                else
                {
                    Debug.LogWarning("Remote Config Fetch 실패. 기본값으로 체크");
                    CheckVersion();
                }
            });
    }

    private void CheckVersion()
    {
        string latestVersion = FirebaseRemoteConfig.DefaultInstance.GetValue(KEY_LATEST_VERSION).StringValue;
        string updateUrl = FirebaseRemoteConfig.DefaultInstance.GetValue(KEY_UPDATE_URL).StringValue;

        Debug.Log($"=== 버전 체크 ===");
        Debug.Log($"현재 버전: {currentVersion}");
        Debug.Log($"최신 버전: {latestVersion}");
        Debug.Log($"업데이트 URL: {updateUrl}");

        // 버전 비교
        if (CompareVersion(currentVersion, latestVersion) < 0)
        {
            ShowUpdatePopup(updateUrl);
        }
        else
        {
            OnVersionCheckCompleted?.Invoke();
        }
    }
    private int CompareVersion(string version1, string version2)
    {
        try
        {
            string[] v1Parts = version1.Split('.');
            string[] v2Parts = version2.Split('.');

            int maxLength = Mathf.Max(v1Parts.Length, v2Parts.Length);

            for (int i = 0; i < maxLength; i++)
            {
                int v1 = i < v1Parts.Length ? int.Parse(v1Parts[i]) : 0;
                int v2 = i < v2Parts.Length ? int.Parse(v2Parts[i]) : 0;

                if (v1 < v2) return -1;
                if (v1 > v2) return 1;
            }

            return 0;
        }
        catch (Exception e)
        {
            return 0;
        }
    }

    private void ShowUpdatePopup(string updateUrl)
    {
        if (updatePopup == null)
        {
            Debug.LogError("UpdatePopup이 할당되지 않았습니다!");
            OnVersionCheckCompleted?.Invoke();
            return;
        }

        UpdatePopup popup = updatePopup.GetComponent<UpdatePopup>();
        if (popup != null)
        {
            // 활성화 전에 Initialize 호출
            popup.Initialize(updateUrl);
        }
        else
        {
            Debug.LogError("UpdatePopup 컴포넌트를 찾을 수 없습니다!");
            OnVersionCheckCompleted?.Invoke();
            return;
        }

        // Initialize 후에 활성화
        updatePopup.SetActive(true);
    }
}