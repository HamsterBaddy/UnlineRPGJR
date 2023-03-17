using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerActiveManager : NetworkBehaviour
{
	public List<string> noPlayerScenes = new() { "Lobby" };

	public GameObject childsprite;

	private void Awake()
	{
		NetworkManager.Singleton.SceneManager.OnLoadComplete += onSceneLoaded;

		setEnableComponents(!noPlayerScenes.Contains(SceneManager.GetActiveScene().name));
	}

	public void onSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
	{
			Debug.Log($"{sceneName} | {clientId} | {OwnerClientId} | {noPlayerScenes.Contains(sceneName)} | {noPlayerScenes[0]}");
			
			setEnableComponents(!noPlayerScenes.Contains(sceneName));
	}

	public void setEnableComponents(bool enable)
	{
		GetComponent<PlayerMovement>().enabled =
		GetComponent<BoxCollider2D>().enabled = enable;

		GetComponent<Rigidbody2D>().constraints = enable ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.FreezeAll;

		childsprite.SetActive(enable);
	}
}
