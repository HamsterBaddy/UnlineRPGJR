using System;

using Unity.Netcode;

using UnityEngine;

public class PlayerMovement : NetworkBehaviour
{
	public NetworkVariable<bool> isGrounded = new();
	public NetworkVariable<float> velocity = new(0f);
	public GroundCheckWithSnapping groundCheck;

	public NetworkVariable<bool>    side            = new(false);
	public NetworkVariable<float>   gravity         = new(6f);

	public NetworkVariable<Vector2> moveinput       = new(new Vector2(0, 0));
	public NetworkVariable<float>   jumpInput       = new(0f);

	public NetworkVariable<float> buttonTime        = new(0.5f);
	public NetworkVariable<float> jumpHeight        = new(5);
	public NetworkVariable<float> cancelRate        = new(100);
	NetworkVariable<float> jumpTime                 = new();
	NetworkVariable<bool> jumping                   = new();
	NetworkVariable<bool> jumpCancelled             = new();

	//public NetworkVariable<float>   jumpStrengh     = new(480f);
	//public NetworkVariable<int>     jumped          = new(0);
	//public NetworkVariable<int>     maxJumped       = new(3);

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
		//if (isGrounded.Value && velocity.Value < 0)
		//{
		//	float floorHeight = 0.7f;
		//	velocity.Value = 0;
		//	transform.position = new Vector3(transform.position.x, groundCheck.surfacePosition.y + floorHeight, transform.position.z);
		//}

		if (IsOwner)
		{
			moveinput.Value = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

			jumpInput.Value = Input.GetAxisRaw("Jump");

			if (jumpInput.Value != 0)
			{
				float jumpForce = Mathf.Sqrt(jumpHeight.Value * -2 * (Physics2D.gravity.y * rb.gravityScale));
				rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
				jumping.Value = true;
				jumpCancelled.Value = false;
				jumpTime.Value = 0;
			}
			if (jumping.Value)
			{
				jumpTime.Value += Time.deltaTime;
				if (jumpInput.Value == 0)
				{
					jumpCancelled.Value = true;
				}
				if (jumpTime.Value > buttonTime.Value)
				{
					jumping.Value = false;
				}
			}

			/*if (isGrounded.Value && jump.Value > 0)
			{
				jumped.Value = 1;
			}
			else if (rb.velocity.y < 0)
			{
				jumped.Value = 61;
			}*/

			//oppositeForce = -rb.velocity.normalized * drag;

			//setMoveInput_ServerRpc(moveinput.Value, NetworkObjectId);
		}
	}

	private void onSideChange(bool previous, bool current)
	{
		gameObject.GetComponent<Rigidbody2D>().gravityScale = Convert.ToInt32(current) * gravity.Value;
	}


	private void FixedUpdate()
	{
		//Debug.Log($"Jump: {jump.Value} | Jumped: {jumped.Value}");

		if (notSlide.Value)
		{
			rb.velocity = new Vector2(0, 0);
		}

		if (!side.Value)
		{
			rb.AddForce(moveinput.Value * speed.Value);


			rb.velocity = new Vector2(Math.Sign(rb.velocity.x) * Math.Min(maxSpeed.Value.x, Math.Abs(rb.velocity.x)),
									  Math.Sign(rb.velocity.y) * Math.Min(maxSpeed.Value.y, Math.Abs(rb.velocity.y)));
		}
		else
		{
			if (jumpCancelled.Value && jumping.Value && rb.velocity.y > 0)
			{
				rb.AddForce(Vector2.down * cancelRate.Value);
			}

			rb.AddForce(new Vector2(moveinput.Value.x * speed.Value, 0));

			rb.velocity = new Vector2(Math.Sign(rb.velocity.x) * Math.Min(maxSpeed.Value.x, Math.Abs(rb.velocity.x)), rb.velocity.y);

			/*if (jumped.Value > 0 && jumped.Value <= maxJumped.Value)
			{
				rb.AddForce(new Vector2(0, jumpStrengh.Value));
				jumped.Value += 1;
			}*/
		}
	}

	//private void OnCollisionEnter2D(Collision2D collision)
	//{
	//	ResetJumped();
	//}
	//private void OnCollisionStay2D(Collision2D collision)
	//{
	//	ResetJumped();
	//}

	//private void ResetJumped()
	//{
	//	jumped.Value = 0;
	//}

	/*[ServerRpc]
    void setMoveInput_ServerRpc(Vector2 value, ulong sourceNetworkObjectId)
    {
        setMoveInput_ClientRpc(value, sourceNetworkObjectId);
    }

    [ClientRpc]
    void setMoveInput_ClientRpc(Vector2 value, ulong sourceNetworkObjectId)
    {
        moveinput.x = value.x;
        moveinput.y = value.y;

        //oppositeForce.x = opForce.x;
        //oppositeForce.y = opForce.y;
    }*/
}
