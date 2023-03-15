using UnityEngine.Audio;
using System;
using UnityEngine;
using Unity.Netcode;

public class AudioManager : NetworkBehaviour
{
	private static AudioManager _instance;
	public static AudioManager Instance { get => _instance; set { if (_instance != null) _instance = value; } }

	public Sound[] Music;

	public Sound[] Sfxs;

	public SceneAudio[] sceneAudios;

	public AreaAudio[] areaAudios;

	public bool battle = false;

	public bool doPlay = false;

	public bool muteAudio = false;
	public bool muteSFX = false;

	private bool last_muteAudio = false;
	private bool last_muteSFX = false;

	public float standardAudioVolume = 1f;
	public float standardSFXVolume = 0.3f;
	public float standardMasterVolume = 1f;

	public float audioVolume = 1f;
	public float SFXVolume = 0.3f;
	public float masterVolume = 1f;

	public string standardSoundTrack = "Vulcano";

	AudioManager()
	{
		Instance = this;
	}

	// Start is called before the first frame update
	void Awake()
	{

		DontDestroyOnLoad(gameObject);

		foreach (Sound s in Music)
		{
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;

			s.source.volume = s.volume;
			s.source.pitch = s.pitch;
			s.source.loop = s.loop;

			s.source.spatialBlend = s.spatialBlend;
		}

		foreach (Sound s in Sfxs)
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
		masterVolume = ClientPrefs.GetMasterVolume();
		audioVolume = ClientPrefs.GetMusicVolume();
		SFXVolume = ClientPrefs.GetSFXVolume();
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
				setVolumeAudioAll(audioVolume);
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
				setVolumeSFXAll(SFXVolume);
			}
		}

		last_muteAudio = muteAudio;
		last_muteSFX = muteSFX;

		if (doPlay)
		{
			if (battle)
			{
				Sound s = Array.Find(Music, sound => sound.name == "Oki_Doki!");
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
					Sound s = Array.Find(Music, sound => sound.name == aa.name);

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
				try
				{
					if (sa.shouldPlay())
					{
						Sound s = Array.Find(Music, sound => sound.name == sa.name);

						if (s.source.isPlaying)
							return;

						Debug.Log(string.Format("Playing " + s.name + " in Scene " + sa.sceneName)); //+ " Player Position: {0},{1}", NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.transform.position.x, NetworkManager.Singleton.LocalClient.PlayerObject.gameObject.transform.position.y));

						stopAllSoundtrack();

						PlayAudio(sa.name);
						return;
					}
				}
				catch (System.NullReferenceException e)
				{
					
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
		foreach (Sound ss in Music)
		{
			if (ss.source.isPlaying)
			{
				return true;
			}
		}

		return false;
	}

	public void stopAudio(string name)
	{
		Sound s = Array.Find(Music, sound => sound.name == name);
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
		Sound s = Array.Find(Sfxs, sound => sound.name == name);
		if (s == null)
		{
			Debug.Log("SFX File not found");
			return;
		}
		Debug.Log("Stop SFX" + s.source.name);
		s.source.Stop();
	}

	public void stopAllSounds(Sound[] soundArray)
	{
		foreach (Sound sound in audio)
		{
			sound.source.Stop();
		}
	}

	public void stopAllSoundtrack(bool debug = true)
	{
		if (debug) Debug.Log("Stopping All Soundtrack");
		foreach (Sound ss in Music)
			ss.source.Stop();
	}

	public void stopAllSFX(bool debug = true)
	{
		if (debug) Debug.Log("Stopping All SFX");
		foreach (Sound ss in Sfxs)
			ss.source.Stop();
	}

	public void stopAll()
	{
		Debug.Log("Stopping All Sound");
		stopAllSounds(Music);
		stopAllSounds(Sfxs);

	}

	public void PlayAudio(string name)
	{
		Sound s = Array.Find(Music, sound => sound.name == name);
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
		Sound s = Array.Find(Sfxs, sound => sound.name == name);
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
		Sound s = Array.Find(Music, sound => sound.name == name);
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
		Sound s = Array.Find(Sfxs, sound => sound.name == name);
		if (s == null)
		{
			Debug.Log("SFX File not found");
			return;
		}
		Debug.Log("Set Volume of SFX " + s.name + " to " + volume);
		s.source.volume = volume;
	}

	public void setVolumeAudioAll(float volume, bool debug = true)
	{
		audioVolume = volume;
		ClientPrefs.SetMusicVolume(volume);
		if (debug == true && debug != false)
		{
			Debug.Log("Set Volume of all Audio to " + volume);
		}
		foreach (Sound ss in Music)
			ss.source.volume = volume * masterVolume;
	}

	public void setVolumeSFXAll(float volume, bool debug = true)
	{
		SFXVolume = volume;
		ClientPrefs.SetSFXVolume(volume);
		if (debug == true && debug != false)
		{
			Debug.Log("Set Volume of all SFX to " + volume);
		}
		foreach (Sound ss in Sfxs)
			ss.source.volume = volume * masterVolume;
	}

	public void setMasterVolume(float volume)
	{
		ClientPrefs.SetMasterVolume(volume);
		Debug.Log("Set Master Volume to " + volume);
		masterVolume = volume;
		setVolumeAudioAll(audioVolume, false);
		setVolumeSFXAll(SFXVolume, false);
	}
}
