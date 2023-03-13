using UnityEngine.Audio;
using System;
using UnityEngine;
using Unity.Netcode;

public class AudioManager : NetworkBehaviour
{
    public static AudioManager Instance { set; get; }

    public Sound[] sounds;

    public Sound[] sfxs;

    public AreaAudio[] areaAudios;

    public static AudioManager instance;

    public bool battle = false;

    public bool doPlay = false;

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
        Play(standardSoundTrack);
    }

    private void Update()
    {
        if (doPlay)
        {
            if (battle)
            {
                Sound s = Array.Find(sounds, sound => sound.name == "Oki_Doki!");
                if (s.source.isPlaying)
                    return;

                stopAll();

                Play("Oki_Doki!");

                return;
            }

            if(!isOnePlaying())
            {
                Play(standardSoundTrack);
            }

            foreach (AreaAudio aa in areaAudios)
            {
                if (aa.inside(NetworkManager.Singleton.LocalClient.PlayerObject.gameObject))
                {
                    Sound s = Array.Find(sounds, sound => sound.name == aa.name);

                    if (s.source.isPlaying)
                        return;

                    Debug.Log(string.Format("Playing " + s.name + "   Player Position: {0},{1}", NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.transform.position.x, NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.transform.position.y));


                    stopAll();

                    Play(aa.name);
                }
            }
        }
        else
        {
            stopAll();
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

    public void stopAll()
    {
        foreach (Sound ss in sounds)
            ss.source.Stop();
    }

    public void Play (string name)
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
}
