using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    #region Singleton
    private static AudioManager instance;
    private AudioManager() { }
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
                if (instance == null)
                {
                    AudioManager audioManagerPrefab = Resources.Load<AudioManager>("Prefabs/Managers/AudioManager");
                    AudioManager go = Instantiate(audioManagerPrefab);
                    go.name = "AudioManager";
                    instance = go.GetComponent<AudioManager>();
                }
            }
            return instance;
        }
    }
    #endregion
    [HideInInspector] public AudioSource[] sources;
    [Header("Generic Clips")]
    public AudioClip[] genericClips;
    [Header("Battle Clips")]
    public AudioClip[] battleClips;
    AudioMixer mixer;
    AudioMixerGroup[] audioMixerGroups;

    public void PreInitialize()
    {
        ChachingComponents();
        audioMixerGroups = GetAudioMixerGroups;
    }

    private void ChachingComponents()
    {
        mixer = Resources.Load<AudioMixer>("Audio/AudioMixer");
        sources = GetComponentsInChildren<AudioSource>();
    }

    public AudioMixerGroup GetAudioMixerGroupChannel(string name)
    {
        return audioMixerGroups.Where(x => x.name.Equals(name)).FirstOrDefault();
    }

    /// <summary>
    /// params[] => [0] = PlayOnAwake [1] = Loop
    /// </summary>
    /// <param name="source"></param>
    /// <param name="clip"></param>
    /// <param name="routing"></param>
    /// <param name="ps"></param>
    /// <returns></returns>
    public bool InitializeSettingsAndRouting(AudioSource source, AudioClip clip, string routing, params bool[] ps)
    {
        source.clip = clip;
        source.playOnAwake = ps[0];
        source.loop = ps[1];
        source.outputAudioMixerGroup = GetAudioMixerGroupChannel(routing);
        return true;
    }

    /// <summary>
    /// Retrieve all groups in the Audio mixer
    /// </summary>
    public AudioMixerGroup[] GetAudioMixerGroups
    {
        get
        {
            return mixer.FindMatchingGroups(string.Empty);
        }
    }

    public void GetMusicVolume(out float volume)
    {
        mixer.GetFloat("MusicVolume", out volume);
    }

    public void GetSFXVolume(out float volume)
    {
        mixer.GetFloat("SFXVolume", out volume);
    }

    public void SetMusicVolume(float volume)
    {
        mixer.SetFloat("MusicVolume", volume);
        mixer.SetFloat("BGMusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        mixer.SetFloat("SFXVolume", volume);
        //mixer.SetFloat("ActionSFXVolume", volume);
        mixer.SetFloat("AmbianceVolume", volume);
        mixer.SetFloat("UISFXVolume", volume);
    }

    /// <summary>
    /// Play Sound
    /// </summary>
    public void InitializeRoutingAndPlay(AudioClip[] sounds, int groupid, int index, string routing, bool loop)
    {
            InitializeSettingsAndRouting(sources[groupid], sounds[index], routing, false, loop);
            sources[groupid].Play();
    }
}