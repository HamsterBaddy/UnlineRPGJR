using UnityEngine;
using TMPro;

public class Interactable_Scroll : Interactable
{
	public TMP_Text textDisplay;
	public GameObject canvas;

	private static bool isFirst = true;

	[MultilineAttribute]
	public string text;

	[MultilineAttribute]
	public string alternativeText;

	protected override void OnInteraction()
	{
		if (!canvas.activeSelf)
		{
			Debug.Log("ScrollInteraction");
			textDisplay.text = text;
			canvas.SetActive(true);
		}
		else if(isFirst)
		{
			Debug.Log("ScrollInteraction");
			textDisplay.text = text;
			isFirst = false;
		}
	}

	protected override void OnStop()
	{
		Debug.Log("ScrollStop");
		canvas.SetActive(false);
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Player" && collision.gameObject == localPlayer)
		{
			if (isFirst)
			{
				textDisplay.text = alternativeText;
				canvas.SetActive(true);
			}
		}
	}
}
