using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/**
    https://gamedevbeginner.com/how-to-jump-in-unity-with-or-without-physics/#jump_unity
**/
public class GroundCheckWithSnapping : NetworkBehaviour
{
    public PlayerMovement player;
    public float offset = 0.1f;
    public Vector2 surfacePosition;
    ContactFilter2D filter;
    Collider2D[] results = new Collider2D[1];
    private void Update()
    {
        Vector2 point = transform.position + Vector3.down * offset;
        Vector2 size = new Vector2(transform.localScale.x, transform.localScale.y);
        if (Physics2D.OverlapBox(point, size, 0, filter.NoFilter(), results) > 0)
        {
            player.isGrounded.Value = true;
            surfacePosition = Physics2D.ClosestPoint(transform.position, results[0]);
        }
        else
        {
            player.isGrounded.Value = false;
        }
    }
}
