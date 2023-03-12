using System.Net;

using TMPro;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private TMP_InputField _IPAdressField;
	[SerializeField] private TMP_InputField _PortField;
	[SerializeField] private Button         _JoinGameButton;
	[SerializeField] private Button         _HostGameButton;
	[SerializeField] private Button         _ExitAppButton;

	private void Awake()
	{
		_HostGameButton.onClick.AddListener(HostGame);
		_JoinGameButton.onClick.AddListener(JoinGame);
		_ExitAppButton.onClick.AddListener(ExitGame);
	}

	public void HostGame()
	{
		NetworkManager.Singleton.StartHost();
		NetworkManager.Singleton.SceneManager.LoadScene("SideWorld", LoadSceneMode.Single);
	}

	public void JoinGame()
	{
		Debug.Log($"Eingegebene Ip-Adresse: {_IPAdressField.text}");
		Debug.Log($"Eingegebener Port: {_PortField.text}");

		if (IPAddress.TryParse(_IPAdressField.text, out _) && ushort.TryParse(_PortField.text, out ushort Parsed_Port))
		{
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(_IPAdressField.text, Parsed_Port);
		}
		NetworkManager.Singleton.StartClient();
	}

	public void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

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