using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public Vector2 moveinput = new Vector2(0,0);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            moveinput.y = Input.GetAxisRaw("Vertical");
            moveinput.x = Input.GetAxisRaw("Horizontal");

            setMoveInput_ServerRpc(moveinput, NetworkObjectId);
        }
    }

    private void FixedUpdate()
    {
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();

        rb.AddForce(moveinput);
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
    }
}
