using UnityEngine;

public class JumpController : MonoBehaviour
{
    [SerializeField] private float jumpHeight = 4f;
    //[SerializeField] private float jumpTime = 0.4f;
    [SerializeField] private float jumpForce;
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float airControl = 0.8f;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool canDoubleJump;
    private bool isTouchingWall;
    private bool isTouchingLeftWall;
    private bool isTouchingRightWall;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(transform.position, 0.2f);
        isTouchingLeftWall = Physics2D.Raycast(transform.position, -transform.right, 0.5f);
        isTouchingRightWall = Physics2D.Raycast(transform.position, transform.right, 0.5f);

        if (isGrounded)
        {
            canDoubleJump = true;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump(jumpHeight);
            }
            else if (canDoubleJump)
            {
                Jump(jumpHeight);
                canDoubleJump = false;
            }
            else if (isTouchingLeftWall || isTouchingRightWall)
            {
                WallJump(isTouchingLeftWall ? -1 : 1);
            }
        }
    }

    private void FixedUpdate()
    {
        float move = Input.GetAxisRaw("Horizontal");

        if (isGrounded)
        {
            rb.velocity = new Vector2(move * jumpForce, rb.velocity.y);
        }
        else if (!isTouchingWall)
        {
            rb.velocity += new Vector2(move * jumpForce * airControl, 0);
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -jumpForce, jumpForce), rb.velocity.y);
        }
    }

    private void Jump(float height)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(Vector2.up * Mathf.Sqrt(height * -2f * Physics2D.gravity.y), ForceMode2D.Impulse);
    }

    private void WallJump(int dir)
    {
        rb.velocity = new Vector2(0, 0);
        bool jumpLeft = Input.GetKey(KeyCode.A);
        rb.AddForce(new Vector2(wallJumpForce * (jumpLeft ? 1 : -1) * dir, jumpHeight), ForceMode2D.Impulse);
    }
}