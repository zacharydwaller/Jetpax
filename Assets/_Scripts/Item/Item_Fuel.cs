using UnityEngine;
using System.Collections;

public class Item_Fuel : Item_Base
{
    void HitShipBottom(GameObject shipBottom)
    {
        if (
            !isShipBottom && beenDropped
            && shipBottom.GetComponent<Item_Base>().playerNumber == this.playerNumber
          )
        {
            shipBottom.SendMessageUpwards("FuelComponent", this.gameObject);
            Destroy(this.gameObject);
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
