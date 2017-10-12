using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SpawnArea : NetworkBehaviour
{
    // Class Constants ////////////////////////////////////////////////////////

    // Class Variables ////////////////////////////////////////////////////////

    // Spawned Object Info
    public GameObject[] objectReference;
    public Type objectType;
    private float spawnDelay;
    public float nextSpawn;
    public bool faceLeft;
    private int currentIndex;

    // Spawn Delay Info
    public bool isSpawning = false;
    public float baseSpawnDelay = 2f;  // Default values for enemy spawners
    public float spawnDelayDecrease = 0.01f; // How much spawn delay decreases per second
    public float minSpawnDelay = 0.40f; // Will take a little over 2.5 minutes to reach

    // Components
    private Collider2D area;

    // Type Enum
    public enum Type
    {
        Enemy, Item
    }

    // Class Methods //////////////////////////////////////////////////////////

    // Start
    public override void OnStartServer()
    {
        area = GetComponent<Collider2D>();

        spawnDelay = baseSpawnDelay;
        nextSpawn = Mathf.Infinity;
        currentIndex = Random.Range(0, objectReference.Length);
    }

    // Update
    void Update()
    {
        SpawnObject();

        spawnDelay = Mathf.Clamp(spawnDelay - (spawnDelayDecrease * Time.deltaTime), minSpawnDelay, Mathf.Infinity);
    }

    public void StartSpawning()
    {
        isSpawning = true;
        spawnDelay = baseSpawnDelay;
        nextSpawn = Time.time + spawnDelay;
    }

    public void StopSpawning()
    {
        isSpawning = false;
        nextSpawn = Mathf.Infinity;
    }

    // Spawn random objects on an interval
    void SpawnObject()
    {
        if (!isServer) return;
        if (!isSpawning) return;

        Vector2 spawnPoint;
        Bounds bounds = area.bounds;
        GameObject spawnedObject;

        if (Time.time >= nextSpawn)
        {
            currentIndex = Random.Range(0, objectReference.Length);
            spawnPoint.x = Random.Range(bounds.min.x, bounds.max.x);
            spawnPoint.y = Random.Range(bounds.min.y, bounds.max.y);

            spawnedObject = (GameObject) GameObject.Instantiate(objectReference[currentIndex], (Vector3) spawnPoint, Quaternion.identity);

            if (objectType == Type.Enemy)
            {
                spawnedObject.GetComponent<EnemyBase>().isFacingLeft = faceLeft;
            }

            NetworkServer.Spawn(spawnedObject);

            nextSpawn = Time.time + spawnDelay;
        }
    }

    // Adds an item inbetween every existing item
    public void FoldItem(GameObject newItem)
    {
        int index, bigIndex;
        GameObject[] tmpArray = objectReference;
        objectReference = new GameObject[tmpArray.Length * 2];

        for (index = bigIndex = 0; index < tmpArray.Length; index++)
        {
            objectReference[bigIndex] = tmpArray[index];
            objectReference[bigIndex + 1] = newItem;

            bigIndex += 2;
        }

    }

    // Adds an item to spawn list
    public void AddItem(GameObject newItem)
    {
        int index;
        GameObject[] tmpArray = objectReference;
        objectReference = null;
        objectReference = new GameObject[tmpArray.Length + 1];

        for (index = 0; index < tmpArray.Length; index++)
        {
            objectReference[index] = tmpArray[index];
        }
        objectReference[index] = newItem;
    }

    // Replaces object spawn list
    public void ReplaceSpawnRefs(GameObject[] newReference)
    {
        objectReference = newReference;
    }
}
