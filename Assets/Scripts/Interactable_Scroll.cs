using UnityEngine;
using TMPro;

public class Interactable_Scroll : Interactable
{
	public TMP_Text textDisplay;
	public Canvas canvas;

	private static bool isFirst = true;

	[MultilineAttribute]
	public string text;

	[MultilineAttribute]
	public string alternativeText;

	protected override void OnInteraction()
	{
		if (!canvas.isActiveAndEnabled)
		{
			Debug.Log("ScrollInteraction");
			textDisplay.text = text;
			canvas.enabled = true;
		}
	}

	protected override void OnStop()
	{
		Debug.Log("ScrollStop");
		canvas.enabled = false;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player" && collision.gameObject == localPlayer)
		{
			if (isFirst)
			{
				textDisplay.text = alternativeText;
				canvas.enabled = true;
				isFirst = false;
			}
		}
	}
}
