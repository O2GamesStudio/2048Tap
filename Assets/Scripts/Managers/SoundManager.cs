using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("AudioSource Pool Settings")]
    [SerializeField] int initialPoolSize = 10;
    [SerializeField] int maxPoolSize = 20;

    [Header("Audio Clips")]
    [SerializeField] AudioClip[] sfxClips;
    [SerializeField] AudioClip[] bgmClips;
    public AudioClip buttonClickClip, uiBtnClickClip, combineComboClip, eraseClip, restoreClip, slideClip;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] float sfxVolume = 1f;
    [SerializeField][Range(0f, 1f)] float bgmVolume = 0.5f;

    private bool isSoundEnabled = true;
    private bool isMusicEnabled = true;

    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    private AudioSource bgmSource;

    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeSoundManager()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.playOnAwake = false;

        InitializeAudioClipDictionaries();

        isSoundEnabled = PlayerPrefs.GetInt("IsSoundOn", 1) == 1;
        isMusicEnabled = PlayerPrefs.GetInt("IsMusicOn", 1) == 1;

        Debug.Log($"SoundManager 초기화 완료 - 풀 크기: {audioSourcePool.Count}");
    }

    void InitializeAudioClipDictionaries()
    {
        if (sfxClips != null)
        {
            foreach (AudioClip clip in sfxClips)
            {
                if (clip != null && !sfxDictionary.ContainsKey(clip.name))
                {
                    sfxDictionary.Add(clip.name, clip);
                }
            }
        }

        if (bgmClips != null)
        {
            foreach (AudioClip clip in bgmClips)
            {
                if (clip != null && !bgmDictionary.ContainsKey(clip.name))
                {
                    bgmDictionary.Add(clip.name, clip);
                }
            }
        }
    }

    AudioSource CreateNewAudioSource()
    {
        GameObject audioObj = new GameObject("PooledAudioSource");
        audioObj.transform.SetParent(transform);
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = sfxVolume;
        audioSourcePool.Enqueue(audioSource);
        return audioSource;
    }

    AudioSource GetAudioSource()
    {
        AudioSource audioSource;

        while (audioSourcePool.Count > 0)
        {
            audioSource = audioSourcePool.Dequeue();
            if (audioSource != null && !audioSource.isPlaying)
            {
                activeAudioSources.Add(audioSource);
                return audioSource;
            }
        }

        if (activeAudioSources.Count < maxPoolSize)
        {
            audioSource = CreateNewAudioSource();
            audioSourcePool.Dequeue();
            activeAudioSources.Add(audioSource);
            Debug.Log($"새로운 AudioSource 생성됨 - 현재 활성: {activeAudioSources.Count}");
            return audioSource;
        }

        Debug.LogWarning("AudioSource 풀 최대 크기 도달 - 가장 오래된 소스 재사용");
        audioSource = activeAudioSources[0];
        audioSource.Stop();
        return audioSource;
    }

    void ReturnAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;

        audioSource.clip = null;
        audioSource.Stop();
        activeAudioSources.Remove(audioSource);

        if (!audioSourcePool.Contains(audioSource))
        {
            audioSourcePool.Enqueue(audioSource);
        }
    }

    public void SetSoundEnabled(bool enabled)
    {
        isSoundEnabled = enabled;

        if (!enabled)
        {
            StopAllSFX();
        }
    }

    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;

        if (bgmSource != null)
        {
            if (enabled)
            {
                if (!bgmSource.isPlaying && bgmSource.clip != null)
                    bgmSource.Play();
            }
            else
            {
                bgmSource.Pause();
            }
        }
    }

    public void PlayUIClickSFX(float volumeScale = 1f)
    {
        if (!isSoundEnabled) return;

        if (uiBtnClickClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다!");
            return;
        }

        AudioSource audioSource = GetAudioSource();
        audioSource.clip = uiBtnClickClip;
        audioSource.volume = sfxVolume * volumeScale;
        audioSource.Play();

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, uiBtnClickClip.length));
    }

    public void PlayBtnClickSFX(float volumeScale = 1f)
    {
        if (!isSoundEnabled) return;

        if (buttonClickClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다!");
            return;
        }

        AudioSource audioSource = GetAudioSource();
        audioSource.clip = buttonClickClip;
        audioSource.volume = sfxVolume * volumeScale;
        audioSource.Play();

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, buttonClickClip.length));
    }

    public void PlayUIBtnClickSFX(float volumeScale = 1f)
    {
        if (!isSoundEnabled) return;

        if (uiBtnClickClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다!");
            return;
        }

        AudioSource audioSource = GetAudioSource();
        audioSource.clip = uiBtnClickClip;
        audioSource.volume = sfxVolume * volumeScale;
        audioSource.Play();

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, uiBtnClickClip.length));
    }

    public void PlayMergeSFX(float volumeScale = 1f)
    {
        if (!isSoundEnabled) return;

        if (combineComboClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다!");
            return;
        }

        AudioSource audioSource = GetAudioSource();
        audioSource.clip = combineComboClip;
        audioSource.volume = sfxVolume * volumeScale;
        audioSource.Play();

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, combineComboClip.length));
    }

    public void PlayEraseSFX(float volumeScale = 1f)
    {
        if (!isSoundEnabled) return;

        if (eraseClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다!");
            return;
        }

        AudioSource audioSource = GetAudioSource();
        audioSource.clip = eraseClip;
        audioSource.volume = sfxVolume * volumeScale;
        audioSource.Play();

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, eraseClip.length));
    }

    public void PlayRestoreSFX(float volumeScale = 1f)
    {
        if (!isSoundEnabled) return;

        if (restoreClip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다!");
            return;
        }

        AudioSource audioSource = GetAudioSource();
        audioSource.clip = restoreClip;
        audioSource.volume = sfxVolume * volumeScale;
        audioSource.Play();

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, restoreClip.length));
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (!isSoundEnabled) return;

        if (clip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다!");
            return;
        }

        AudioSource audioSource = GetAudioSource();
        audioSource.clip = clip;
        audioSource.volume = sfxVolume * volumeScale;
        audioSource.Play();

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, clip.length));
    }

    public void PlaySFX(string clipName, float volumeScale = 1f)
    {
        if (!isSoundEnabled) return;

        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            PlaySFX(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning($"SFX '{clipName}'를 찾을 수 없습니다!");
        }
    }

    public void PlayOneShotSFX(AudioClip clip, float volumeScale = 1f)
    {
        if (!isSoundEnabled) return;
        if (clip == null) return;

        AudioSource audioSource = GetAudioSource();
        audioSource.PlayOneShot(clip, sfxVolume * volumeScale);

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, clip.length));
    }

    public void PlayOneShotSFX(string clipName, float volumeScale = 1f)
    {
        if (!isSoundEnabled) return;

        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            PlayOneShotSFX(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning($"SFX '{clipName}'를 찾을 수 없습니다!");
        }
    }

    IEnumerator ReturnToPoolAfterPlay(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        ReturnAudioSource(audioSource);
    }

    public void PlayBGM(bool loop = true)
    {
        if (!isMusicEnabled) return;
        if (bgmClips == null || bgmClips.Length == 0 || bgmClips[0] == null) return;

        if (bgmSource.clip == bgmClips[0] && bgmSource.isPlaying)
            return;

        bgmSource.clip = bgmClips[0];
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        if (isMusicEnabled)
            bgmSource.UnPause();
    }

    public void FadeBGM(float targetVolume, float duration)
    {
        StartCoroutine(FadeBGMCoroutine(targetVolume, duration));
    }

    IEnumerator FadeBGMCoroutine(float targetVolume, float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    public void StopAllSFX()
    {
        foreach (AudioSource source in activeAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }

        while (activeAudioSources.Count > 0)
        {
            ReturnAudioSource(activeAudioSources[0]);
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        foreach (AudioSource source in activeAudioSources)
        {
            if (source != null)
            {
                source.volume = sfxVolume;
            }
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
    }

    public void PrintPoolStatus()
    {
        Debug.Log($"풀 상태 - 사용 가능: {audioSourcePool.Count}, 활성: {activeAudioSources.Count}");
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}