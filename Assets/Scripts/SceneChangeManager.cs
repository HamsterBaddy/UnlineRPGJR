using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeManager : MonoBehaviour
{
    private static SceneChangeManager _singelton;
    public static SceneChangeManager Singelton { get => _singelton; set { if (_singelton == null) _singelton = value; } }


    public Stack<string> returnScenes = new Stack<string>();

    SceneChangeManager()
    {
        if (Singelton != null)
            throw new InvalidOperationException("Es kann nur ein SceneChangeManager existieren");
    }

    // Start is called before the first frame update
    void Start()
    {
        SceneChangeManager.Singelton = this;
        DontDestroyOnLoad(this);
    }

    public void changeScene(string sceneName, bool addReturn = true)
    {
        if(addReturn) returnScenes.Push(SceneManager.GetActiveScene().name);

        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void returnToPreviousScene()
    {
        changeScene(returnScenes.Pop(), false);
    }
}
