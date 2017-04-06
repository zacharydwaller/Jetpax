using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Enemy_Base : NetworkBehaviour
{
    // Class Constants ////////////////////////////////////////////////////////

    private const float INACTIVE_TIME = 1.0f;
    private const float DEATH_TIME = 0.5f;

    // Class Variables ////////////////////////////////////////////////////////

    // Self
    private float activationTime;
    public bool isActive;
    public bool isFacingLeft;
    public bool isDying;

    public int scoreReward = 100;

    // Animation
    public float currentTime = 0;

    // Damage
    public int damage;

    // Components
    private Collider2D _collider;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Class Methods //////////////////////////////////////////////////////////

    // Start
    void Start()
    {
        Initialize();
    }

    // Update
    void Update()
    {
        if (isActive == false)
        {
            if (Time.time >= activationTime)
            {
                activate();
            }
        }
    }

    void activate()
    {
        isActive = true;
        _collider.enabled = true;
        animator.SetBool("isActive", true);
        colorizeRandom();
    }

    // Paint the sprite a random color
    void colorizeRandom()
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

        spriteRenderer.color = newColor;
    }

    // Convoluted way to make enemy face the right way
    void fixRotation()
    {
        if (isFacingLeft)
        {
            transform.localScale = new Vector3(-1, 1);
        }
        else
        {
            transform.localScale = new Vector3(1, 1);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Collider2D coll = collision.collider;
        string otherTag = collision.collider.tag;

        if (otherTag == "Player")
        {
            coll.SendMessage("TakeDamage", damage);
            Die();
        }
        else if(otherTag == "Bullet")
        {
            // Die() called by bullet
        }
        else
        {
            CollisionDeath(coll);
        }
    }

    // Killed by hitting wall or player
    // Dies after a short period
    void CollisionDeath(Collider2D coll)
    {
        if (isDying) Destroy(this.gameObject);

        Rigidbody2D rgb = GetComponent<Rigidbody2D>();

        rgb.gravityScale = 1.0f;
        //rgb.AddForce((transform.position - coll.transform.position) * 100);
        isDying = true;
    }

    // Entered killzone or died in some other way
    // Kills instantly
    void Die()
    {
        Destroy(this.gameObject);
    }

    // Initialization
    void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();

        _collider.enabled = false;

        activationTime = Time.time + INACTIVE_TIME;
        isActive = false;
        isDying = false;
        spriteRenderer.color = new Vector4(1f, 1f, 1f, 0.5f); // Transluscent white

        fixRotation();
    }
}
