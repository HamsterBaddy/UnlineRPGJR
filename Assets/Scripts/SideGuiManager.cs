using System;

using UnityEngine;
using UnityEngine.UI;

public class SideGuiManager : MonoBehaviour
{
	private static SideGuiManager _singelton;
	public static SideGuiManager Singelton { get => _singelton; set { if (_singelton == null) _singelton = value; } }

	[field: SerializeField]
	public Button enterBattleButton { get; set; }

	[field: SerializeField]
	public Button enterTopDownButton { get; set; }

	SideGuiManager()
	{
		if (Singelton != null)
			throw new InvalidOperationException("Es kann nur eine SideGuiManager existieren");
	}

	// Start is called before the first frame update
	void Start()
	{
		Singelton = this;
		enterBattleButton.onClick.AddListener(() => SceneChangeManager.Singelton.changeScene("Battle"));
		enterTopDownButton.onClick.AddListener(() => SceneChangeManager.Singelton.changeScene("TopDownWorld"));
	}

	//// Update is called once per frame
	//void Update()
	//{

	//}
}
