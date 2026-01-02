using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    GameManager gameManager;
    [SerializeField] TextMeshProUGUI highScoreText;
    [SerializeField] TextMeshProUGUI nowNumText;
    [SerializeField] TextMeshProUGUI nextNumText;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {
        gameManager = GameManager.Instance;
    }

    public void UpdateUI()
    {
        nowNumText.text = gameManager.nowNum.ToString();
        nextNumText.text = gameManager.nextNum.ToString();
    }
}
