using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;

public class Interactable_Lever : Interactable
{
	public bool isPulled = false;
	public Animator animator;

	[SerializeField]
	private int frameLastActivation = -32;

	public event EventHandler OnLeverActivation;

	public event EventHandler OnLeverDeactivation;

	protected override void OnInteraction()
	{
		if (Time.frameCount - frameLastActivation >= 32)
		{
			Debug.Log("LevelPulled");
			isPulled = !isPulled;
			if (isPulled)
			{
				Debug.Log("isPulled");
				animator.SetTrigger("isPulled");
				OnLeverActivation?.Invoke(this,EventArgs.Empty);
			}
			else
			{
				Debug.Log("isUnPulled");
				animator.SetTrigger("isUnPulled");
				OnLeverDeactivation?.Invoke(this, EventArgs.Empty);
			}
			frameLastActivation = Time.frameCount;
		}
	}

	protected override void OnStop()
	{
		
	}

	private void FixedUpdate()
	{
		Request_Syncronize_ServerRpc(isPulled);
	}

	[ServerRpc]
	public void Request_Syncronize_ServerRpc(bool isPulledVar)
	{
		onSynchronize_ClientRpc(isPulledVar);
	}

	[ClientRpc]
	public void onSynchronize_ClientRpc(bool isPulledVar)
	{
		isPulled = isPulledVar;
	}
}
