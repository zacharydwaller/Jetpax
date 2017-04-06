using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/* Squid
 * Sinusoidal Pattern
 *     Magnitude:   1.5
 *     Period:      0.25
 * Speed: 5.0
 */

public class Enemy_Squid : NetworkBehaviour
{
    // Class Constants ////////////////////////////////////////////////////////

    private const float X_SPEED = 10f;
    private const float Y_MAGNITUDE = 2.5f;
    private const float Y_PERIOD = .25f;
    private const float LEFT_SCALAR = -1.0f;
    private const float RIGHT_SCALAR = 1.0f;

    // Class Variables ////////////////////////////////////////////////////////

    // Components
    private Enemy_Base parent;
    private Rigidbody2D rgb;

    // Class Methods //////////////////////////////////////////////////////////

    private void Start()
    {
        parent = GetComponent<Enemy_Base>();
        rgb = GetComponent<Rigidbody2D>();

        rgb.gravityScale = 0.0f;
    }

    private void Update()
    {
        if (!isServer) return;

        if (parent.isActive == false) return;
        if (parent.isDying == false) PatternMove();
    }


    // Sinusoidal 
    private void PatternMove()
    {
        if (!isServer) return;

        Vector2 finalMotion = new Vector2();

        if (parent.isFacingLeft)
        {
            finalMotion.x = X_SPEED * LEFT_SCALAR;
        }
        else
        {
            finalMotion.x = X_SPEED * RIGHT_SCALAR;
        }

        finalMotion.y = (Y_MAGNITUDE * Mathf.Sin(Time.time / Y_PERIOD));

        //transform.Translate(finalMotion * Time.deltaTime);

        rgb.velocity = finalMotion;
    }
}
