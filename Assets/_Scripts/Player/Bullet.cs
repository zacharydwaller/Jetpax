using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Bullet : NetworkBehaviour
{
    // Class Constants ////////////////////////////////////////////////////////

    public const float FLIGHTSPEED = 20.0f;
    public const float LEFT_SCALAR = -1.0f;
    public const float RIGHT_SCALAR = 1.0f;
    public const int BASE_DAMAGE = 1;

    // Class Variables ////////////////////////////////////////////////////////

    public PlayerController owner;
    public bool isFacingLeft;
    public int damage;

    // Class Methods //////////////////////////////////////////////////////////
    // Start
    void Start()
    {
        ColorizeRandom();
    }

    // Update
    void Update()
    {
        // Do nothing
    }

    // Collision Enter
    void OnTriggerEnter2D(Collider2D other)
    {
        string otherTag = other.tag;
        if (tag == "Platform")
        {
            Die();
        }
        else if (otherTag == "Enemy")
        {
            owner.CmdAddScore(other.GetComponent<EnemyBase>().scoreReward);
            other.SendMessage("Die");
            Die();
        }
        /* if hits
         *      another player
         *      during a competitive game
         *      that is not invincible
         */
        else if
            (
              otherTag == "Player"
              && other.gameObject.GetComponent<PlayerController>() != owner
              && GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().isCompetitiveGame
              && other.GetComponent<PlayerController>().isInvincible == false
            )
        {
            other.SendMessage("TakeDamage", damage);
            Die();
        }
    }

    // Paint the bullet a random color
    public void ColorizeRandom()
    {
        Color newColor = Color.white;
        int randomInt = Random.Range(1, 7); // 7 pretty colors
        switch (randomInt)
        {
            case 1:
                newColor = Color.blue;
                break;
            case 2:
                newColor = Color.cyan;
                break;
            case 3:
                newColor = Color.green;
                break;
            case 4:
                newColor = Color.magenta;
                break;
            case 5:
                newColor = Color.red;
                break;
            case 6:
                newColor = Color.white;
                break;
            case 7:
                newColor = Color.yellow;
                break;
        }

        GetComponent<SpriteRenderer>().color = newColor;
        GetComponent<TrailRenderer>().material.color = newColor;
    }

    // Entered killzone
    public void Die()
    {
        Destroy(this.gameObject);
    }
}
