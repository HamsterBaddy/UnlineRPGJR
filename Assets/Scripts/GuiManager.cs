using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Net;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Networking;
using Unity.Networking.Transport;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;

public class GuiManager : MonoBehaviour
{
    public TMP_InputField ipAdress;
    public TMP_InputField portt;

    public Button[] StartButtons;

    public Button SetButton;

    public bool successfulIP = false;
    public bool successfulPort = false;

    public void setIP()
    {
        //Assembly ImDumb = Assembly.GetAssembly(typeof(Unity.Networking.Transport.NetworkConnection));
        //ImDumb.GetTypes();

        successfulIP = IPAddress.TryParse(ipAdress.text, out _);

        Debug.Log(ipAdress.text);

        successfulPort = ushort.TryParse(portt.text, out ushort porttt);

        if (successfulPort && successfulIP)
        {
            SetButton.interactable = 
            ipAdress.interactable = 
            portt.interactable = false;

            Debug.Log(porttt);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(
                ipAdress.text,  // The IP address is a string
                porttt // The port number is an unsigned short
            );

            foreach (Button b in StartButtons)
            {
                b.interactable = true;
            }
        }
    }
}
