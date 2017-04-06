using UnityEngine;
using System.Collections;

public class Item_ShipPart : Item_Base
{

    void HitShipBottom(GameObject shipBottom)
    {
        if (
            !isShipBottom && beenDropped
            && shipBottom.GetComponent<Item_Base>().playerNumber == this.playerNumber
          )
        {
            shipBottom.tag = "OldShipBottom";
            tag = "ShipBottom";
            isShipBottom = true;
            shipBottom.transform.FindChild("DropZone").gameObject.SetActive(false);
            transform.FindChild("DropZone").gameObject.SetActive(true);
            transform.localPosition = new Vector3(0f, 1f);
            GetComponent<Rigidbody2D>().isKinematic = true;

            shipBase = shipBottom.GetComponent<Item_Base>().shipBase;
            shipBase.SendMessage("BuildComponent", this.gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "ShipBottom" && beenDropped)
        {
            HitShipBottom(other.gameObject);
        }
    }
}
