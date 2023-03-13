using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class AudioManagerNah : NetworkBehaviour
{
	private static AudioManagerNah _instance;
	public static AudioManagerNah Instance { get => _instance; set { if (_instance == null) _instance = value; } }

	// Audio players components.
	public AudioSource EffectsSource;
	public AudioSource MusicSource;

	// Random pitch adjustment range.
	public float LowPitchRange = .95f;
	public float HighPitchRange = 1.05f;

	public string standardST;

	/**
	 * https://forum.unity.com/threads/cant-see-dictionaries-in-inspector.938741/
	 * */
	[Serializable]
	public class KeyValuePair
	{
		public string key;
		public AudioClip val;
	}

	public List<KeyValuePair> soundTracks = new List<KeyValuePair>();
	Dictionary<string, AudioClip> soundtracksDic = new Dictionary<string, AudioClip>();

	public List<KeyValuePair> SFXs = new List<KeyValuePair>();
	Dictionary<string, AudioClip> SFXDic = new Dictionary<string, AudioClip>();

	AudioManagerNah()
	{
		if (Instance != null)
			throw new InvalidOperationException("Es kann nur eine AudioManagerNah existieren");
	}

	private void Awake()
	{
		// If there is not already an instance of SoundManager, set it to this.
		if (Instance == null)
		{
			Instance = this;
		}
		//If an instance already exists, destroy whatever this object is to enforce the singleton.
		else if (Instance != this)
		{
			Destroy(gameObject);
		}

		foreach (var kvp in soundTracks)
		{
			soundtracksDic[kvp.key] = kvp.val;
		}

		foreach (var kvp in SFXs)
		{
			SFXDic[kvp.key] = kvp.val;
		}

		//Set SoundManager to DontDestroyOnLoad so that it won't be destroyed when reloading our scene.
		DontDestroyOnLoad(gameObject);
	}

    private void Start()
    {
        
    }

    // Play a single clip through the sound effects source.
    public void Play(AudioClip clip)
	{
		EffectsSource.clip = clip;
		EffectsSource.Play();
	}

	// Play a single clip through the music source.
	public void PlayMusic(AudioClip clip)
	{
		MusicSource.clip = clip;
		MusicSource.Play();
	}

	// Play a random clip from an array, and randomize the pitch slightly.
	public void RandomSoundEffect(params AudioClip[] clips)
	{
		int randomIndex = UnityEngine.Random.Range(0, clips.Length);
		float randomPitch = UnityEngine.Random.Range(LowPitchRange, HighPitchRange);

		EffectsSource.pitch = randomPitch;
		EffectsSource.clip = clips[randomIndex];
		EffectsSource.Play();
	}
}
