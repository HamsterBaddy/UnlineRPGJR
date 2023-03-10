using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class NetworkManagerMine : NetworkManager
{
    public void startHost()
    {
        this.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene("TestWorld", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void startClient()
    {
        this.StartClient();
        NetworkManager.Singleton.SceneManager.LoadScene("TestWorld", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void startServer()
    {
        this.StartServer();
        NetworkManager.Singleton.SceneManager.LoadScene("TestWorld", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void doShutDown()
    {
        GuiManager.Singelton.enableIpInput(true);
        this.Shutdown();
    }

    public void closeProgram()
    {
        doShutDown();
        Application.Quit();
    }
}
