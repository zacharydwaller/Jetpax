﻿using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    // Class Constants ////////////////////////////////////////////////////////

    private const string OBJ_SHIP = "Objects/Item/Obj_ShipBottom";
    private const string OBJ_SHIP_MID = "Objects/Item/Obj_ShipMiddle";
    private const string OBJ_SHIP_TOP = "Objects/Item/Obj_ShipTop";
    private const string OBJ_FUEL = "Objects/Item/Obj_Fuel";

    private const string LEVEL_NAME = "Level1";
    private const string MAIN_MENU = "MainMenu";

    private const int TOTAL_ITEMS = 6;

    // Class Variables ////////////////////////////////////////////////////////

    // Game Management
    public bool gameStarted = false;

    // Players
    public ArrayList players;
    public int numPlayers = 0;
    public bool isCompetitiveGame = true;

    private GameObject shipReference;
    private GameObject fuelReference;
    private GameObject[] respawnPoints;
    private GameObject[] shipSpawnPoints;

    // Spawning
    public GameObject itemSpawner;
    public GameObject[] enemySpawners;

    public int shipsBuilt;

    // Class Methods //////////////////////////////////////////////////////////

    // Set to not destroy on load
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    void Start()
    {
        players = new ArrayList();

        shipReference = Resources.Load<GameObject>(OBJ_SHIP);
        fuelReference = Resources.Load<GameObject>(OBJ_FUEL);
    }

    private void Update()
    {
        if (!isServer) return;

        // TODO: Fix this
        if (NetworkServer.active == false)
        {
            itemSpawner.SendMessage("StopSpawning");
            enemySpawners[0].SendMessage("StopSpawning");
            enemySpawners[1].SendMessage("StopSpawning");
        }
    }

    // Start Game
    [Server]
    public void StartGame()
    {
        if(!isServer) return;

        gameStarted = true;

        itemSpawner = GameObject.FindGameObjectWithTag("ItemSpawner");
        enemySpawners = GameObject.FindGameObjectsWithTag("EnemySpawner");
        shipSpawnPoints = GameObject.FindGameObjectsWithTag("ShipSpawn");

        itemSpawner.SendMessage("StartSpawning");
        enemySpawners[0].SendMessage("StartSpawning");
        enemySpawners[1].SendMessage("StartSpawning");
    }

    [Command]
    public void CmdAddPlayer(GameObject player)
    {
        if(numPlayers == 0)
        {
            StartGame();
        }

        numPlayers++;
        players.Add(player);
    }

    [Command]
    public void CmdRemovePlayer(GameObject player)
    {
        players.Remove(player);
        numPlayers--;

        if (numPlayers == 0)
        {
            StopGame();
        }
    }

    [Server]
    public void StopGame()
    {
        if (!isServer) return;

        gameStarted = false;
        NetworkManager.Shutdown();
    }

    [Command]
    public void CmdCreateShip(GameObject ownerObj)
    {
        PlayerController owner = ownerObj.GetComponent<PlayerController>();
        GameObject newShip = GameObject.Instantiate(shipReference);
        
        ShipBase shipBase = newShip.GetComponent<ShipBase>();
        ItemBase itemBase = newShip.GetComponent<ItemBase>();

        // Player
        newShip.transform.position = shipSpawnPoints[owner.playerNumber - 1].transform.position;
        newShip.name = "Player" + owner.playerNumber + " Ship";
        shipBase.owner = owner;

        // Color
        itemBase.playerNumber = owner.playerNumber;
        shipBase.activeColor = owner.activeColor;
        shipBase.inactiveColor = shipBase.activeColor;
        shipBase.inactiveColor.a = 0.5f;
        shipBase.SetColor(shipBase.activeColor);

        NetworkServer.Spawn(newShip);

        owner.ownedShip = newShip;
    }

    [Command]
    public void CmdStartSpawningFuel()
    {
        shipsBuilt++;

        // If all players built their ships, only spawn fuel
        if (shipsBuilt == numPlayers)
        {
            GameObject[] newRefs = new GameObject[1];
            newRefs[0] = fuelReference;
            itemSpawner.SendMessage("ReplaceSpawnRefs", newRefs);
        }
        // Otherwise spawn ship components AND fuel
        else
        {
            // Add 2 fuel references
            for (int i = 0; i < 2; i++)
            {
                itemSpawner.SendMessage("AddItem", fuelReference);
            }
        }
    }

    [Command]
    public void CmdStopSpawningFuel()
    {
        shipsBuilt--;

        GameObject[] newItems = new GameObject[2];
        newItems[0] = Resources.Load<GameObject>(OBJ_SHIP_MID);
        newItems[1] = Resources.Load<GameObject>(OBJ_SHIP_TOP);

        // If all players fueled their ships, only spawn components
        if (shipsBuilt == 0)
        {
            itemSpawner.SendMessage("ReplaceSpawnRefs", newItems);
        }
        // Otherwise add components to mix
        else
        {
            itemSpawner.SendMessage("AddItem", newItems[0]);
            itemSpawner.SendMessage("AddItem", newItems[1]);
        }
    }
}
