using System;

using UnityEngine;
using UnityEngine.UI;

public class SideGuiManager : MonoBehaviour
{
	private static SideGuiManager _singelton;
	public static SideGuiManager Singelton { get => _singelton; set { if (_singelton == null) _singelton = value; } }

	SideGuiManager()
	{
		if (Singelton != null)
			throw new InvalidOperationException("Es kann nur eine SideGuiManager existieren");
	}

	// Start is called before the first frame update
	void Start()
	{
		Singelton = this;
	}
}
