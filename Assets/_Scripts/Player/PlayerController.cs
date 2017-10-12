using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    // Class Constants ////////////////////////////////////////////////////////

    // Movement
    private const float THRUST = 4.0f;
    private const float GRAVITY = -4.0f;
    private const float LATERAL_MOVESPEED = 5.0f;

    private const float LEFT_SCALAR = 1.0f;
    private const float RIGHT_SCALAR = -1.0f;

    // Firing
    private const float FIRE_DELAY = 0.05f;
    private const float BULLET_RECHARGE_DELAY = 1f;
    private const int MAX_BULLETS = 5;

    // Health and Lives
    private const int BASE_EXTRA_LIVES = 9999;
    private const float RESPAWN_DURATION = 2f;
    private const float INVINCIBLE_DURATION = 1f;
    private const int MAX_HEALTH = 10;

    // Score
    private const float SURVIVE_SCORE_DELAY = 3f;
    private const int SURVIVE_SCORE_AMOUNT = 25;

    // Resources
    private const string OBJ_BULLET = "Objects/Player/Obj_Bullet";

    private const string DAMAGE_SOUND = "Sounds/Sound_PlayerDamage";
    private const string DEATH_SOUND = "Sounds/Sound_PlayerDeath";
    private const string SCORE_BOX = "Objects/Player/TextBox_PlayerScore";
    private const string SHOOT_SOUND = "Sounds/Sound_Shoot";

    private const string OBJ_SHIP = "Objects/Item/Obj_ShipBottom";
    private const string OBJ_SHIP_MID = "Objects/Item/Obj_ShipMiddle";
    private const string OBJ_SHIP_TOP = "Objects/Item/Obj_ShipTop";
    private const string OBJ_FUEL = "Objects/Item/Obj_Fuel";

    // Class Variables ////////////////////////////////////////////////////////

    // Game Manager
    private GameManager gameManager;

    // Firing
    private GameObject bulletRef;
    [SyncVar]
    private bool facingLeft;
    private float nextFire;
    private int numBullets;
    private float nextBulletRecharge;

    // Items
    private Queue itemQ; // Holds strings of tags of needed items in order, populated by gameManager
    [SyncVar]
    private bool isHoldingItem;
    [SyncVar]
    private GameObject heldItem;
    
    // Multiplayer
    [SyncVar]
    public int playerNumber;
    public int playerHealth;

    [SyncVar]
    public GameObject ownedShip;

    // Score
    private Text scoreText;
    [SyncVar]
    public int playerScore;
    private float nextSurvScoreTick;

    // Animation
    public bool isTouchingGround;
    private enum AnimState{idle, walking, flying}

    // Respawning
    private NetworkStartPosition[] spawnPoints;
    public bool isRespawning = true;
    [SyncVar]
    public bool isInvincible;
    public bool isDead;
    private float respawnEndTime;
    private float invincibleEndTime;

    public int extraLives;

    // Audio/Visual
    public Color activeColor; // Used for general play
    public Color inactiveColor; // Used for respawn and invincible states

    private AudioClip damageSound;
    private AudioClip deathSound;
    private AudioClip shootSound;

    // Components
    new private Rigidbody2D rigidbody;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Animator animator;

    // Class Methods //////////////////////////////////////////////////////////

    // Start and Update

    // Start
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        Initialize();
    }

    // Update
    void FixedUpdate()
    {
        checkState();
        performInput();
        rechargeBullets();
    }

    // Checks to see whether a state should change or not
    void checkState()
    {
        //if (!isLocalPlayer) return;

        if (isDead)
        {
            return;
        }

        if (isRespawning)
        {
            if (Time.time >= respawnEndTime)
            {
                endRespawnState();
            }

        }
        else
        {
            if (Time.time >= nextSurvScoreTick)
            {
                CmdAddScore(SURVIVE_SCORE_AMOUNT);
                nextSurvScoreTick = Time.time + SURVIVE_SCORE_DELAY;
            }
        }

        if (isInvincible && !isRespawning)
        {
            if (Time.time >= invincibleEndTime)
            {
                endInvincibleState();
            }
        }


        if (playerHealth <= 0)
        {
            Die();
        }
    }

    // Input Methods ////////////////////////////////////////////////

    // Performs any input
    void performInput()
    {
        if (!isLocalPlayer) return;

        // TODO: Make escape not hard-coded
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameManager.CmdRemovePlayer(this.gameObject);
            SceneManager.LoadScene(0);
            return;
        }
        if (isRespawning || isDead)
        {
            return; // If respawning don't accept input (except for escape of course)
        }

        Vector2 finalMovement = Vector2.zero;
        float horzAxis = Input.GetAxis("Horizontal");
        float vertAxis = Input.GetAxis("Vertical");

        finalMovement.x = horzAxis * LATERAL_MOVESPEED;
        if (vertAxis > 0)
        {
            finalMovement.y = vertAxis * THRUST;
        }
        else
        {
            finalMovement.y = GRAVITY;
        }

        rigidbody.velocity
            = new Vector2(finalMovement.x, finalMovement.y);

        if (Input.GetAxis("Fire") > 0)
        {
            CmdFireWeapon();
        }

        determineAnimation(finalMovement);
    }

    // Fires a projectile
    [Command]
    void CmdFireWeapon()
    {
        // If firing first bullet in a charge, set next recharge time
        if (numBullets == MAX_BULLETS)
        {
            nextBulletRecharge = Time.time + BULLET_RECHARGE_DELAY;
        }

        if (numBullets > 0 && Time.time > nextFire)
        {
            GameObject bulletObj
                = (GameObject) GameObject.Instantiate(bulletRef, transform.position, Quaternion.identity);
            Bullet bullet = bulletObj.GetComponent<Bullet>();

            bullet.owner = this;
            bullet.damage = Bullet.BASE_DAMAGE;
            
            if (facingLeft)
            {
                bulletObj.GetComponent<Bullet>().isFacingLeft = true;
                bulletObj.GetComponent<Rigidbody2D>().velocity
                    = new Vector2(Bullet.FLIGHTSPEED, 0) * Bullet.LEFT_SCALAR;
            }
            else
            {
                bulletObj.GetComponent<Bullet>().isFacingLeft = false;
                bulletObj.GetComponent<Rigidbody2D>().velocity
                    = new Vector2(Bullet.FLIGHTSPEED, 0) * Bullet.RIGHT_SCALAR;
            }

            NetworkServer.Spawn(bulletObj);

            audioSource.PlayOneShot(shootSound, 0.5f);
            nextFire = Time.time + FIRE_DELAY;
            numBullets--;
        }
    }

    // Recharges bullets on an interval
    void rechargeBullets()
    {
        if (!isLocalPlayer) return;

        if (numBullets < MAX_BULLETS && Time.time >= nextBulletRecharge)
        {
            numBullets = MAX_BULLETS;
        }
    }

    // Item Methods ///////////////////////////////////////////////////////////

    // Populates the item queue
    public void PopulateItemQueue()
    {
        if (!isLocalPlayer) return;

        if(itemQ != null)
        {
            itemQ.Clear();
        }
        else
        {
            itemQ = new Queue();
        }

        itemQ.Enqueue("ShipMiddle");
        itemQ.Enqueue("ShipTop");
        for (int i = 0; i < 3; i++) itemQ.Enqueue("Fuel");
    }

    // Checks if item collided with is able to be picked up
    void CheckPickup(Collider2D other)
    {
        if (!isLocalPlayer) return;

        if (!isHoldingItem && other.tag == (string) itemQ.Peek())
        {
            itemQ.Dequeue();
            heldItem = other.gameObject;
            isHoldingItem = true;
            heldItem.SendMessage("Pickup", this.gameObject);
        }
    }

    // Checks if player is carrying an item in a drop zone
    void CheckDrop(Collider2D other)
    {
        if (!isLocalPlayer) return;

        if (
            other.transform.parent != null && isHoldingItem && other.tag == "DropZone"
            && other.GetComponentInParent<ItemBase>().playerNumber == playerNumber
          )
        {
            heldItem.SendMessage("Drop", other.gameObject);
            heldItem = null;
            isHoldingItem = false;
        }
    }

    // If player is holding an item and dies, fumble the item
    public void TryFumbleItem()
    {
        if (!isLocalPlayer) return;

        if (isHoldingItem)
        {
            // Push needed item back onto item queue
            itemQ = PushQueue(heldItem.GetComponent<ItemBase>().initialTag, itemQ);
            heldItem.SendMessage("Fumble");
            heldItem = null;
            isHoldingItem = false;
        }
    }

    // A stack-like push onto a Queue
    // Returns queue with pushed on item
    Queue PushQueue(object newObject, Queue paramQueue)
    {
        Queue tempQueue = paramQueue;
        Queue retQueue = new Queue();

        retQueue.Enqueue(newObject);
        while (tempQueue.Count > 0)
        {
            retQueue.Enqueue(tempQueue.Dequeue());
        }

        return retQueue;
    }

    // Respawn Methods ////////////////////////////////////////////////////////

    // State where player cannot move and does not take damage from enemies
    void beginRespawnState()
    {
        if (!isLocalPlayer) return;

        isRespawning = true;
        isInvincible = true;
        isDead = false;

        respawnEndTime = Time.time + RESPAWN_DURATION;

        playerHealth = MAX_HEALTH;

        spriteRenderer.color = inactiveColor;
        updateAnimation("Respawning");


        Rigidbody2D rgb = GetComponent<Rigidbody2D>();
        rgb.velocity = Vector2.zero;
        rgb.isKinematic = true;

        transform.position =
            spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
    }

    // Ends respawn state, transitions to invincible state
    void endRespawnState()
    {
        if (!isLocalPlayer) return;

        isRespawning = false;
        nextSurvScoreTick = Time.time + SURVIVE_SCORE_DELAY;

        Rigidbody2D rgb = GetComponent<Rigidbody2D>();
        rgb.isKinematic = false;

        updateAnimation("Respawned");
        beginInvincibleState();
    }

    // State where player can move but does not take damage from enemies
    void beginInvincibleState()
    {
        if (!isLocalPlayer) return;

        isInvincible = true;
        invincibleEndTime = Time.time + INVINCIBLE_DURATION;
        spriteRenderer.color = inactiveColor;
    }

    // Ends invincible state, transitions to regular state
    void endInvincibleState()
    {
        if (!isLocalPlayer) return;

        isInvincible = false;
        spriteRenderer.color = activeColor;
    }

    void beginGameOverState()
    {
        isDead = true;
        transform.localScale = Vector3.zero;
        Destroy(this.gameObject, 1f);
    }

    // Death/Damage Methods ////////////////////////////////////////////////

    // Take Damage
    void TakeDamage(int damage)
    {
        if (!isServer) return;

        if (!isInvincible)
        {
            audioSource.PlayOneShot(damageSound);
            playerHealth -= damage;
        }
    }

    // Kill Player if not invincible
    void Die()
    {
        if (!isServer) return;

        Die(false);
    }

    // Die overload
    // Param: true overrides invincibility state
    void Die(bool immediate)
    {
        if (!isServer) return;

        if (immediate == true || isInvincible == false)
        {
            TryFumbleItem();
            audioSource.PlayOneShot(deathSound);

            // TODO: Play destroy animation, do below code after animation

            extraLives--;
            if (extraLives <= 0)
            {
                beginGameOverState();
            }
            else
            {
                //transform.position = respawnPoint.transform.position;
                beginRespawnState();
            }
        }

    }

    // Score and UI Methods //////////////////////////////////////////////////////////

    // Add to score
    [Command]
    public void CmdAddScore(int amount)
    {
        playerScore += amount;
        //scoreText.text = "Score: " + playerScore;
    }

    // Place ScoreBox
    // TODO: Make dimensions and positioning relative to size of window
    private void PlaceScoreBox()
    {
        if (!isLocalPlayer) return;

        GameObject canvas = GameObject.FindObjectOfType<Canvas>().gameObject;
        GameObject scoreBoxTmp = (GameObject) Object.Instantiate(Resources.Load(SCORE_BOX));
        RectTransform rectTransf = scoreBoxTmp.GetComponent<RectTransform>();
        Vector3 boxPosition = new Vector3();
        float boxOffset = 400; // Maybe make this related to size of window

        boxPosition.x = (playerNumber * boxOffset) - rectTransf.rect.width / 2;
        boxPosition.y = -25; // Make this related to size of window

        scoreBoxTmp.transform.SetParent(canvas.transform);
        rectTransf.anchoredPosition = boxPosition;
        scoreText = scoreBoxTmp.GetComponent<Text>();
    }

    // Collision Methods //////////////////////////////////////////////////////

    // Enter collision
    void OnCollisionEnter2D(Collision2D coll)
    {
        CheckPickup(coll.collider);

        if (coll.collider.CompareTag("Platform"))
        {
            isTouchingGround = true;
        }
    }

    // Enter trigger
    void OnTriggerEnter2D(Collider2D other)
    {
        CheckDrop(other);
    }

    // Exit collision
    void OnCollisionExit2D(Collision2D coll)
    {
        if (coll.collider.CompareTag("Platform"))
        {
            isTouchingGround = false;
        }
    }

    // Animation Methods //////////////////////////////////////////////////////

    // Determines animation mode and updates animator
    void determineAnimation(Vector2 finalMovement)
    {
        // Moving
        if (isTouchingGround)
        {
            if (finalMovement.x == 0)
            {
                updateAnimation("Idle");
            }
            else
            {
                updateAnimation("Walking");
            }
        }
        else
        {
            updateAnimation("Flying");
        }

        // LEFT
        if (finalMovement.x < 0)
        {
            transform.localScale = new Vector3(LEFT_SCALAR, 1);
            facingLeft = true;
        }
        // RIGHT
        else if (finalMovement.x > 0)
        {
            transform.localScale = new Vector3(RIGHT_SCALAR, 1);
            facingLeft = false;
        }
    }

    // Updates animator to given parameter
    public void updateAnimation(string newAnimState)
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (newAnimState == "Idle")
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isFlying", false);
        }
        else if (newAnimState == "Walking")
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isFlying", false);
        }
        else if (newAnimState == "Flying")
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isFlying", true);
        }
        else if (newAnimState == "Respawning")
        {
            animator.SetTrigger("hasDied");
        }
        else if (newAnimState == "Respawned")
        {
            animator.SetTrigger("hasRespawned");
        }
    }

    // Initialization /////////////////////////////////////////////////////////

    public void Initialize()
    {
        if (!isLocalPlayer) return;

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        // Player Number and Spawn
        spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        gameManager.CmdAddPlayer(this.gameObject);
        
        playerNumber = gameManager.numPlayers;
        name = "Player " + playerNumber;

        // Color
        activeColor = Color.white;
        inactiveColor = activeColor;
        inactiveColor.a = 0.5f;

        // Spawn Ship
        gameManager.CmdCreateShip(this.gameObject);

        // Items - Middle, Top, 3 Fuels
        PopulateItemQueue();
        
        // Score Box
        PlaceScoreBox();
        scoreText.color = activeColor;
        playerScore = 0;

        // Lives
        extraLives = BASE_EXTRA_LIVES;

        // Get Components
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody2D>();

        // Load Resources
        damageSound = Resources.Load<AudioClip>(DAMAGE_SOUND);
        deathSound = Resources.Load<AudioClip>(DEATH_SOUND);
        bulletRef = (GameObject) Resources.Load(OBJ_BULLET);
        shootSound = (AudioClip) Resources.Load(SHOOT_SOUND);
        
        // Initizalize variables
        nextFire = Time.time;
        numBullets = MAX_BULLETS;
        nextBulletRecharge = Time.time;

        // Spawn
        beginRespawnState();
    }
}
