using Unity.Netcode;

using UnityEngine;

public class CameraController : MonoBehaviour
{
	public Transform player;
	public Vector3 offset = new(0,0,-10);
	//public float smoothTime = 0.3f;

	//private Vector3 velocity = Vector3.zero;

	private void Start()
	{
		player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Transform>();
	}

	void Update()
	{
		transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, offset.z); // Camera follows the player with specified offset position

		//Vector3 targetPosition = player.position;
		//targetPosition.z = transform.position.z;

		//transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
	}
}
