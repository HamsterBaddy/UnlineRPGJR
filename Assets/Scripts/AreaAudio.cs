using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class AreaAudio
{
	public string name;

	public Vector2 maxPosition;
	public Vector2 minPosition;

	public bool inside(GameObject player)
	{
		if (player != null)
		{
			for (int i = 0; i < 2; ++i)
			{
				if (player.transform.position[i] >= minPosition[i] && player.transform.position[i] <= maxPosition[i])
				{
					return true;
				}
			}
		}
		return false;
	}
}