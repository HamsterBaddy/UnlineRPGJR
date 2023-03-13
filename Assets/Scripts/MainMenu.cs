using System;
using System.Net;

using TMPro;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

	private void Start()
	{
		NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
	}


	/// <summary>
	/// Listener für Host-Button
	/// </summary>
	public void HostGame()
	{
		NetworkManager.Singleton.StartHost();
	}

	private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
	{
		if(NetworkManager.Singleton.IsHost)
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

		_JoinAnswerText.text = RequestString;
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

		if (IPAddress.TryParse(_IPAdressField.text, out _) && ushort.TryParse(_PortField.text, out ushort Parsed_Port))
		{
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(_IPAdressField.text, Parsed_Port);
		}

		IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());

		string connectionData = $"Target-Ip{_IPAdressField.text}; Target-Port: {_PortField.text}, Origin-IPs: {localIPs}, Origin-Computer: {Environment.MachineName}";
		Debug.Log("Connection-Data: " + connectionData);

		NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(connectionData);
		NetworkManager.Singleton.StartClient();
		NetworkLog.LogInfoServer($"Trying To Connect To Server; Connectiondata: {connectionData}");

		_JoinAnswerText.gameObject.SetActive(true);
		_JoinAnswerText.text = connectionData + Environment.NewLine;
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

	///// <summary>
	///// Update-Methode <para/>
	///// Erlaubt das benutzten der Tab-Taste zur Auswahl der Buttons
	///// </summary>
	//public void Update()
	//{

	//	if (Input.GetKeyDown(KeyCode.Tab))
	//	{
	//		Selectable next = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();

	//		if (next != null)
	//		{

	//			if (next.TryGetComponent(out InputField inputfield))
	//				inputfield.OnPointerClick(new PointerEventData(EventSystem.current));  //if it's an input field, also set the text caret

	//			EventSystem.current.SetSelectedGameObject(next.gameObject, new BaseEventData(EventSystem.current));
	//		}
	//		//else Debug.Log("next nagivation element not found");

	//	}
	//}
}