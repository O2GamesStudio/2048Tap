using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Particle Settings")]
    [SerializeField] ParticleSystem combineParticlePrefab;
    [SerializeField] int initialPoolSize = 5;
    [SerializeField] int maxPoolSize = 10;

    [Header("Particle Colors by Number")]
    [SerializeField] Color color16 = new Color(1f, 0.58f, 0.38f);  // #F59563
    [SerializeField] Color color32 = new Color(0.96f, 0.48f, 0.37f); // #F67B5F
    [SerializeField] Color color64 = new Color(0.96f, 0.37f, 0.23f); // #F65E3B
    [SerializeField] Color color128 = new Color(0.93f, 0.81f, 0.44f); // #EDCF71
    [SerializeField] Color color256 = new Color(0.93f, 0.8f, 0.38f);  // #EDCC61
    [SerializeField] Color colorDefault = new Color(1f, 0.84f, 0.0f); // 골드

    // 파티클 풀
    private Queue<ParticleSystem> particlePool = new Queue<ParticleSystem>();
    private List<ParticleSystem> activeParticles = new List<ParticleSystem>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeVFXManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeVFXManager()
    {
        if (combineParticlePrefab == null)
        {
            Debug.LogWarning("CombineParticlePrefab이 할당되지 않았습니다!");
            return;
        }

        // 파티클 풀 초기화
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewParticle();
        }

        Debug.Log($"VFXManager 초기화 완료 - 파티클 풀 크기: {particlePool.Count}");
    }

    ParticleSystem CreateNewParticle()
    {
        ParticleSystem particle = Instantiate(combineParticlePrefab, transform);
        particle.gameObject.SetActive(false);
        particlePool.Enqueue(particle);
        return particle;
    }

    ParticleSystem GetParticle()
    {
        ParticleSystem particle;

        // 풀에서 사용 가능한 파티클 찾기
        while (particlePool.Count > 0)
        {
            particle = particlePool.Dequeue();
            if (particle != null && !particle.isPlaying)
            {
                activeParticles.Add(particle);
                return particle;
            }
        }

        // 풀에 없으면 새로 생성 (최대 크기 체크)
        if (activeParticles.Count < maxPoolSize)
        {
            particle = CreateNewParticle();
            particlePool.Dequeue();
            activeParticles.Add(particle);
            Debug.Log($"새로운 파티클 생성됨 - 현재 활성: {activeParticles.Count}");
            return particle;
        }

        // 최대 크기 도달 시 가장 오래된 파티클 재사용
        Debug.LogWarning("파티클 풀 최대 크기 도달 - 가장 오래된 파티클 재사용");
        particle = activeParticles[0];
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return particle;
    }

    void ReturnParticle(ParticleSystem particle)
    {
        if (particle == null) return;

        particle.gameObject.SetActive(false);
        activeParticles.Remove(particle);

        if (!particlePool.Contains(particle))
        {
            particlePool.Enqueue(particle);
        }
    }

    // 숫자에 따른 색상 반환
    Color GetColorByNumber(int number)
    {
        switch (number)
        {
            case 16: return color16;
            case 32: return color32;
            case 64: return color64;
            case 128: return color128;
            case 256: return color256;
            default: return colorDefault;
        }
    }

    public void PlayCombineParticle(Vector3 position)
    {
        PlayCombineParticle(position, colorDefault);
    }

    public void PlayCombineParticle(Vector3 position, Color color)
    {
        if (combineParticlePrefab == null) return;

        ParticleSystem particle = GetParticle();
        if (particle == null) return;

        particle.transform.position = new Vector3(position.x, position.y, 0f);
        particle.gameObject.SetActive(true);

        var main = particle.main;
        main.startColor = color;

        particle.Play();

        StartCoroutine(ReturnToPoolAfterPlay(particle));
    }

    public void PlayCombineParticle(Vector3 position, int number)
    {
        Color color = GetColorByNumber(number);
        PlayCombineParticle(position, color);
    }

    public void PlayCombineParticle(Transform target, int number)
    {
        PlayCombineParticle(target.position, number);
    }

    IEnumerator ReturnToPoolAfterPlay(ParticleSystem particle)
    {
        // 파티클 재생 시간만큼 대기
        yield return new WaitForSeconds(particle.main.duration + particle.main.startLifetime.constantMax);
        ReturnParticle(particle);
    }

    // 모든 파티클 정지
    public void StopAllParticles()
    {
        foreach (ParticleSystem particle in activeParticles)
        {
            if (particle != null && particle.isPlaying)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        while (activeParticles.Count > 0)
        {
            ReturnParticle(activeParticles[0]);
        }
    }

    // 풀 상태 확인 (디버깅용)
    public void PrintPoolStatus()
    {
        Debug.Log($"파티클 풀 상태 - 사용 가능: {particlePool.Count}, 활성: {activeParticles.Count}");
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}