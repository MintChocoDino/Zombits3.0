using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Pool")]
    public int poolSize = 16;

    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume = 1f;

    [Header("3D Defaults")]
    public float max3DDistance = 20f;

    private readonly List<AudioSource> pool = new List<AudioSource>();
    private int nextPoolIndex;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        for (int i = 0; i < poolSize; i++)
            pool.Add(CreatePooledSource());
    }

    private AudioSource CreatePooledSource()
    {
        GameObject go = new GameObject("PooledAudioSource");
        go.transform.SetParent(transform);
        AudioSource src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        return src;
    }

    private AudioSource GetNextSource()
    {
        AudioSource src = pool[nextPoolIndex];
        nextPoolIndex = (nextPoolIndex + 1) % pool.Count;
        return src;
    }

    public void PlayAt(AudioClip clip, Vector3 position, float volumeScale = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        AudioSource src = GetNextSource();
        src.transform.position = position;
        src.clip = clip;
        src.spatialBlend = 1f;
        src.minDistance = 1f;
        src.maxDistance = max3DDistance;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.volume = masterVolume * volumeScale;
        src.pitch = pitch;
        src.loop = false;
        src.Play();
    }

    public void PlayRandomAt(AudioClip[] clips, Vector3 position, float volumeScale = 1f, float pitchJitter = 0.05f)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip pick = clips[Random.Range(0, clips.Length)];
        float pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        PlayAt(pick, position, volumeScale, pitch);
    }
}