using System;
using System.Net;

using TMPro;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using NetworkEvent = Unity.Networking.Transport.NetworkEvent;
using UnityEngine.Rendering.LookDev;

/// <summary>
/// Enthält die Logik für das Hauptmenü und den Aufbau der Netzwerkverbindung
/// </summary>
public class MainMenu : MonoBehaviour
{
	[SerializeField] private TMP_InputField _IPAdressField;
	[SerializeField] private TMP_InputField _PortField;
	[SerializeField] private Button         _JoinGameButton;
	[SerializeField] private Button         _HostGameButton;
	[SerializeField] private Button         _ExitAppButton;

	[SerializeField] private Button         _StartGameButton;
	[SerializeField] private Button         _AcceptJoinButton;
	[SerializeField] private TMP_Text       _JoinRequestText;

	[SerializeField] private TMP_Text       _JoinAnswerText;

	private NetworkManager.ConnectionApprovalResponse JoinResponse = null;

	private const int MaxConnections = 2;

	private RelayServerData relayServerData;
	private string PlayerId;

	/// <summary>
	/// Initialises Click-Listeners
	/// </summary>
	private void Awake()
	{
		_HostGameButton.onClick.AddListener(HostGame);
		_JoinGameButton.onClick.AddListener(JoinGame);
		_StartGameButton.onClick.AddListener(StartGame);
		_ExitAppButton.onClick.AddListener(ExitGame);
		_AcceptJoinButton.onClick.AddListener(AcceptJoinGame);
	}

	private async void Start()
	{
		NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

		try
		{
			await UnityServices.InitializeAsync();
			await AuthenticationService.Instance.SignInAnonymouslyAsync();
			PlayerId = AuthenticationService.Instance.PlayerId;
			_JoinAnswerText.text = PlayerId;
		}
		catch (Exception ex)
		{
			Debug.LogError(ex.ToString());
			throw;
		}
	}


	/// <summary>
	/// Listener für Host-Button
	/// </summary>
	public void HostGame()
	{
		StartCoroutine(Example_ConfigureTransportAndStartNgoAsHost());

	}

	IEnumerator Example_ConfigureTransportAndStartNgoAsHost()
	{
		Task<(RelayServerData, string)> serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(MaxConnections);
		while (!serverRelayUtilityTask.IsCompleted)
		{
			yield return null;
		}
		if (serverRelayUtilityTask.IsFaulted)
		{
			Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
			yield break;
		}

		(RelayServerData, string) relayServerDataAndJoinCode = serverRelayUtilityTask.Result;



		// Display the joinCode to the user.
		_JoinRequestText.text = relayServerDataAndJoinCode.Item2;

		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerDataAndJoinCode.Item1);
		NetworkManager.Singleton.StartHost();
		yield return null;
	}


	public static async Task<(RelayServerData, string)> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
	{
		Allocation allocation;
		string createJoinCode;
		try
		{
			allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
		}
		catch (Exception e)
		{
			Debug.LogError($"Relay create allocation request failed {e.Message}");
			throw;
		}

		Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
		Debug.Log($"server: {allocation.AllocationId}");

		try
		{
			createJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
			Debug.Log(createJoinCode);
		}
		catch
		{
			Debug.LogError("Relay create join code request failed");
			throw;
		}

		return (new RelayServerData(allocation, "dtls"), createJoinCode);
	}

	private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	{
		if (NetworkManager.Singleton.IsHost)
		{
			response.Approved = true;
		}
		else
		{
			response.Pending = true;
		}

		NetworkLog.LogInfoServer("Received ConnectionRequest");
		// The client identifier to be authenticated
		ulong clientId = request.ClientNetworkId;

		// Additional connection data defined by user code
		byte[] connectionData = request.Payload;

		response.CreatePlayerObject = true;

		string RequestString = $"Client-Id:{clientId}; {System.Text.Encoding.ASCII.GetString(connectionData)}";
		Debug.Log("Requst-String: " + RequestString);

		_JoinAnswerText.text += RequestString;
		JoinResponse = response;
		_AcceptJoinButton.interactable = true;
	}

	private void OnClientDisconnectCallback(ulong obj)
	{
		NetworkLog.LogInfoServer("Disconnected On Client");
		if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.DisconnectReason != string.Empty)
		{
			Debug.Log($"Approval Declined Reason: {NetworkManager.Singleton.DisconnectReason}");
		}
	}

	private void OnClientConnectedCallback(ulong obj)
	{
		_JoinAnswerText.text += "Server Approved Connection";
		NetworkLog.LogInfoServer($"Connected On Client {NetworkManager.Singleton.LocalClientId}");
	}

	/// <summary>
	/// Listener für Accept-Join-Button
	/// </summary>
	public void AcceptJoinGame()
	{
		NetworkLog.LogInfoServer("Accepting Join Request");
		JoinResponse.Approved = true;
		_StartGameButton.interactable = true;
		JoinResponse.Pending = false;
	}

	/// <summary>
	/// Listener für Scene-Loader nachdem Spierl gejoint ist
	/// </summary>
	public void StartGame()
	{
		NetworkLog.LogInfoServer("Starting Game");
		NetworkManager.Singleton.SceneManager.LoadScene("SideWorld", LoadSceneMode.Single);
	}

	/// <summary>
	/// Listener für Join-Button
	/// </summary>
	public void JoinGame()
	{
		Debug.Log($"Eingegebene Ip-Adresse: {_IPAdressField.text}");
		Debug.Log($"Eingegebener Port: {_PortField.text}");

		//if (IPAddress.TryParse(_IPAdressField.text, out _) && ushort.TryParse(_PortField.text, out ushort Parsed_Port))
		//{
		//	NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(_IPAdressField.text, Parsed_Port);
		//}

		//IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

		//string connectionData = $"Target-Ip{_IPAdressField.text}; Target-Port: {_PortField.text}, Origin-IPs: {string.Join(",", localIPs.Select(x => x.ToString()).ToArray())}, Origin-Computer: {Environment.MachineName}";
		//Debug.Log("Connection-Data: " + connectionData);

		//NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(connectionData);

		StartCoroutine(Example_ConfigureTransportAndStartNgoAsConnectingPlayer(_IPAdressField.text));

		//NetworkLog.LogInfoServer($"Trying To Connect To Server; Connectiondata: {connectionData}");

		_JoinAnswerText.gameObject.SetActive(true);
		//_JoinAnswerText.text = connectionData + Environment.NewLine;
	}

	public static async Task<RelayServerData> JoinRelayServerFromJoinCode(string joinCode)
	{
		JoinAllocation allocation;
		try
		{
			allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
		}
		catch
		{
			Debug.LogError("Relay create join code request failed");
			throw;
		}

		Debug.Log($"client: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
		Debug.Log($"host: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
		Debug.Log($"client: {allocation.AllocationId}");

		return new RelayServerData(allocation, "dtls");
	}

	IEnumerator Example_ConfigureTransportAndStartNgoAsConnectingPlayer(string RelayJoinCode)
	{
		// Populate RelayJoinCode beforehand through the UI
		Task<RelayServerData> clientRelayUtilityTask = JoinRelayServerFromJoinCode(RelayJoinCode);

		while (!clientRelayUtilityTask.IsCompleted)
		{
			yield return null;
		}

		if (clientRelayUtilityTask.IsFaulted)
		{
			Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
			yield break;
		}

		relayServerData = clientRelayUtilityTask.Result;

		NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

		NetworkManager.Singleton.StartClient();
		yield return null;
	}


	/// <summary>
	/// Listener für Exit-Button
	/// </summary>
	public void ExitGame()
	{
		//Wird abhängig davon ausgeführt, ob es im Unity-Editor oder im Build läuft
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}
}