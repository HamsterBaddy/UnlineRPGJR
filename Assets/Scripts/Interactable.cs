using UnityEngine;
using Unity.Netcode;

public abstract class Interactable : NetworkBehaviour
{
    protected Controls localPlayerControls;
	protected GameObject localPlayer;

	private void Awake()
	{
		localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
		localPlayerControls = localPlayer.GetComponent<PlayerMovement>().controls;
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if(collision.gameObject.tag == "Player" && collision.gameObject == localPlayer)
		{
			if (PlayerMovement.useKeyboard && localPlayerControls.PlayerKeyboardOnly.ReadInteract.IsPressed())
			{
				OnInteraction();
			}
			else if (localPlayerControls.Player.ReadInteract.IsPressed())
			{
				OnInteraction();
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player" && collision.gameObject == localPlayer)
		{
			OnStop();
		}
	}

	protected abstract void OnInteraction();
	protected abstract void OnStop();
}
