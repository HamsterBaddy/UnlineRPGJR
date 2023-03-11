using System;
using System.Net;
using System.Reflection;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class TopDownGuiManager : MonoBehaviour
{
    private static TopDownGuiManager _singelton;
    public static TopDownGuiManager Singelton { get => _singelton; set { if (_singelton == null) _singelton = value; } }

    [field: SerializeField]
    public Button enterBattleButton { get; set; }

    [field: SerializeField]
    public Button enterSideButton { get; set; }

    TopDownGuiManager()
    {
        if (Singelton != null)
            throw new InvalidOperationException("Es kann nur eine TopDownGuiManager existieren");
    }

    // Start is called before the first frame update
    void Start()
    {
        TopDownGuiManager.Singelton = this;
        enterBattleButton.onClick.AddListener(TaskOnClickBattle);
        enterSideButton.onClick.AddListener(TaskOnClickSide);
    }

    public void TaskOnClickBattle()
    {
        SceneChangeManager.Singelton.changeScene("Battle");
    }

    public void TaskOnClickSide()
    {
        SceneChangeManager.Singelton.changeScene("SideWorld");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
