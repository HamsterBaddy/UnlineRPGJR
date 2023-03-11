using System.Collections.Generic;

using Unity.Netcode;

using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerActiveManager : NetworkBehaviour
{
	public List<string> noPlayerScenes = new() { "Lobby", "Battle" };

	public GameObject childsprite;

	private void Awake()
	{
		NetworkManager.Singleton.SceneManager.OnLoadComplete += onSceneLoaded;

		setEnableComponents(!noPlayerScenes.Contains(SceneManager.GetActiveScene().name));
	}

	public void onSceneLoaded(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
	{
		if (IsOwner)
		{
			Debug.Log($"{sceneName} | {clientId} | {OwnerClientId} | {noPlayerScenes.Contains(sceneName)} | {noPlayerScenes[0]}");

			if (OwnerClientId == clientId)
			{
				setEnableComponents(!noPlayerScenes.Contains(sceneName));

				if (sceneName == "TopDownWorld")
				{
					GetComponent<PlayerMovement>().side.Value = false;
				}
				else if (sceneName == "SideWorld")
				{
					GetComponent<PlayerMovement>().side.Value = true;
				}
			}
		}
	}

	public void setEnableComponents(bool enable)
	{
		GetComponent<PlayerMovement>().enabled =
		GetComponent<BoxCollider2D>().enabled = enable;

		GetComponent<Rigidbody2D>().constraints = enable ? RigidbodyConstraints2D.FreezeRotation : RigidbodyConstraints2D.FreezeAll;

		childsprite.SetActive(enable);
	}
}
