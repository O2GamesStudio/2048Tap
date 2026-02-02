using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class DebugLogDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject debugPanel;
    [SerializeField] private TextMeshProUGUI logText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button clearButton;

    [Header("Settings")]
    [SerializeField] private int maxLogLines = 50;
    [SerializeField] private bool showOnStart = true;
    [SerializeField] private bool logToFile = false;

    private StringBuilder logs = new StringBuilder();
    private int lineCount = 0;
    private static DebugLogDisplay instance;

    public static DebugLogDisplay Instance => instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (debugPanel != null)
        {
            debugPanel.SetActive(showOnStart);
        }

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleDebugPanel);
        }

        if (clearButton != null)
        {
            clearButton.onClick.AddListener(ClearLogs);
        }
    }

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;

        // 디바이스 정보 표시
        LogDeviceInfo();
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void LogDeviceInfo()
    {
        AddLog("=== Unity Ads 디버그 시작 ===", LogType.Log);
        AddLog($"인터넷: {Application.internetReachability}", LogType.Log);
#if !UNITY_EDITOR
        AddLog($"디바이스: {SystemInfo.deviceModel}", LogType.Log);
#else
        AddLog("Unity 에디터 모드", LogType.Log);
#endif
        AddLog("===========================", LogType.Log);
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Unity Ads 관련 중요 로그만 필터링
        bool isRelevant = false;

        // 에러와 경고는 항상 표시
        if (type == LogType.Error || type == LogType.Warning || type == LogType.Exception)
        {
            isRelevant = true;
        }
        // Unity Ads 관련 키워드만 표시
        else if (logString.Contains("Unity Ads") ||
                 logString.Contains("광고") ||
                 logString.Contains("Ad Unit") ||
                 logString.Contains("Game ID") ||
                 logString.Contains("초기화") ||
                 logString.Contains("로드") ||
                 logString.Contains("배너") ||
                 logString.Contains("보상") ||
                 logString.Contains("==="))
        {
            isRelevant = true;
        }

        if (isRelevant)
        {
            AddLog(logString, type);
        }
    }

    void AddLog(string logString, LogType type)
    {
        string coloredLog = FormatLog(logString, type);

        logs.AppendLine(coloredLog);
        lineCount++;

        // 최대 라인 수 초과 시 오래된 로그 제거
        if (lineCount > maxLogLines)
        {
            int firstLineEnd = logs.ToString().IndexOf('\n');
            if (firstLineEnd > 0)
            {
                logs.Remove(0, firstLineEnd + 1);
                lineCount--;
            }
        }

        UpdateLogText();

        // 스크롤을 맨 아래로
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    string FormatLog(string logString, LogType type)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string color = GetColorForLogType(type);
        string prefix = GetPrefixForLogType(type);

        return $"<color={color}>[{timestamp}] {prefix} {logString}</color>";
    }

    string GetColorForLogType(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                return "#FF4444"; // 빨강
            case LogType.Warning:
                return "#FFAA00"; // 주황
            case LogType.Log:
                return "#FFFFFF"; // 흰색
            default:
                return "#AAAAAA"; // 회색
        }
    }

    string GetPrefixForLogType(LogType type)
    {
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                return "[ERROR]";
            case LogType.Warning:
                return "[WARN]";
            case LogType.Log:
                return "[INFO]";
            default:
                return "[ ]";
        }
    }

    void UpdateLogText()
    {
        if (logText != null)
        {
            logText.text = logs.ToString();
        }
    }

    public void ToggleDebugPanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(!debugPanel.activeSelf);
        }
    }

    public void ClearLogs()
    {
        logs.Clear();
        lineCount = 0;
        UpdateLogText();
        LogDeviceInfo();
        Debug.Log("로그 클리어됨");
    }

    public void ShowDebugPanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(true);
        }
    }

    public void HideDebugPanel()
    {
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }

    // 외부에서 직접 로그 추가 가능
    public static void Log(string message)
    {
        Debug.Log(message);
    }

    public static void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    public static void LogError(string message)
    {
        Debug.LogError(message);
    }
}