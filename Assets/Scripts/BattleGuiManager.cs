using System;
using System.Net;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class BattleGuiManager : MonoBehaviour
{
    private static BattleGuiManager _singelton;
    public static BattleGuiManager Singelton { get => _singelton; set { if (_singelton == null) _singelton = value; } }

    [field: SerializeField]
    public Button returnButton { get; set; }

    BattleGuiManager()
    {
        if (Singelton != null)
            throw new InvalidOperationException("Es kann nur eine BattleGuiManager existieren");
    }

    // Start is called before the first frame update
    void Start()
    {
        BattleGuiManager.Singelton = this;
        returnButton.onClick.AddListener(SceneChangeManager.Singelton.returnToPreviousScene);
    }
}
