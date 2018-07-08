using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    private List<AudioSource> audioSources = new List<AudioSource>();
    private GameObject mainCamera;
    public AudioSource preparedAudioSource;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void Start()
    {
        mainCamera = GameObject.Find("MainCamera");
    }

    public void CreateSound(string name, bool randomizePitch, bool fade)
    {
        AudioSource audioSource = mainCamera.AddComponent<AudioSource>();
        audioSource.volume = MainManager.instance.volume;
        if (randomizePitch) audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.clip = Resources.Load(name) as AudioClip;
        if (fade)
        {
            StartCoroutine(SoundFadeIn(audioSource));
        }
        audioSource.Play();
    }

    public AudioSource PrepareSound(AudioClip clip)
    {
        preparedAudioSource = mainCamera.AddComponent<AudioSource>();
        preparedAudioSource.volume = MainManager.instance.volume;
        preparedAudioSource.clip = clip;
        return preparedAudioSource;
    }

    public AudioSource PrepareSound(AudioClip clip, AudioSource source)
    {
        preparedAudioSource = source;
        preparedAudioSource.volume = MainManager.instance.volume;
        preparedAudioSource.clip = clip;
        return preparedAudioSource;
    }

    public void PlayPreparedSound()
    {
        StartCoroutine(SoundFadeIn(preparedAudioSource));
    }

    public void DestroyAllSounds()
    {
        AudioSource[] tempAudioSources = mainCamera.GetComponents<AudioSource>();
        int i;
        for (i = 0; i < tempAudioSources.Length; i++)
        {
            Destroy(tempAudioSources[i]);
        }
    }

    public void AdjustVolume()
    {
        AudioSource[] tempAudioSources = mainCamera.GetComponents<AudioSource>();
        int i;
        for (i = 0; i < tempAudioSources.Length; i++)
        {
            tempAudioSources[i].volume = MainManager.instance.volume;
        }
    }

    public void StopSounds()
    {
        AudioSource[] tempAudioSources = mainCamera.GetComponents<AudioSource>();
        int i;
        for (i = 0; i < tempAudioSources.Length; i++)
        {
            if (tempAudioSources[i] == preparedAudioSource) continue;
            StartCoroutine(SoundFadeOut(tempAudioSources[i]));
        }
    }

    public IEnumerator SoundPlayCheck(AudioSource source)
    {
        while (source.isPlaying)
        {
            yield return new WaitForSeconds(1f);
        }
        StartCoroutine(SoundFadeOut(source));
    }

    IEnumerator SoundFadeOut(AudioSource source)
    {
        while (source.volume > 0)
        {
            source.volume -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        audioSources.Remove(source);
        Destroy(source);
    }

    IEnumerator SoundFadeIn(AudioSource source)
    {
        source.volume = 0;
        source.Play();
        while (source.volume < MainManager.instance.volume)
        {
            source.volume = source.volume + 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
