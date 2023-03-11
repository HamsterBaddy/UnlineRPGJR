using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerActiveManager : NetworkBehaviour
{
    public List<string> noPlayerScenes = new List<string> { "Lobby", "Battle" };

    public GameObject childsprite;

    private void Awake()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += onSceneLoaded;

        setEnableComponents(!noPlayerScenes.Contains(SceneManager.GetActiveScene().name));
    }

    public void onSceneLoaded(UInt64 clientId, String sceneName, LoadSceneMode loadSceneMode)
    {
        if (IsOwner)
        {
            Debug.Log(sceneName + " | " + clientId + " | " + OwnerClientId + " | " + noPlayerScenes.Contains(sceneName) + " | " + noPlayerScenes[0]);
            if (OwnerClientId == clientId)
            {
                setEnableComponents(!(noPlayerScenes.Contains(sceneName)));

                if(sceneName == "TopDownWorld")
                {
                    GetComponent<PlayerMovement>().side.Value = false;
                }
                else if(sceneName == "SideWorld")
                {
                    GetComponent<PlayerMovement>().side.Value = true;
                }
            }
        }
    }

    public void setEnableComponents(bool enable)
    {
        GetComponent<PlayerMovement>().enabled = enable;
        GetComponent<BoxCollider2D>().enabled = enable;
        if (enable)
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        }
        childsprite.SetActive(enable);
    }
}
