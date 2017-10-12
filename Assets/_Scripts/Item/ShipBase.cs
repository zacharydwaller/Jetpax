using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ShipBase : NetworkBehaviour
{
    // Class Constants ////////////////////////////////////////////////////////

    private const int COMPONENTS_UNTIL_FUEL = 2;
    private const int FUEL_UNTIL_COMPLETE = 3;
    private const int SCORE_FOR_COMPLETION = 750;

    // Class Variables ////////////////////////////////////////////////////////

    // Game
    private GameManager gameManager;

    // Player
    public PlayerController owner;

    // Build
    public int componentsBuilt = 0;
    public int componentsFueled = 0;
    public ArrayList shipComponents;

    // Color
    public Color activeColor; // For in-progress building and for fuelled sections
    public Color inactiveColor; // For unfuelled sections

    // Class Methods //////////////////////////////////////////////////////////

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        shipComponents = new ArrayList();
        shipComponents.Add(this.gameObject);
    }

    public void BuildComponent(GameObject newComponent)
    {
        componentsBuilt++;
        shipComponents.Add(newComponent);

        if (componentsBuilt == COMPONENTS_UNTIL_FUEL)
        {
            gameManager.CmdStartSpawningFuel();
            DimComponents();
        }
    }

    public void FuelComponent(GameObject newFuel)
    {
        ((GameObject) shipComponents[componentsFueled]).SendMessage("SetColor", activeColor);

        componentsFueled++;

        if (componentsFueled == FUEL_UNTIL_COMPLETE)
        {
            // Ship fly away, add score
            gameManager.CmdStopSpawningFuel();
            owner.CmdAddScore(SCORE_FOR_COMPLETION);
            FlyAway();

            // Re-populate owner's item queue
            owner.PopulateItemQueue();
        }
    }

    void DimComponents()
    {
        foreach(GameObject component in shipComponents)
        {
            component.SendMessage("SetColor", inactiveColor);
        }
    }

    public void SetColor(Color newColor)
    {
        GetComponent<SpriteRenderer>().color = newColor;
    }

    void FlyAway()
    {
        gameManager.CmdCreateShip(this.gameObject);
        GetComponent<Animator>().SetTrigger("takeOff");
        Destroy(this.gameObject, 2f);
    }
}
