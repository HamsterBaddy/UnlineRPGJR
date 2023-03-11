using System;

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
			throw new InvalidOperationException($"Es kann nur eine {nameof(BattleGuiManager)} existieren");
	}

	// Start is called before the first frame update
	void Start()
	{
		Singelton = this;
		returnButton.onClick.AddListener(SceneChangeManager.Singelton.returnToPreviousScene);
	}
}
