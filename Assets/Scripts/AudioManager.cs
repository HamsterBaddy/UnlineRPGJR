﻿using UnityEngine.Audio;
using System;
using UnityEngine;
using Unity.Netcode;

public class AudioManager : NetworkBehaviour
{
    public static AudioManager Instance { set; get; }

    public Sound[] sounds;

    public Sound[] sfxs;

    public SceneAudio[] sceneAudios;

    public AreaAudio[] areaAudios;

    public static AudioManager instance;

    public bool battle = false;

    public bool doPlay = false;

    public bool muteAudio = false;
    public bool muteSFX = false;

    private bool last_muteAudio = false;
    private bool last_muteSFX = false;

    public float standardAudioVolume = 1f;
    public float standardSFXVolume = 0.6f;

    public string standardSoundTrack = "Vulcano";

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        foreach ( Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.spatialBlend = s.spatialBlend;
        }

        foreach (Sound s in sfxs)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;

            s.source.spatialBlend = s.spatialBlend;
        }
    }

    private void Start()
    {
        PlayAudio(standardSoundTrack);
    }

    private void Update()
    {
        if (muteAudio && !last_muteAudio)
        {
            setVolumeAudioAll(0);
        }
        else
        {
            if (!muteAudio && last_muteAudio)
            {
                setVolumeAudioAll(standardAudioVolume);
            }
        }
        if (muteSFX && !last_muteSFX)
        {
            setVolumeSFXAll(0);
        }
        else
        {
            if (!muteSFX && last_muteSFX)
            {
                setVolumeSFXAll(standardSFXVolume);
            }
        }

        last_muteAudio = muteAudio;
        last_muteSFX = muteSFX;

        if (doPlay)
        {
            if (battle)
            {
                Sound s = Array.Find(sounds, sound => sound.name == "Oki_Doki!");
                if (s.source.isPlaying)
                    return;

                stopAllSoundtrack();

                PlayAudio("Oki_Doki!");

                return;
            }

            foreach (AreaAudio aa in areaAudios)
            {
                if (aa.inside(NetworkManager.Singleton.LocalClient.PlayerObject.gameObject))
                {
                    Sound s = Array.Find(sounds, sound => sound.name == aa.name);

                    if (s.source.isPlaying)
                        return;

                    Debug.Log(string.Format("Playing " + s.name + "   Player Position: {0},{1}", NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.transform.position.x, NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.transform.position.y));


                    stopAllSoundtrack();

                    PlayAudio(aa.name);
                    return;
                }
            }

            foreach (SceneAudio sa in sceneAudios)
            {
                if (sa.shouldPlay())
                {
                    Sound s = Array.Find(sounds, sound => sound.name == sa.name);

                    if (s.source.isPlaying)
                        return;

                    Debug.Log(string.Format("Playing " + s.name + " in Scene " + sa.sceneName)); //+ " Player Position: {0},{1}", NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.transform.position.x, NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.transform.position.y));

                    stopAllSoundtrack();

                    PlayAudio(sa.name);
                    return;
                }
            }

            if (!isOnePlaying())
            {
                PlayAudio(standardSoundTrack);
            }
        }
        else
        {
            stopAllSoundtrack();
        }
    }

    public bool isOnePlaying()
    {
        foreach (Sound ss in sounds)
        {
            if(ss.source.isPlaying)
            {
                return true;
            }
        }

        return false;
    }

    public void stopAudio(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Audio File not found");
            return;
        }
        Debug.Log("Stop " + s.source.name);
        s.source.Stop();
    }

    public void stopSFX(string name)
    {
        Sound s = Array.Find(sfxs, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("SFX File not found");
            return;
        }
        Debug.Log("Stop SFX" + s.source.name);
        s.source.Stop();
    }

    public void stopAllSoundtrack(bool debug = true)
    {
        if(debug) Debug.Log("Stopping All Soundtrack");
        foreach (Sound ss in sounds)
            ss.source.Stop();
    }

    public void stopAllSFX(bool debug = true)
    {
        if(debug) Debug.Log("Stopping All SFX");
        foreach (Sound ss in sfxs)
            ss.source.Stop();
    }

    public void stopAll()
    {
        Debug.Log("Stopping All Sound");
        stopAllSoundtrack(false);
        stopAllSFX(false);
    }

    public void PlayAudio(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Audio File not found");
            return;
        }
        Debug.Log("Play " + s.name);
        s.source.Play();
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxs, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("SFX File not found");
            return;
        }
        Debug.Log("Play SFX " + s.name);
        s.source.Play();
    }

    public void setVolumeAudio(string name, float volume)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("Audio File not found");
            return;
        }
        Debug.Log("Set Volume of Audio " + s.name + " to " + volume);
        s.source.volume = volume;
    }

    public void setVolumeSFX(string name, float volume)
    {
        Sound s = Array.Find(sfxs, sound => sound.name == name);
        if (s == null)
        {
            Debug.Log("SFX File not found");
            return;
        }
        Debug.Log("Set Volume of SFX " + s.name + " to " + volume);
        s.source.volume = volume;
    }

    public void setVolumeAudioAll(float volume)
    {
        Debug.Log("Set Volume of all Audio to " + volume);
        foreach (Sound ss in sounds)
            ss.source.volume = volume;
    }

    public void setVolumeSFXAll(float volume)
    {
        Debug.Log("Set Volume of all SFX to " + volume);
        foreach (Sound ss in sfxs)
            ss.source.volume = volume;
    }
}
