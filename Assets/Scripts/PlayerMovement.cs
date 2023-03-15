using System;

using Unity.Netcode;

using UnityEngine;

using UnityEngine.InputSystem;

//https://gamedevbeginner.com/how-to-jump-in-unity-with-or-without-physics/#jump_unity � jump based on this
public class PlayerMovement : NetworkBehaviour
{
	//public NetworkVariable<bool>    side            = new(false);
	public NetworkVariable<float>   gravity				= new(6f);

	public NetworkVariable<Vector2> moveinput			= new(new Vector2(0, 0), writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<float>   jumpInput           = new(0f, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<float>   stompInput           = new(0f, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<float>   RunInput           = new(0f, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<bool>	grounded			= new(false, writePerm: NetworkVariableWritePermission.Owner);

	//public NetworkVariable<float>   buttonTime        = new(0.5f);
	//public NetworkVariable<float>   jumpHeight        = new(5);
	//public NetworkVariable<float>   cancelRate        = new(100);
	//NetworkVariable<float>          jumpTime          = new();
	//NetworkVariable<bool>			jumping           = new();
	//NetworkVariable<bool>           jumpCancelled     = new();
	//public NetworkVariable<bool>    falling           = new(true);

	//public float jumpForce = 10f; // the force with which the character will jump
	//public float maxJumpTime = 1f; // the maximum amount of time the character can jump
	//public float maxJumpHeight = 4f; // the maximum height the character can jump
	//public bool isJumping = false; // a flag to check if the character is jumping
	//public float jumpTime = 0f; // the current amount of time the character has been jumping
	//public float initialJumpForce; // the initial force with which the character will jump
	//public float startY; // the y position of the character when it starts jumping



	public NetworkVariable<Vector2> offset            = new(new Vector2(0f,0.1f));
	public NetworkVariable<float>   distanceRay        = new(0.1f);
	public NetworkVariable<float> jumpStrength = new(24f, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<bool> jumping = new(false, writePerm: NetworkVariableWritePermission.Owner);

	public NetworkVariable<Vector2> lastGroundPosition = new(Vector2.zero, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<float> airtime = new(-1f, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<float> maxFallTime = new(9f);


	//public NetworkVariable<Vector2> surfacePosition   = new();
	//ContactFilter2D filter;
	//RaycastHit2D[] results = new RaycastHit2D[3];

	//public NetworkVariable<float>   jumpStrengh       = new(480f);
	//public NetworkVariable<int>     jumped            = new(0);
	//public NetworkVariable<int>     maxJumped         = new(3);

	public NetworkVariable<float>   speed             = new(81f, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<bool>    notSlide          = new(false, writePerm : NetworkVariableWritePermission.Owner);
	public NetworkVariable<Vector2> maxSpeed          = new(new Vector2(3, 3), writePerm: NetworkVariableWritePermission.Owner);

	public NetworkVariable<bool> isOld = new(false);
	public NetworkVariable<bool> canStomping = new(false, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<bool> isStomping = new(false, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<bool> canJump = new(false, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<float> maxDurationStomp = new(0.6f);
	public NetworkVariable<float> lastStompActivation = new(-1f, writePerm: NetworkVariableWritePermission.Owner);
	public NetworkVariable<float> stompStrength = new(32f);
	public NetworkVariable<float> waterSpeedSlowdownFactor = new(0.75f);
	public NetworkVariable<bool> isCollidingWithSpikes = new(false, writePerm: NetworkVariableWritePermission.Owner);

	public NetworkVariable<float> runSpeedUpFactor = new(1.25f);

	public Animator animator;

	public Controls controls;




	//public float drag = 5f;

	private Rigidbody2D rb;

	private void Awake()
	{
		controls = new Controls();

		controls.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
		controls.Player.Move.canceled += ctx => Move(ctx.ReadValue<Vector2>());

		controls.Player.Jump.performed += ctx => Jump(ctx.ReadValue<float>());
		controls.Player.Jump.canceled += ctx => Jump(ctx.ReadValue<float>());

		controls.Player.Stomp.performed += ctx => Stomp(ctx.ReadValue<float>());
		controls.Player.Stomp.canceled += ctx => Stomp(ctx.ReadValue<float>());

		controls.Player.Run.performed += ctx => Run(ctx.ReadValue<float>());
		controls.Player.Run.canceled += ctx => Run(ctx.ReadValue<float>());
	}

	private void OnEnable()
	{
		controls.Player.Enable();
	}

	private void OnDisable()
	{
		controls.Player.Disable();
	}

	void Move(Vector2 input)
	{
		if (IsOwner)
		{
			moveinput.Value = input;
		}
	}

	void Jump(float input)
	{
		if (IsOwner)
		{
			jumpInput.Value = input;
		}
	}

	void Stomp(float input)
	{
		if (IsOwner)
		{
			stompInput.Value = input;
		}
	}

	void Run(float input)
	{
		if (IsOwner)
		{
			RunInput.Value = input;
		}
	}

	// Start is called before the first frame update
	void Start()
	{
		rb = gameObject.GetComponent<Rigidbody2D>();
		rb.gravityScale = gravity.Value;
		animator = gameObject.GetComponent<Animator>();

		lastGroundPosition.Value = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);

		//initialJumpForce = jumpForce;

	//side.OnValueChanged += onSideChange;
	}

	public bool isGroundedMiddle()
	{
		RaycastHit2D hitMiddle = Physics2D.Raycast(new Vector2(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y - GetComponent<Collider2D>().bounds.extents.y) - new Vector2(0, offset.Value.y), -Vector2.up, distanceRay.Value);
		if (hitMiddle.collider == null) // || (hitMiddle.collider != null && hitMiddle.collider.tag != "Ground"))
		{
			return false;
		}

		return true;
	}

	public bool isGrounded()
    {
		RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(GetComponent<Collider2D>().bounds.min.x, GetComponent<Collider2D>().bounds.min.y) - new Vector2(offset.Value.x, offset.Value.y), -Vector2.up, distanceRay.Value);
		if (hitLeft.collider == null) //|| (hitLeft.collider != null && hitLeft.collider.tag != "Ground"))
		{
			RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(GetComponent<Collider2D>().bounds.center.x + GetComponent<Collider2D>().bounds.extents.x, GetComponent<Collider2D>().bounds.center.y - GetComponent<Collider2D>().bounds.extents.y) + new Vector2(offset.Value.x, -offset.Value.y), -Vector2.up, distanceRay.Value);
			if (hitRight.collider == null) // || (hitRight.collider != null && hitRight.collider.tag != "Ground"))
			{
				RaycastHit2D hitMiddle = Physics2D.Raycast(new Vector2(GetComponent<Collider2D>().bounds.center.x, GetComponent<Collider2D>().bounds.center.y - GetComponent<Collider2D>().bounds.extents.y) - new Vector2(0, offset.Value.y), -Vector2.up, distanceRay.Value);
				if (hitMiddle.collider == null) // || (hitMiddle.collider != null && hitMiddle.collider.tag != "Ground"))
				{
					//Collider2D[] results = new Collider2D[3];
					//int resultNumber = rb.OverlapCollider(new ContactFilter2D().NoFilter(),results);
					//Debug.Log(resultNumber);
					//if(resultNumber == 0)
					//               {
					grounded.Value = false;
					return false;
     //               }
				}
			}
		}

		//if(!grounded.Value)
		//      {
		//	AudioManager.Instance.PlaySFX("Jump Landed");
		//      }

		grounded.Value = true;
		return true;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		Debug.Log("Enter Coll 2D of " + collision.gameObject.tag + " is Grounded: " + grounded.Value + " | " + isGrounded());
		if (isGrounded() && collision.gameObject.tag == "Ground")
		{
			canJump.Value = true;
			if (!isCollidingWithSpikes.Value && isGroundedMiddle())
			{
				lastGroundPosition.Value = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
			}
			airtime.Value = -1;
			lastStompActivation.Value = -1;

			rb.velocity = Vector2.down * stompStrength.Value;
			rb.inertia = 1;

			AudioManager.Instance.PlaySFX("Jump Landed");
		}

		if (collision.gameObject.tag == "Spikes")
		{
			isCollidingWithSpikes.Value = true;

			if (!isOld.Value)
			{
				//if ((collision.collider.ClosestPoint(gameObject.transform.position) - (new Vector2(gameObject.transform.position.x, gameObject.transform.position.y))).x < 0)
				//{
				//	gameObject.transform.position = lastGroundPosition.Value + new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				//}
				//else
				//{
				//	gameObject.transform.position = lastGroundPosition.Value - new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				//}
				gameObject.transform.position = lastGroundPosition.Value;
				rb.velocity = Vector2.zero;
				Debug.Log("here1");
			}
			else if (!canStomping.Value || (canStomping.Value && (!isStomping.Value && !(Time.realtimeSinceStartup - lastStompActivation.Value <= maxDurationStomp.Value))))
			{
				//if ((collision.collider.ClosestPoint(gameObject.transform.position) - (new Vector2(gameObject.transform.position.x, gameObject.transform.position.y))).x < 0)
				//{
				//	gameObject.transform.position = lastGroundPosition.Value + new Vector2(GetComponent<Collider2D>().bounds.size.x,0);
				//}
				//else
				//{
				//	gameObject.transform.position = lastGroundPosition.Value - new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				//}
				gameObject.transform.position = lastGroundPosition.Value;
				rb.velocity = Vector2.zero;
				Debug.Log("here2");
			}
			else if (isStomping.Value && isOld.Value && (Time.realtimeSinceStartup - lastStompActivation.Value <= maxDurationStomp.Value))
			{
				rb.velocity = Vector2.down * stompStrength.Value;
				airtime.Value = Time.realtimeSinceStartup;
				Debug.Log("here3");
			}
			else
			{
				Debug.Log("here4");
				//if ((collision.collider.ClosestPoint(gameObject.transform.position) - (new Vector2(gameObject.transform.position.x, gameObject.transform.position.y))).x < 0)
				//{
				//	gameObject.transform.position = lastGroundPosition.Value + new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				//}
				//else
				//{
				//	gameObject.transform.position = lastGroundPosition.Value - new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				//}
				gameObject.transform.position = lastGroundPosition.Value;
				rb.velocity = Vector2.zero;
			}
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Spikes")
		{
			isCollidingWithSpikes.Value = false;
		}

		canJump.Value = false;

		Debug.Log("Exit Coll 2D of " + collision.gameObject.tag + " is Grounded: " + grounded.Value + " | " + isGrounded());
		if(isGrounded() && collision.gameObject.tag == "Ground")
		{
			airtime.Value = Time.realtimeSinceStartup;
			if (!isCollidingWithSpikes.Value && isGroundedMiddle())
			{
				lastGroundPosition.Value = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
			}
		}
	}

	private void OnCollisionStay2D(Collision2D collision)
	{
		if (collision.gameObject.tag == "Spikes")
		{
			isCollidingWithSpikes.Value = true;
		}

		//Debug.Log("Exit Coll 2D of " + collision.gameObject.tag + " is Grounded: " + grounded.Value + " | " + isGrounded());
		if (isGrounded() && collision.gameObject.tag == "Ground")
		{
			canJump.Value = true;
			airtime.Value = -1; 
			lastStompActivation.Value = -1;
			if (!isCollidingWithSpikes.Value && isGroundedMiddle())
			{
				lastGroundPosition.Value = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Water")
		{
			if (!isOld.Value)
			{
				//if ((collision.ClosestPoint(gameObject.transform.position) - (new Vector2(gameObject.transform.position.x, gameObject.transform.position.y))).x < 0)
				//{
				//	gameObject.transform.position = lastGroundPosition.Value + new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				//}
				//else
				//{
				//	gameObject.transform.position = lastGroundPosition.Value - new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				//}
				gameObject.transform.position = lastGroundPosition.Value;
				//rb.velocity = Vector2.zero;
			}
			else
			{
				maxSpeed.Value *= waterSpeedSlowdownFactor.Value;
				speed.Value *= waterSpeedSlowdownFactor.Value;
			}
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Water")
		{
			if (!isOld.Value)
			{
				//if ((collision.ClosestPoint(gameObject.transform.position) - (new Vector2(gameObject.transform.position.x, gameObject.transform.position.y))).x < 0)
				//{
				//	gameObject.transform.position = lastGroundPosition.Value + new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				//}
				//else
				//{
				//	gameObject.transform.position = lastGroundPosition.Value - new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				//}
				gameObject.transform.position = lastGroundPosition.Value;
				//rb.velocity = Vector2.zero;
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "Water")
		{
			if (!isOld.Value)
			{
				if ((collision.ClosestPoint(gameObject.transform.position) - (new Vector2(gameObject.transform.position.x, gameObject.transform.position.y))).x < 0)
				{
					gameObject.transform.position = lastGroundPosition.Value - new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				}
				else
				{
					gameObject.transform.position = lastGroundPosition.Value + new Vector2(GetComponent<Collider2D>().bounds.size.x, 0);
				}
				//rb.velocity = Vector2.zero;
			}
			else
			{
				maxSpeed.Value /= waterSpeedSlowdownFactor.Value;
				speed.Value /= waterSpeedSlowdownFactor.Value;
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		rb.gravityScale = gravity.Value;

		isGrounded();

		if(jumping.Value && rb.velocity.y < 0)
        {
			jumping.Value = false;
        }

		if(isStomping.Value && rb.velocity.y > 0)
		{
			isStomping.Value = false;
		}
		if(!isStomping.Value && rb.velocity.y <= 0)
		{
			lastStompActivation.Value = -1;
		}

		if(airtime.Value != -1)
		{
			if(Time.realtimeSinceStartup - airtime.Value > maxFallTime.Value)
			{
				airtime.Value = -1;
				transform.position = lastGroundPosition.Value;
				rb.velocity = Vector2.zero;
			}
		}
		else
		{
			if(rb.velocity.y != 0)
				airtime.Value = Time.realtimeSinceStartup;
		}

		//if(jumping.Value && rb.velocity.y <= 0)
		//      {
		//	jumping.Value = false;
		//      }

		////if (Physics2D.OverlapBox(GetComponent<Collider2D>().bounds.size, GetComponent<Collider2D>().bounds.size, 0f, filter.NoFilter(), results) > 0)
		////if (rb.OverlapCollider(filter.NoFilter(), results) > 0)
		////int resulttt = Physics2D.Raycast(GetComponent<Collider2D>().bounds.min, Vector2.down, filter.NoFilter(), results, offset.Value);
		//RaycastHit2D hitLeft = Physics2D.Raycast(new Vector2(GetComponent<Collider2D>().bounds.min.x, GetComponent<Collider2D>().bounds.min.y) - new Vector2(offset.Value.x, offset.Value.y), -Vector2.up, distanceRay.Value);
		//// If it hits something...
		//if (hitLeft.collider != null)
		//{
		//	// Calculate the distance from the surface and the "error" relative
		//	// to the floating height.

		//	//Debug.Log(hitLeft.collider.gameObject + " | " + hit.distance);

		//	if(hitLeft.distance < 0.0001)
		//          {
		//		falling.Value = false;
		//	}
		//          else
		//          {
		//		RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(GetComponent<Collider2D>().bounds.center.x + GetComponent<Collider2D>().bounds.extents.x, GetComponent<Collider2D>().bounds.center.y - GetComponent<Collider2D>().bounds.extents.y) + new Vector2(offset.Value.x, -offset.Value.y), -Vector2.up, distanceRay.Value);
		//		if (hitRight.collider != null)
		//		{
		//			if (hitRight.distance < 0.0001)
		//			{
		//				falling.Value = false;
		//			}
		//			else
		//			{
		//				falling.Value = true;
		//			}
		//		}
		//              else
		//              {
		//			falling.Value = true;
		//		}
		//	}

		//	//float heightError = floatHeight - distance;

		//	//// The force is proportional to the height error, but we remove a part of it
		//	//// according to the object's speed.
		//	//float force = liftForce * heightError - rb2D.velocity.y * damping;

		//	//// Apply the force to the rigidbody.
		//	//rb2D.AddForce(Vector3.up * force);
		//}
		//      else
		//      {
		//	RaycastHit2D hitRight = Physics2D.Raycast(new Vector2(GetComponent<Collider2D>().bounds.center.x + GetComponent<Collider2D>().bounds.extents.x, GetComponent<Collider2D>().bounds.center.y - GetComponent<Collider2D>().bounds.extents.y) + new Vector2(offset.Value.x, -offset.Value.y), -Vector2.up, distanceRay.Value);
		//	if (hitRight.collider != null)
		//	{
		//		if (hitRight.distance < 0.0001)
		//		{
		//			falling.Value = false;
		//		}
		//		else
		//		{
		//			falling.Value = true;
		//		}
		//	}
		//	else
		//	{
		//		falling.Value = true;
		//	}
		//}

		////if (resulttt > 1)
		////{
		////	Debug.Log(" | " + resulttt);
		////	Debug.Log(results[0].collider.gameObject + " | " + results[1].collider.gameObject + " | " + results[2].collider.gameObject);
		////	falling.Value = false;
		////	//surfacePosition.Value = Physics2D.ClosestPoint(transform.position, results[0]);
		////}
		////else
		////{
		////	Debug.Log(" | " + resulttt);
		////	falling.Value = true;
		////}

		if (IsOwner)
		{
			//if (Physics2D.Raycast(transform.position, Vector2.down, distanceToGround.Value))
			//{
			//	falling.Value = true;
			//}
			//else
			//{
			//	falling.Value = false;
			//}

			//moveinput.Value = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

			//jumpInput.Value = Input.GetAxisRaw("Jump");

			//if (isGrounded() && jumpInput.Value != 0)
			//{
			//	isJumping = true;
			//	jumpTime = 0f;
			//	startY = transform.position.y;
			//}

			//if (!falling.Value && !jumping.Value && jumpInput.Value != 0)
			//{
			//	float jumpForce = Mathf.Sqrt(jumpHeight.Value * -2 * (Physics2D.gravity.y * rb.gravityScale));
			//	rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
			//	jumping.Value = true;
			//	jumpCancelled.Value = false;
			//	jumpTime.Value = 0;
			//}
			//if (jumping.Value)
			//{
			//	falling.Value = false;

			//	jumpTime.Value += Time.deltaTime;
			//	if (jumpInput.Value == 0)
			//	{
			//		jumpCancelled.Value = true;
			//	}
			//	if (jumpTime.Value > buttonTime.Value)
			//	{
			//		jumping.Value = false;
			//	}
			//}
		}

		if(canJump.Value && !jumping.Value && grounded.Value && jumpInput.Value != 0)
        {
			jumping.Value = true;
			rb.velocity = new(rb.velocity.x,0);
			rb.AddForce((Vector2.up * (jumpInput.Value * (jumpStrength.Value /* * (-1 * Physics2D.gravity.y * rb.gravityScale)*/))) /* * Time.deltaTime*/, ForceMode2D.Impulse);
			AudioManager.Instance.PlaySFX("Jump");
		}

		if(isOld.Value && canStomping.Value && !grounded.Value && !isStomping.Value && lastStompActivation.Value == -1 && stompInput.Value > 0)
		{
			isStomping.Value = true;
			rb.AddForce((Vector2.down * jumpStrength.Value), ForceMode2D.Impulse);
			lastStompActivation.Value = Time.realtimeSinceStartup;
		}

		//if (isJumping && jumpTime >= maxJumpTime)
		//{
		//	isJumping = false;
		//}
		//else if (isJumping && transform.position.y > maxJumpHeight)
		//{
		//	isJumping = false;
		//}

		//Debug.Log(jumpTime.Value + " | " + buttonTime.Value + " | " + falling.Value + " | " + jumping.Value);

		animator.SetFloat("MoveInputX", moveinput.Value.x);
		animator.SetFloat("JumpInput", Convert.ToSingle(jumping.Value));
		animator.SetFloat("StompInput", Convert.ToSingle(isStomping.Value));
		animator.SetFloat("RunInput", Convert.ToSingle(RunInput.Value));
		animator.SetFloat("Falling", Convert.ToSingle(rb.velocity.y < 0 && lastStompActivation.Value == -1));
	}



	//private void onSideChange(bool previous, bool current)
	//{
	//	gameObject.GetComponent<Rigidbody2D>().gravityScale = Convert.ToInt32(current) * gravity.Value;
	//}


	private void FixedUpdate()
	{
		//if (isGrounded())
		//{
		//	GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0f);
		//}


		//if (jumpCancelled.Value && jumping.Value && rb.velocity.y > 0)
		//{
		//	jumping.Value = false;
		//	float cancelForce = Mathf.Sqrt(jumpHeight.Value * -2 * (Physics2D.gravity.y * rb.gravityScale));
		//	rb.AddForce(Vector2.down * cancelForce * cancelRate.Value, ForceMode2D.Impulse);
		//}

		//if (isJumping && jumpTime < maxJumpTime)
		//{
		//	// calculate the amount of force to apply to the character based on the time the jump button was pressed
		//	float jumpForceMultiplier = Mathf.Lerp(0f, 1f, jumpTime / maxJumpTime);
		//	jumpForce = initialJumpForce * jumpForceMultiplier;

		//	// apply the force to the character
		//	GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
		//	//GetComponent<Rigidbody2D>().AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

		//	jumpTime += Time.fixedDeltaTime;

		//	// limit the maximum jump height relative to the starting y position
		//	if (transform.position.y - startY > maxJumpHeight)
		//	{
		//		GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0f);
		//		isJumping = false;
		//	}
		//}
		//else
		//{
		//	isJumping = false;
		//	jumpForce = initialJumpForce;
		//}

		if (notSlide.Value)
		{
			rb.velocity = new Vector2(0, rb.velocity.y);
		}

		if (RunInput.Value < 0.1)
		{
			rb.AddForce(new Vector2(moveinput.Value.x * speed.Value, 0));

			rb.velocity = new Vector2(Math.Sign(rb.velocity.x) * Math.Min(maxSpeed.Value.x, Math.Abs(rb.velocity.x)), rb.velocity.y);
		}
		else
		{
			rb.AddForce(new Vector2(moveinput.Value.x * speed.Value * runSpeedUpFactor.Value, 0));

			rb.velocity = new Vector2(Math.Sign(rb.velocity.x) * Math.Min(maxSpeed.Value.x * runSpeedUpFactor.Value, Math.Abs(rb.velocity.x)), rb.velocity.y);
		}
	}
}
