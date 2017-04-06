using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Item_Base : NetworkBehaviour
{
    // Class Constants ////////////////////////////////////////////////////////

    // Class Variables ////////////////////////////////////////////////////////

    public string initialTag;

    public int playerNumber;
    [SyncVar]
    public bool beenPickedUp;
    [SyncVar]
    public bool beenDropped;
    public bool isShipBottom;
    public GameObject shipBase;

    public int scoreReward = 250;

    // Class Methods //////////////////////////////////////////////////////////

    void Start()
    {
        initialTag = tag;

        if (isShipBottom)
        {
            shipBase = this.gameObject;
        }
    }

    void Pickup(GameObject newParent)
    {
        if (!isServer) return;

        Player_Controller parent = newParent.GetComponent<Player_Controller>();
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<BoxCollider2D>().isTrigger = true;
        transform.parent = newParent.transform;
        transform.localPosition = new Vector3(0.5f, 0f, 0f);
        transform.localScale = Vector3.one / 2f;
        transform.rotation = Quaternion.identity;
        
        SetColor(parent.activeColor);
        playerNumber = parent.playerNumber;
        tag = "HeldItem";

        beenPickedUp = true;
    }

    // Drop - Drops an object into DropZone
    void Drop(GameObject dropZone)
    {
        if (!isServer) return;

        Rigidbody2D rBody = GetComponent<Rigidbody2D>();

        transform.parent.SendMessage("CmdAddScore", scoreReward);
        
        transform.parent = dropZone.transform.parent;
        transform.localScale = Vector3.one;

        /* If the thing is below its parent, make it not above. Otherwise leave its
           vertical position and snap its horizontal position to local 0 */
        if (transform.position.y < transform.parent.position.y)
        {
            transform.localPosition = new Vector3(0f, 1.1f);
        }
        else
        {
            transform.localPosition = new Vector3(0f, transform.localPosition.y);
        }

        rBody.isKinematic = false;
        rBody.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionX;
        beenDropped = true;
    }

    // Fumble - Drops an object into the air
    void Fumble()
    {
        if (!isServer) return;

        GetComponent<Rigidbody2D>().isKinematic = false;
        GetComponent<BoxCollider2D>().isTrigger = false;
        transform.localScale = Vector3.one;
        transform.parent = null;

        SetColor(Color.white);
        tag = initialTag;
        playerNumber = 0;
        beenDropped = beenPickedUp = false;
    }

    public void SetColor(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
    }

    void Die()
    {
        Destroy(this.gameObject);
    }
}
