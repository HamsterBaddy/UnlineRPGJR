using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public Vector2 moveinput = new Vector2(0,0);
    //public Vector2 oppositeForce = new Vector2(0,0);

    public float speed = 3f;
    public bool notSlide = false;
    public Vector2 maxSpeed = new Vector2(3, 3);
    //public float drag = 5f;

    private Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            moveinput.y = Input.GetAxisRaw("Vertical");
            moveinput.x = Input.GetAxisRaw("Horizontal");

            //oppositeForce = -rb.velocity.normalized * drag;

            setMoveInput_ServerRpc(moveinput, NetworkObjectId);
        }
    }

    private void FixedUpdate()
    {
        if (notSlide)
        {
            if (moveinput.x == 0) rb.velocity = new Vector2(0, rb.velocity.y);
            if (moveinput.y == 0) rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        rb.AddForce(moveinput * speed);

        if (rb.velocity.x > maxSpeed.x) rb.velocity = new Vector2(maxSpeed.x, rb.velocity.y);
        if (rb.velocity.y > maxSpeed.y) rb.velocity = new Vector2(rb.velocity.x, maxSpeed.y);
        //nicht den M64 machen
        if (rb.velocity.x < -maxSpeed.x) rb.velocity = new Vector2(-maxSpeed.x, rb.velocity.y);
        if (rb.velocity.y < -maxSpeed.y) rb.velocity = new Vector2(rb.velocity.x, -maxSpeed.y);
    }

    [ServerRpc]
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
    }
}
