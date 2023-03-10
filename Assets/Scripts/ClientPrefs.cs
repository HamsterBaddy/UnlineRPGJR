using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Singleton class which saves/loads local-client settings.
/// (This is just a wrapper around the PlayerPrefs system,
/// so that all the calls are in the same place.)
/// </summary>
public static class ClientPrefs
{
	const string k_MasterVolumeKey = "MasterVolume";
	const string k_MusicVolumeKey = "MusicVolume";
	const string k_SFXVolumeKey = "SFXVolume";
	const string k_ClientGUIDKey = "client_guid";

	public static float GetMasterVolume()
	{
		return PlayerPrefs.GetFloat(k_MasterVolumeKey, AudioManager.Instance.standardMasterVolume);
	}

	public static void SetMasterVolume(float volume)
	{
		//AudioManager.Instance.setMasterVolume(volume);
		PlayerPrefs.SetFloat(k_MasterVolumeKey, volume);
	}

	public static float GetMusicVolume()
	{
		return PlayerPrefs.GetFloat(k_MusicVolumeKey, AudioManager.Instance.standardAudioVolume);
	}

	public static void SetMusicVolume(float volume)
	{
		//AudioManager.Instance.setVolumeAudioAll(volume);
		PlayerPrefs.SetFloat(k_MusicVolumeKey, volume);
	}

	public static float GetSFXVolume()
	{
		return PlayerPrefs.GetFloat(k_SFXVolumeKey, AudioManager.Instance.standardSFXVolume);
	}

	public static void SetSFXVolume(float volume)
	{
		//AudioManager.Instance.setVolumeSFXAll(volume);
		PlayerPrefs.SetFloat(k_SFXVolumeKey, volume);
	}

	/// <summary>
	/// Either loads a Guid string from Unity preferences, or creates one and checkpoints it, then returns it.
	/// </summary>
	/// <returns>The Guid that uniquely identifies this client install, in string form. </returns>
	public static string GetGuid()
	{
		if (PlayerPrefs.HasKey(k_ClientGUIDKey))
		{
			return PlayerPrefs.GetString(k_ClientGUIDKey);
		}

		Guid guid = Guid.NewGuid();
		string guidString = guid.ToString();

		PlayerPrefs.SetString(k_ClientGUIDKey, guidString);
		return guidString;
	}
}