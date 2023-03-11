using System;
using System.Net;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class GuiManager : MonoBehaviour
{

	[field: SerializeField]
	public TMP_InputField IpAdress { get; set; }
	[field: SerializeField]
	public TMP_InputField Port { get; set; }

	[field: SerializeField]
	public Button[] StartButtons { get; set; }

	[field: SerializeField]
	public Button SetButton { get; set; }

	private static GuiManager _singelton;
	public static GuiManager Singelton { get => _singelton; set { if (_singelton != null) _singelton = value; } }

	GuiManager()
	{
		if (Singelton != null)
			throw new InvalidOperationException("Es kann nur eine GuiManagerKlasse existieren");
	}

	public void EnableIpInput(bool yes)
	{
		foreach (Button b in StartButtons)
		{
			b.interactable = !(yes);
		}

		SetButton.interactable =
		IpAdress.interactable =
		Port.interactable = yes;
	}

	public void SetIP()
	{
		Debug.Log($"Eingegebene Ip-Adresse{IpAdress.text}");
		Debug.Log($"Eingegebene Ip-Adresse{Port.text}");

		if (IPAddress.TryParse(IpAdress.text, out _) && ushort.TryParse(Port.text, out ushort Parsed_Port))
		{
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
				IpAdress.text,  // The IP address is a string
				Parsed_Port // The port number is an unsigned short
			);

			EnableIpInput(false);
		}
	}
}