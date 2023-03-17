using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blockade : MonoBehaviour
{
	public Animator animator;

	public bool openByLever;
	public bool openByLeverArray;

	public Interactable_Lever levertoActiavte;

	public Interactable_Lever[] leversToActivate;
	public bool[] leverCode;

	private bool isClosed = true;

    // Start is called before the first frame update
    void Start()
    {
		if (openByLever)
		{
			levertoActiavte.OnLeverActivation += OpenBlockade;
			levertoActiavte.OnLeverDeactivation += CloseBlockade;
		}
    }

	private void OpenBlockade(object sender, System.EventArgs e)
	{
		OpenBlockade();
	}

	public void OpenBlockade()
	{
		gameObject.GetComponent<BoxCollider2D>().enabled = false;
		animator.SetTrigger("open");
		isClosed = false;
	}

	private void CloseBlockade(object sender, System.EventArgs e)
	{
		CloseBlockade();
	}

	public void CloseBlockade()
	{
		gameObject.GetComponent<BoxCollider2D>().enabled = true;
		animator.SetTrigger("close");
		isClosed = true;
	}

	// Update is called once per frame
	void Update()
    {
        if(openByLeverArray)
		{
			bool doOpen = true;
			for (int i = 0; i < leversToActivate.Length; i++)
			{
				if(leversToActivate[i].isPulled != leverCode[i])
				{
					doOpen = false;
					break;
				}
			}

			if(doOpen && isClosed)
			{
				OpenBlockade();
			}
			else if(!doOpen && !isClosed)
			{
				CloseBlockade();
			}
		}
    }
}
