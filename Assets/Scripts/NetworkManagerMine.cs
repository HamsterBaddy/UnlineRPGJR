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
		Singleton.SceneManager.LoadScene("SideWorld", UnityEngine.SceneManagement.LoadSceneMode.Single);
	}

	public void doShutDown()
	{
		Shutdown();
	}

	public void closeProgram()
	{
		doShutDown();
		Application.Quit();
	}
}
