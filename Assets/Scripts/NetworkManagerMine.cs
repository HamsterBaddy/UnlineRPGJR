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
    }

    public void startClient()
    {
        this.StartClient();
    }

    public void startServer()
    {
        this.StartServer();
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
