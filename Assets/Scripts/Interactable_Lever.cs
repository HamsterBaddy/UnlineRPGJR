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

	public event EventHandler OnLeverActivationRPC;

	public event EventHandler OnLeverDeactivationRPC;

	private void Start()
	{
		OnLeverActivation += SynchronizeActivation;
		OnLeverDeactivation += SynchronizeDeactivation;
	}

	private void SynchronizeDeactivation(object sender, EventArgs e)
	{
		localPlayer.GetComponent<PlayerMovement>().leverUpdate(this);
	}

	private void SynchronizeActivation(object sender, EventArgs e)
	{
		localPlayer.GetComponent<PlayerMovement>().leverUpdate(this);
	}

	protected override void OnInteraction()
	{
		if (Time.frameCount - frameLastActivation >= 32)
		{
			Debug.Log("LevelPulled");
			isPulled = !isPulled;
			PullLever();
		}
	}

	public void PullLever(bool doInvoke = true)
	{
		if (isPulled)
		{
			Debug.Log("isPulled");
			animator.SetTrigger("isPulled");
			if(doInvoke) 
				OnLeverActivation?.Invoke(this, EventArgs.Empty);
			else
				OnLeverActivationRPC?.Invoke(this, EventArgs.Empty);
		}
		else
		{
			Debug.Log("isUnPulled");
			animator.SetTrigger("isUnPulled");
			if(doInvoke) 
				OnLeverDeactivation?.Invoke(this, EventArgs.Empty);
			else
				OnLeverDeactivationRPC?.Invoke(this, EventArgs.Empty);
		}
		frameLastActivation = Time.frameCount;
	}

	protected override void OnStop()
	{
		
	}
}
