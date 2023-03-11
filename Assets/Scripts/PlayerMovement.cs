using System;

using Unity.Netcode;

using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
	public NetworkVariable<bool>    side            = new(false);
	public NetworkVariable<float>   gravity         = new(6f);

	public NetworkVariable<Vector2> moveinput       = new(new Vector2(0, 0));
	public NetworkVariable<float>   jump            = new(0f);
	public NetworkVariable<float>   jumpStrengh     = new(480f);
	public NetworkVariable<int>     jumped          = new(0);
	public NetworkVariable<int>     maxJumped       = new(3);

	public NetworkVariable<float>   speed           = new(3f);
	public NetworkVariable<bool>    notSlide        = new(false);
	public NetworkVariable<Vector2> maxSpeed        = new(new Vector2(3, 3));
	//public float drag = 5f;

	private Rigidbody2D rb;


	// Start is called before the first frame update
	void Start()
	{
		rb = gameObject.GetComponent<Rigidbody2D>();

		side.OnValueChanged += onSideChange;
	}

	// Update is called once per frame
	void Update()
	{
		if (IsOwner)
		{
			moveinput.Value = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

			jump.Value = Input.GetAxisRaw("Jump");

			if (jumped.Value == 0 && jump.Value > 0)
			{
				jumped.Value = 1;
			}
			else if (rb.velocity.y < 0)
			{
				jumped.Value = 61;
			}
		}
	}

	private void onSideChange(bool previous, bool current)
	{
		gameObject.GetComponent<Rigidbody2D>().gravityScale = Convert.ToInt32(current) * gravity.Value;
	}


	private void FixedUpdate()
	{
		Debug.Log($"Jump: {jump.Value} | Jumped: {jumped.Value}");

		if (notSlide.Value)
		{
			rb.velocity = new Vector2(0, rb.velocity.y);
		}

		rb.AddForce(new Vector2(moveinput.Value.x * speed.Value, 0));

		rb.velocity = new Vector2(Math.Sign(rb.velocity.x) * Math.Min(maxSpeed.Value.x, Math.Abs(rb.velocity.x)), rb.velocity.y);

		if (jumped.Value > 0 && jumped.Value <= maxJumped.Value)
		{
			rb.AddForce(new Vector2(0, jumpStrengh.Value));
			jumped.Value += 1;
		}
	}
}
