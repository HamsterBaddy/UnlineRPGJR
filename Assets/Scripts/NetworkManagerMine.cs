using Unity.Netcode;
using UnityEngine;

public class NetworkManagerMine : NetworkManager
{
	public void startHost()
	{
		base.StartHost();
		LoadTestWorld();
	}

	public void startClient()
	{
		base.StartClient();
		LoadTestWorld();
	}

	public void startServer()
	{
		base.StartServer();
		LoadTestWorld();
	}

	private void LoadTestWorld()
	{
		NetworkManager.Singleton.SceneManager.LoadScene("TopDownWorld", UnityEngine.SceneManagement.LoadSceneMode.Single);
	}

	public void doShutDown()
	{
		GuiManager.Singelton.EnableIpInput(true);
		this.Shutdown();
	}

	public void closeProgram()
	{
		doShutDown();
		Application.Quit();
	}
}
