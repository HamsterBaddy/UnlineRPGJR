using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using TMPro;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Enthält die Logik für das Hauptmenü und den Aufbau der Netzwerkverbindung
/// </summary>
public class MainMenu : MonoBehaviour
{

	[SerializeField] private TMP_InputField _UserNameField;

	[SerializeField] private TMP_InputField _JoinCodeField;

	[SerializeField] private Button         _JoinGameMainMenuGameButton;
	[SerializeField] private Button         _HostGameButton;
	[SerializeField] private Button         _ExitAppButton;

	[SerializeField] private Button         _StartGameButton;
	[SerializeField] private TMP_Text       _JoinCodeTextText;
	[SerializeField] private TMP_Text       _BeigetreneSpielerText;
	[SerializeField] private Button         _BackToMainMenuFromHostingButton;

	[SerializeField] private TMP_Text       _JoinAnswerText;
	[SerializeField] private Button         _JoinGameButton;

	[SerializeField] private Slider         _MasterVolumeSlider;
	[SerializeField] private Slider         _MusicVolumeSlider;
	[SerializeField] private Slider         _SFXVolumeSlider;

	[SerializeField] private TMP_Dropdown   _RegionsDropdown;

	//private NetworkManager.ConnectionApprovalResponse JoinResponse = null;

	private const int MaxConnections = 2;

	private RelayServerData relayServerData;

	/// <summary>
	/// Initialises Click-Listeners
	/// </summary>
	private void Awake()
	{
		_UserNameField.onValueChanged.AddListener(UserNameChanged);

		_HostGameButton.onClick.AddListener(HostGame);
		_JoinGameButton.onClick.AddListener(JoinGame);
		_StartGameButton.onClick.AddListener(StartGame);
		_ExitAppButton.onClick.AddListener(ExitGame);

		_BackToMainMenuFromHostingButton.onClick.AddListener(BackToMainMenuFromHosting);

		_MasterVolumeSlider.onValueChanged.AddListener(MasterVolumeChanged);
		_MusicVolumeSlider.onValueChanged.AddListener(MusicVolumeChanged);
		_SFXVolumeSlider.onValueChanged.AddListener(SFXVolumeChanged);

		_MasterVolumeSlider.value = ClientPrefs.GetMasterVolume();
		_MusicVolumeSlider.value = ClientPrefs.GetMusicVolume();
		_SFXVolumeSlider.value = ClientPrefs.GetSFXVolume();
	}

	private async void Start()
	{
		//NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

		try
		{
			await UnityServices.InitializeAsync();

			// ParrelSync should only be used within the Unity Editor so you should use the UNITY_EDITOR define
#if UNITY_EDITOR
			if (ParrelSync.ClonesManager.IsClone())
			{
				// When using a ParrelSync clone, switch to a different authentication profile to force the clone
				// to sign in as a different anonymous user account.
				string customArgument = ParrelSync.ClonesManager.GetArgument();
				AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
			}
#endif

			await AuthenticationService.Instance.SignInAnonymouslyAsync();

			List<Region> allRegions = await RelayService.Instance.ListRegionsAsync();

			List<string> RegionNames = new();

			foreach (Region region in allRegions)
			{
				Debug.Log($"Region: {region.Id}: {region.Description}");
				RegionNames.Add(region.Id);
			}
			_RegionsDropdown.options.Clear();
			_RegionsDropdown.AddOptions(RegionNames);

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
		StartCoroutine(ConfigureTransportAndStartNgoAsHost());
		_RegionsDropdown.interactable = false;
	}

	IEnumerator ConfigureTransportAndStartNgoAsHost()
	{
		Task<(RelayServerData, string)> serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(MaxConnections/*, _RegionsDropdown.options[_RegionsDropdown.value].text*/);
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
		_JoinCodeTextText.text = relayServerDataAndJoinCode.Item2;

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
		catch (Exception ex)
		{
			Debug.LogError("Relay create join code request failed: " + ex);
			throw;
		}

		return (new RelayServerData(allocation, "dtls"), createJoinCode);
	}

	//private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	//{
	//	if (NetworkManager.Singleton.IsHost)
	//	{
	//		response.Approved = true;
	//	}
	//	else
	//	{
	//		response.Pending = true;
	//	}

	//	NetworkLog.LogInfoServer("Received ConnectionRequest");
	//	// The client identifier to be authenticated
	//	ulong clientId = request.ClientNetworkId;

	//	// Additional connection data defined by user code
	//	byte[] connectionData = request.Payload;

	//	response.CreatePlayerObject = true;

	//	string RequestString = $"Client-Id:{clientId}; {System.Text.Encoding.ASCII.GetString(connectionData)}";
	//	Debug.Log("Requst-String: " + RequestString);

	//	_JoinAnswerText.text += RequestString;
	//	JoinResponse = response;
	//	_AcceptJoinButton.interactable = true;
	//}

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
		if (NetworkManager.Singleton.IsServer)
		{
			//_BeigetreneSpielerText.text += ;
		}
		else
		{
			_JoinAnswerText.text += "Server Approved Connection";
			NetworkLog.LogInfoServer($"Connected On Client {NetworkManager.Singleton.LocalClientId}");
		}
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
		Debug.Log($"Eingegebene Ip-Adresse: {_JoinCodeField.text}");
		//Debug.Log($"Eingegebener Port: {_PortField.text}");

		//if (IPAddress.TryParse(_IPAdressField.text, out _) && ushort.TryParse(_PortField.text, out ushort Parsed_Port))
		//{
		//	NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(_IPAdressField.text, Parsed_Port);
		//}

		//IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

		//string connectionData = $"Target-Ip{_IPAdressField.text}; Target-Port: {_PortField.text}, Origin-IPs: {string.Join(",", localIPs.Select(x => x.ToString()).ToArray())}, Origin-Computer: {Environment.MachineName}";
		//Debug.Log("Connection-Data: " + connectionData);

		//NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(connectionData);


		_RegionsDropdown.interactable = false;

		StartCoroutine(ConfigureTransportAndStartNgoAsConnectingPlayer(_JoinCodeField.text));

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

	IEnumerator ConfigureTransportAndStartNgoAsConnectingPlayer(string RelayJoinCode)
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
		SendClientInformation(_UserNameField.text);
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

	[ServerRpc]
	private void SendClientInformation(string UserName)
	{
		_BeigetreneSpielerText.text += UserName;
	}

	private void UserNameChanged(string value)
	{
		_JoinGameMainMenuGameButton.interactable =
		_HostGameButton.interactable = !string.IsNullOrWhiteSpace(value);
	}

	private void BackToMainMenuFromHosting()
	{
		//foreach (KeyValuePair<ulong, NetworkClient> a in NetworkManager.Singleton.ConnectedClients)
		//	NetworkManager.Singleton.DisconnectClient(a.Value.ClientId, "Der Host hat den Server beendet");
		NetworkManager.Singleton.Shutdown();
		//if (NetworkManager.Singleton != null)
		//{
		//	Destroy(NetworkManager.Singleton.gameObject);
		//}
		//gameObject.AddComponent<NetworkManager>();
	}

	private void MasterVolumeChanged(float Volume)
	{
		ClientPrefs.SetMasterVolume(Volume);
	}

	private void MusicVolumeChanged(float Volume)
	{
		ClientPrefs.SetMusicVolume(Volume);
	}
	private void SFXVolumeChanged(float Volume)
	{
		ClientPrefs.SetSFXVolume(Volume);
	}
}