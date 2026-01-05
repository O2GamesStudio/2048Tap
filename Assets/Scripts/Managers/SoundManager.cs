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
    [SerializeField] AudioClip buttonClickClip;

    [Header("Volume Settings")]
    [SerializeField][Range(0f, 1f)] float sfxVolume = 1f;
    [SerializeField][Range(0f, 1f)] float bgmVolume = 0.5f;

    // AudioSource 풀
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();

    // BGM 전용 AudioSource
    private AudioSource bgmSource;

    // AudioClip Dictionary (이름으로 빠른 접근)
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> bgmDictionary = new Dictionary<string, AudioClip>();

    void Awake()
    {
        // 싱글톤 패턴
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
        // AudioSource 풀 초기화
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }

        // BGM 전용 AudioSource 생성
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.playOnAwake = false;

        // AudioClip Dictionary 초기화
        InitializeAudioClipDictionaries();

        Debug.Log($"SoundManager 초기화 완료 - 풀 크기: {audioSourcePool.Count}");
    }

    void InitializeAudioClipDictionaries()
    {
        // SFX Dictionary
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

        // BGM Dictionary
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

        // 풀에서 사용 가능한 AudioSource 찾기
        while (audioSourcePool.Count > 0)
        {
            audioSource = audioSourcePool.Dequeue();
            if (audioSource != null && !audioSource.isPlaying)
            {
                activeAudioSources.Add(audioSource);
                return audioSource;
            }
        }

        // 풀에 없으면 새로 생성 (최대 크기 체크)
        if (activeAudioSources.Count < maxPoolSize)
        {
            audioSource = CreateNewAudioSource();
            audioSourcePool.Dequeue(); // 방금 추가한 것을 빼냄
            activeAudioSources.Add(audioSource);
            Debug.Log($"새로운 AudioSource 생성됨 - 현재 활성: {activeAudioSources.Count}");
            return audioSource;
        }

        // 최대 크기 도달 시 가장 오래된 활성 AudioSource 재사용
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

    public void PlayBtnClickSFX(float volumeScale = 1f)
    {
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
    // SFX 재생 (AudioClip 직접 전달)
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
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

    // SFX 재생 (이름으로 검색)
    public void PlaySFX(string clipName, float volumeScale = 1f)
    {
        if (sfxDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            PlaySFX(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning($"SFX '{clipName}'를 찾을 수 없습니다!");
        }
    }

    // OneShot 방식 (더 짧은 사운드에 적합)
    public void PlayOneShotSFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        AudioSource audioSource = GetAudioSource();
        audioSource.PlayOneShot(clip, sfxVolume * volumeScale);

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, clip.length));
    }

    public void PlayOneShotSFX(string clipName, float volumeScale = 1f)
    {
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
        yield return new WaitForSeconds(delay + 0.1f); // 약간의 여유 시간
        ReturnAudioSource(audioSource);
    }

    // BGM 재생
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void PlayBGM(string clipName, bool loop = true)
    {
        if (bgmDictionary.TryGetValue(clipName, out AudioClip clip))
        {
            PlayBGM(clip, loop);
        }
        else
        {
            Debug.LogWarning($"BGM '{clipName}'를 찾을 수 없습니다!");
        }
    }

    // BGM 정지
    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // BGM 일시정지/재개
    public void PauseBGM()
    {
        bgmSource.Pause();
    }

    public void ResumeBGM()
    {
        bgmSource.UnPause();
    }

    // BGM 페이드 인/아웃
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

    // 모든 SFX 정지
    public void StopAllSFX()
    {
        foreach (AudioSource source in activeAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }

        // 모든 활성 소스를 풀로 반환
        while (activeAudioSources.Count > 0)
        {
            ReturnAudioSource(activeAudioSources[0]);
        }
    }

    // 볼륨 설정
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

    // 풀 상태 확인 (디버깅용)
    public void PrintPoolStatus()
    {
        Debug.Log($"풀 상태 - 사용 가능: {audioSourcePool.Count}, 활성: {activeAudioSources.Count}");
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}