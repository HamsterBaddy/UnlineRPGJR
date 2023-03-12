using System.Net;

using TMPro;

using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

using UnityEngine;


public class MainMenu : MonoBehaviour
{
	[SerializeField] private TMP_InputField  _IPAdressField;
	[SerializeField] private TMP_InputField  _PortField;

	public void ExitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

	public void JoinGame()
	{
		Debug.Log($"Eingegebene Ip-Adresse{_IPAdressField.text}");
		Debug.Log($"Eingegebene Ip-Adresse{_PortField.text}");

		if (IPAddress.TryParse(_IPAdressField.text, out _) && ushort.TryParse(_PortField.text, out ushort Parsed_Port))
		{
			NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(_IPAdressField.text, Parsed_Port);
			NetworkManager.Singleton.StartClient();
		}
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