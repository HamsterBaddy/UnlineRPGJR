using Unity.Netcode;

using UnityEngine;

public class NetworkManagerMine : NetworkManager
{
	public void startHost()
	{
		StartHost();
		LoadTestWorld();
	}

	public void startClient()
	{
		StartClient();
		//LoadTestWorld();
	}

	public void startServer()
	{
		StartServer();
		LoadTestWorld();
	}

	private void LoadTestWorld()
	{
		Singleton.SceneManager.LoadScene("SideWorld", UnityEngine.SceneManagement.LoadSceneMode.Single);
		AudioManager.Instance.doPlay = true;
	}

	public void doShutDown()
	{
		GuiManager.Singelton.EnableIpInput(true);
		Shutdown();
	}

	public void closeProgram()
	{
		doShutDown();
		Application.Quit();
	}

	public GameObject getPlayer()
    {
		return NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;
	}
}
