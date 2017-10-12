using UnityEngine;
using System.Collections;

public class ItemShipPart : ItemBase
{

    void HitShipBottom(GameObject shipBottom)
    {
        if (
            !isShipBottom && beenDropped
            && shipBottom.GetComponent<ItemBase>().playerNumber == this.playerNumber
          )
        {
            shipBottom.tag = "OldShipBottom";
            tag = "ShipBottom";
            isShipBottom = true;
            shipBottom.transform.Find("DropZone").gameObject.SetActive(false);
            transform.Find("DropZone").gameObject.SetActive(true);
            transform.localPosition = new Vector3(0f, 1f);
            GetComponent<Rigidbody2D>().isKinematic = true;

            shipBase = shipBottom.GetComponent<ItemBase>().shipBase;
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
