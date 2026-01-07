using System.Security;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class SettingPanel : MonoBehaviour
{
    [SerializeField] Button retryBtn, toLobbyBtn, closeBtn;

    [SerializeField] Button soundBtn, musicBtn;
    [SerializeField] Image soundToggleImage, musicToggleImage;
    [SerializeField] Image soundHandleImage, musicHandleImage;

    [SerializeField] bool isSoundOn = true;
    [SerializeField] bool isMusicOn = true;

    void Awake()
    {
        if (closeBtn != null)
            closeBtn.onClick.AddListener(CloseOnClick);
        if (retryBtn != null)
            retryBtn.onClick.AddListener(ResumeOnClick);
        if (toLobbyBtn != null)
            toLobbyBtn.onClick.AddListener(ToLobbyOnClick);

        if (soundBtn != null)
            soundBtn.onClick.AddListener(SoundOnClick);
        if (musicBtn != null)
            musicBtn.onClick.AddListener(MusicOnClick);
    }

    void Start()
    {
        isSoundOn = PlayerPrefs.GetInt("IsSoundOn", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("IsMusicOn", 1) == 1;

        UpdateUI();
    }

    public void UpdateUI()
    {
        UpdateSoundUI();
        UpdateMusicUI();
    }

    void UpdateSoundUI()
    {
        if (soundToggleImage != null)
        {
            soundToggleImage.gameObject.SetActive(isSoundOn);
        }

        if (soundHandleImage != null)
        {
            RectTransform handleRect = soundHandleImage.GetComponent<RectTransform>();
            float targetX = isSoundOn ? 50f : -50f;
            handleRect.DOAnchorPosX(targetX, 0.3f).SetEase(Ease.OutCubic);

            soundHandleImage.color = isSoundOn ? new Color(0x36 / 255f, 0x26 / 255f, 0x7E / 255f) : Color.white;
        }
    }

    void UpdateMusicUI()
    {
        if (musicToggleImage != null)
        {
            musicToggleImage.gameObject.SetActive(isMusicOn);
        }

        if (musicHandleImage != null)
        {
            RectTransform handleRect = musicHandleImage.GetComponent<RectTransform>();
            float targetX = isMusicOn ? 50f : -50f;
            handleRect.DOAnchorPosX(targetX, 0.3f).SetEase(Ease.OutCubic);

            musicHandleImage.color = isMusicOn ? new Color(0x36 / 255f, 0x26 / 255f, 0x7E / 255f) : Color.white;
        }
    }

    void SoundOnClick()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("IsSoundOn", isSoundOn ? 1 : 0);

        UpdateSoundUI();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSoundEnabled(isSoundOn);
            if (isSoundOn)
                SoundManager.Instance.PlayUIBtnClickSFX();
        }
    }

    void MusicOnClick()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("IsMusicOn", isMusicOn ? 1 : 0);

        UpdateMusicUI();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetMusicEnabled(isMusicOn);

            if (isMusicOn)
            {
                SoundManager.Instance.PlayBGM();
            }
            else
            {
                SoundManager.Instance.PauseBGM();
            }

            SoundManager.Instance.PlayUIBtnClickSFX();
        }
    }

    void CloseOnClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIBtnClickSFX();
        this.gameObject.SetActive(false);
    }

    void ResumeOnClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIBtnClickSFX();
        this.gameObject.SetActive(false);
    }

    void ToLobbyOnClick()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIBtnClickSFX();
        SceneManager.LoadScene(0);
    }
}