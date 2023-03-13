using UnityEngine.SceneManagement;

[System.Serializable]
public class SceneAudio
{
    public string sceneName;
    public string name;

    public bool shouldPlay()
    {
        return SceneManager.GetActiveScene().name == sceneName;
    }
}
