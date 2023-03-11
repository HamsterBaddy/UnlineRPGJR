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
		LoadTestWorld();
	}

	public void startServer()
	{
		StartServer();
		LoadTestWorld();
	}

	private void LoadTestWorld()
	{
		Singleton.SceneManager.LoadScene("TopDownWorld", UnityEngine.SceneManagement.LoadSceneMode.Single);
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
}
