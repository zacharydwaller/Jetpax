using UnityEngine;
using System.Collections;

public class Killzone : MonoBehaviour
{
    // Class Constants ////////////////////////////////////////////////////////

    // Class Variables ////////////////////////////////////////////////////////

    // Class Methods //////////////////////////////////////////////////////////

    // On trigger enter
    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("Player"))
        {
            other.SendMessage("Die", true); // true meaning ignores invulnerable state
        }
        else if (other.CompareTag("DropZone"))
        {
            // Ignore
        }
        else
        {
            other.SendMessage("Die");
        }
    }
}
