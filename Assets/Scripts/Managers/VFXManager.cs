using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Particle Settings")]
    [SerializeField] ParticleSystem[] combineParticlePrefabs;
    [SerializeField] int initialPoolSize = 5;
    [SerializeField] int maxPoolSize = 10;

    private Dictionary<int, Queue<ParticleSystem>> particlePools = new Dictionary<int, Queue<ParticleSystem>>();
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
        if (combineParticlePrefabs == null || combineParticlePrefabs.Length == 0)
        {
            Debug.LogWarning("CombineParticlePrefabs가 할당되지 않았습니다!");
            return;
        }

        for (int i = 0; i < combineParticlePrefabs.Length; i++)
        {
            particlePools[i] = new Queue<ParticleSystem>();

            for (int j = 0; j < initialPoolSize; j++)
            {
                CreateNewParticle(i);
            }
        }

        Debug.Log($"VFXManager 초기화 완료 - {combineParticlePrefabs.Length}개 타입의 파티클 풀 생성");
    }

    ParticleSystem CreateNewParticle(int prefabIndex)
    {
        if (prefabIndex < 0 || prefabIndex >= combineParticlePrefabs.Length)
        {
            Debug.LogError($"잘못된 파티클 인덱스: {prefabIndex}");
            return null;
        }

        ParticleSystem particle = Instantiate(combineParticlePrefabs[prefabIndex], transform);
        particle.gameObject.SetActive(false);
        particlePools[prefabIndex].Enqueue(particle);
        return particle;
    }

    ParticleSystem GetParticle(int prefabIndex)
    {
        if (prefabIndex < 0 || prefabIndex >= combineParticlePrefabs.Length)
        {
            Debug.LogError($"잘못된 파티클 인덱스: {prefabIndex}");
            return null;
        }

        if (!particlePools.ContainsKey(prefabIndex))
        {
            particlePools[prefabIndex] = new Queue<ParticleSystem>();
        }

        ParticleSystem particle;

        while (particlePools[prefabIndex].Count > 0)
        {
            particle = particlePools[prefabIndex].Dequeue();
            if (particle != null && !particle.isPlaying)
            {
                activeParticles.Add(particle);
                return particle;
            }
        }

        if (activeParticles.Count < maxPoolSize)
        {
            particle = CreateNewParticle(prefabIndex);
            particlePools[prefabIndex].Dequeue();
            activeParticles.Add(particle);
            return particle;
        }

        Debug.LogWarning("파티클 풀 최대 크기 도달 - 가장 오래된 파티클 재사용");
        particle = activeParticles[0];
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        return particle;
    }

    void ReturnParticle(ParticleSystem particle, int prefabIndex)
    {
        if (particle == null) return;

        particle.gameObject.SetActive(false);
        activeParticles.Remove(particle);

        if (particlePools.ContainsKey(prefabIndex) && !particlePools[prefabIndex].Contains(particle))
        {
            particlePools[prefabIndex].Enqueue(particle);
        }
    }

    int GetPrefabIndexByNumber(int number)
    {
        switch (number)
        {
            case 16: return 0;
            case 32: return 1;
            case 64: return 2;
            case 128: return 3;
            case 256: return 4;
            default: return 0;
        }
    }

    public void PlayCombineParticle(Vector3 position, int number)
    {
        int prefabIndex = GetPrefabIndexByNumber(number);

        if (combineParticlePrefabs == null || combineParticlePrefabs.Length == 0) return;
        if (prefabIndex >= combineParticlePrefabs.Length) prefabIndex = combineParticlePrefabs.Length - 1;

        ParticleSystem particle = GetParticle(prefabIndex);
        if (particle == null) return;

        particle.transform.position = new Vector3(position.x, position.y, 0f);
        particle.gameObject.SetActive(true);

        particle.Play();

        StartCoroutine(ReturnToPoolAfterPlay(particle, prefabIndex));
    }

    public void PlayCombineParticle(Transform target, int number)
    {
        PlayCombineParticle(target.position, number);
    }

    IEnumerator ReturnToPoolAfterPlay(ParticleSystem particle, int prefabIndex)
    {
        yield return new WaitForSeconds(particle.main.duration + particle.main.startLifetime.constantMax);
        ReturnParticle(particle, prefabIndex);
    }

    public void StopAllParticles()
    {
        foreach (ParticleSystem particle in activeParticles)
        {
            if (particle != null && particle.isPlaying)
            {
                particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        activeParticles.Clear();
    }

    public void PrintPoolStatus()
    {
        int totalAvailable = 0;
        foreach (var pool in particlePools.Values)
        {
            totalAvailable += pool.Count;
        }
        Debug.Log($"파티클 풀 상태 - 사용 가능: {totalAvailable}, 활성: {activeParticles.Count}");
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}