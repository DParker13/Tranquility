using System.Collections.Generic;
using UnityEngine;
using LibNoise.Generator;
using System.IO;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    [Header("Player Info")]
    public GameObject player;

    [Header("World Name")]
    public string worldName;

    [Header("Chunk Info")]
    public GameObject chunkEmptyObject;
    public int chunkSize = 10;
    public int renderDistance = 2;
    public int deleteDistance = 3;

    [Header("Map Options")]
    public bool saveMode = true;

    [Header("Perlin Noise Options")]
    public Perlin perlin = new Perlin();
    public int octaves = 3;
    public int frequency = 2;
    public float persistance = 0.5f;
    public float scale = 0.01f;
    public int seed = 102938;

    [Header("Biome Generation Options")]
    public Perlin biomePerlin = new Perlin();
    public int biomeSeed = 129038;

    [Header("Tree Perlin Noise Options")]
    public Perlin treePerlin = new Perlin();
    public int treeOctaves = 6;
    public int treeFrequency = 4;
    public float treePersistance = 1;

    [Header("Map Objects")]
    public float objectBuffer = 0.01f;
    public float bushLevel = 0.95f;
    public float treeLevel = 0.85f;
    public float grassTileLevel = 0.8f;
    public GameObject[] objects;

    [Header("Map Layers")]
    public float landBuffer = 0.1f;
    public float waterLevel = 0.25f;
    public float sandLevel = 0.5f;
    public float dirtLevel = 0.6f;
    public float grassLevel = 0.7f;
    public float darkGrassLevel = 0.8f;
    public float[] layerHeights;

    [Header("Tiles")]
    public GameObject tilePrefab;

    [Header("Map Sprites")]
    public Texture2D darkGrassTexture;
    public Texture2D grassTexture;
    public Texture2D dirtTexture;
    public Texture2D sandTexture;
    public Sprite waterTexture;

    [Header("Chunks")]
    
    public int numPooledChunks = 50;
    public Queue<Chunk> pooledChunks;
    public Queue<Chunk> chunksAwaitingActivation;
    public Queue<Chunk> chunksAwaitingDeactivation;
    public Dictionary<Vector2, Chunk> allChunks;

    [Header("Player Info")]
    public Vector2Int playerPos;

    //Thread Crap
    public ThreadStart chunkThreadStart;
    public Thread chunkThread;
    public bool threadExecuted = true;

    public Queue<SaveChunk> loadChunkQueue;

    //Private Variables
    private string persistanceDataPath;

    void Awake() {
        //Gets the scene controller gameobject
        var sceneController = GameObject.FindGameObjectWithTag("Scene Controller");

        //gets the world name from the scene controller
        worldName = sceneController.GetComponent<SceneController>().worldName;

        SetWorldInfo();

        //Initiates all lists, queues, and dictionaries
        pooledChunks = new Queue<Chunk>();
        allChunks = new Dictionary<Vector2, Chunk>();

        //Thread Queues
        chunksAwaitingActivation = new Queue<Chunk>();
        chunksAwaitingDeactivation = new Queue<Chunk>();
        loadChunkQueue = new Queue<SaveChunk>();

        layerHeights = new float[] { waterLevel, sandLevel, dirtLevel, grassLevel, darkGrassLevel };

        //Loads the chunk pool with empty chunks
        CreatePooledChunks();
    }

    public void SetWorldInfo()
    {
        //Generates a random seed for the world
        seed = RandomSeed();
        biomeSeed = RandomSeed();

        persistanceDataPath = Application.persistentDataPath;

        //Attempts to load previous world data from file
        if (!worldName.Equals("Main") && saveMode == true)
        {
            SaveWorld savedWorld = SaveLoadData.LoadWorld(worldName);

            //Runs if no world was previously saved
            if (savedWorld == null)
            {
                //Saves the world information to file
                SaveLoadData.SaveWorld(seed, octaves, frequency, persistance, scale, worldName);
            }
            else
            {
                worldName = savedWorld.worldName;
                seed = savedWorld.seed;
                octaves = savedWorld.octaves;
                frequency = savedWorld.frequency;
                persistance = savedWorld.persistance;
                scale = savedWorld.scale;
            }
        }

        //--Sets the perlin generators--

        //World
        perlin.Seed = seed;
        perlin.OctaveCount = octaves;
        perlin.Frequency = frequency;
        perlin.Persistence = persistance;

        //Trees
        treePerlin.Seed = seed;
        treePerlin.OctaveCount = treeOctaves;
        treePerlin.Frequency = treeFrequency;
        treePerlin.Persistence = treePersistance;

        //Biomes (Heat Map)
        biomePerlin.Seed = biomeSeed;

        //Biomes (Precipitation)

    }

    void Update() {
        //Starts the map generation thread
        ChunkManagementThread();

        //Updates player position for thread use
        playerPos = GetPlayerPosition();

        //Queued chunks awaiting to be actived or deactivated
        if(chunksAwaitingActivation.Count > 0)
        {
            Chunk dequeuedChunk = chunksAwaitingActivation.Dequeue();

            dequeuedChunk.gameObject.name = "Chunk (" + dequeuedChunk.chunkPos.x + "," + dequeuedChunk.chunkPos.y + ")";
            dequeuedChunk.UpdateChunk();
            LoadChunkObjects(dequeuedChunk);
            dequeuedChunk.gameObject.SetActive(true);
        }

        if (chunksAwaitingDeactivation.Count > 0)
        {
            Chunk dequeuedChunk = chunksAwaitingDeactivation.Dequeue();

            dequeuedChunk.PoolAllTiles();
            dequeuedChunk.gameObject.SetActive(false);
            DestroyLoadedChunkObjects(dequeuedChunk);
            pooledChunks.Enqueue(dequeuedChunk);
        }
    }

    /*
     * Starts the chunk loading thread
     */
    public void ChunkManagementThread() {
        if(threadExecuted == true)
        {
            chunkThreadStart = new ThreadStart(LoadChunksAroundPlayer);
            chunkThread = new Thread(chunkThreadStart);
            threadExecuted = false;

            chunkThread.Start();
        }
    }

    /*
     * Updates the player position for thread use
     */
    public Vector2Int GetPlayerPosition() {
        return new Vector2Int((int)player.transform.position.x, (int)player.transform.position.y);
    }

    /*
     * Loads chunks around the player with an inputed render distance
     */
    public void LoadChunksAroundPlayer() {
        FileInfo[] savedChunks = null;

        //int posX = (int)player.transform.position.x;
        //int posY = (int)player.transform.position.y;

        int posX = playerPos.x;
        int posY = playerPos.y;

        //Sets the map generator to save the chunks or not
        if (saveMode == true)
        {
            if(Directory.Exists(persistanceDataPath + "/Worlds/" + worldName) && savedChunks == null)
            {
                //Gets all of the saved chunk files
                DirectoryInfo dir = new DirectoryInfo(persistanceDataPath + "/Worlds/" + worldName);
                savedChunks = dir.GetFiles("*.dat");
            }
            else if(!Directory.Exists(persistanceDataPath + "/Worlds/" + worldName))
            {
                //Creating Directory
                Directory.CreateDirectory(persistanceDataPath + "/Worlds/" + worldName);
            }
        }

        for (int y = -renderDistance*chunkSize; y < renderDistance*chunkSize; y+=chunkSize)
        {
            for (int x = -renderDistance*chunkSize; x < renderDistance*chunkSize; x+=chunkSize)
            {
                //Rounds the x and y coords of the player to the size of the chunk
                int newX = (int)((x + posX) / chunkSize) * chunkSize;
                int newY = (int)((y + posY) / chunkSize) * chunkSize;

                //If there are any saved chunks it will try to load them if the map is set to be saved
                if (savedChunks != null && savedChunks.Length != 0 && saveMode == true)
                {
                    bool chunkFound = false;
                    for (int i = 0; i < savedChunks.Length; i++)
                    {
                        //If the saved chunk name contains the x and y coordinates then load it from file
                        if (savedChunks[i].Name.Contains("(" + newX + "," + newY + ")") && chunkFound == false)
                        {
                            chunkFound = true;
                            //RequestChunkData(newX, newY);]
                            SaveLoadData saveLoadData = new SaveLoadData();
                            LoadChunkAt(saveLoadData.LoadChunk(persistanceDataPath, newX, newY, worldName));
                            break;
                        }
                        else if (i == savedChunks.Length-1 && chunkFound == false) //If the chunk was not found, create a new one
                        {
                            GenerateChunk(newX, newY);
                        }
                    }
                }
                else
                {
                    GenerateChunk(newX, newY);
                }
            }
        }
        //Disables chunks outside the render distance
        PoolChunks();

        threadExecuted = true;
    }

    /*
     * Creates all of the chunks that will be used during the game
     */
    private void CreatePooledChunks() {
        for (int i = 0; i < numPooledChunks; i++)
        {
            GameObject chunkGameObject = Instantiate(chunkEmptyObject, transform);
            pooledChunks.Enqueue(chunkGameObject.GetComponent<Chunk>());

            //sets them active to activate start methods
            chunkGameObject.SetActive(false);
        }
    }

    /*
     * Generates a new chunk at x and y coordinates
     */
    public void GenerateChunk(int x, int y) {
        //Checks if the chunk is already loaded or if the chunk is pooled
        if (!allChunks.ContainsKey(new Vector2(x, y)))
        {
            //Removes the first pooled chunk from the list
            Chunk currentChunk = pooledChunks.Dequeue();

            //Adds the chunk to the loaded chunks list
            allChunks.Add(new Vector2(x, y), currentChunk);

            currentChunk.SetPosition(x, y);
            currentChunk.GenerateLayers();
            chunksAwaitingActivation.Enqueue(currentChunk);

            if (saveMode == true)
            {
                SaveLoadData.SaveChunk(persistanceDataPath, x, y, (SaveChunk)currentChunk, worldName);
            }
        }
    }

    /*
     * Takes a loaded chunk and adds it back to the pool
     */
    public void PoolChunks() {
        //int newX = (int)(player.transform.position.x / chunkSize) * chunkSize;
        //int newY = (int)(player.transform.position.y / chunkSize) * chunkSize;

        int newX = (int)(playerPos.x / chunkSize) * chunkSize;
        int newY = (int)(playerPos.y / chunkSize) * chunkSize;

        Queue<Vector2> removeChunks = new Queue<Vector2>();

        //loops through currently active chunks
        foreach (Chunk chunk in allChunks.Values)
        {
            float distance = Vector2.Distance(new Vector2(newX, newY), new Vector2(chunk.chunkPos.x, chunk.chunkPos.y));

            if (distance > deleteDistance * chunkSize)
            {
                removeChunks.Enqueue(new Vector2(chunk.chunkPos.x, chunk.chunkPos.y));
            }
        }

        if(removeChunks.Count > 0)
        {
            while(removeChunks.Count != 0)
            {
                Vector2 key = removeChunks.Dequeue();

                chunksAwaitingDeactivation.Enqueue(allChunks[key]);
                allChunks.Remove(key);
            }
        }
    }

    /*
     * Loads a new chunk from file with tiles and objects at desired position
     */
    public void LoadChunkAt(Chunk savedChunk) {
        if(savedChunk != null) {
            if (savedChunk.IsEmpty() == false && !allChunks.ContainsKey(new Vector2(savedChunk.chunkPos.x, savedChunk.chunkPos.y))) {
                int x = savedChunk.chunkPos.x;
                int y = savedChunk.chunkPos.y;

                //Checks if the chunk is already loaded or if the chunk is pooled
                if (!allChunks.ContainsKey(new Vector2(x, y))) {
                    //Removes the first pooled chunk from the list
                    Chunk dequeuedChunk = pooledChunks.Dequeue();

                    //Adds the chunk to the loaded chunks list
                    allChunks.Add(new Vector2(x, y), dequeuedChunk);

                    dequeuedChunk.SetPosition(x, y);
                    LoadTiles(dequeuedChunk, savedChunk);
                }
            }
        }
    }

    /*
     * Loads saved tiles on the chunk
     */
    private void LoadTiles(Chunk inWorldChunk, Chunk savedChunk) {
        inWorldChunk.updateTiles = new List<Tile>();

        for (int i = 0; i < savedChunk.chunkLoadedTiles.Count; i++) {
            Tile currentTile = inWorldChunk.chunkPooledTiles.Dequeue();

            currentTile.SetTile(savedChunk.chunkLoadedTiles[i]);

            //adds the new tile to the loaded list
            inWorldChunk.chunkLoadedTiles.Add(currentTile);
            inWorldChunk.updateTiles.Add(currentTile);
        }
    }

    public void LoadChunkObjects(Chunk chunk) {
        while (chunk.objectsAwaitingActivation.Count > 0) {
            ObjectInfo OI = chunk.objectsAwaitingActivation.Dequeue();

            chunk.loadedObjects.Enqueue(OI);
            GameObject currentObject = Instantiate(objects[(int)OI.objectType], OI.objectPos, Quaternion.identity, chunk.gameObject.transform);

            currentObject.GetComponent<ObjectInfo>().SetObject(OI);
        }
    }

    public void DestroyLoadedChunkObjects(Chunk chunk) {
        while (chunk.loadedObjects.Count > 0) {
            ObjectInfo currentObject = chunk.loadedObjects.Dequeue();

            if (currentObject != null)
                Destroy(chunk.loadedObjects.Dequeue().gameObject);
        }
    }

    /*
     * Loads objects from file
     *
    private void LoadObjects(Chunk chunk) {
        int row = 0;
        int column = 0;

        for (int i = 0; i < chunk.chunkObjects.Count; i++) {
            ObjectInfo currentObject = new ObjectInfo();
            GameObject currentObjectGO = Instantiate(objects[(int)currentObject.objectType], currentObject.objectPos, Quaternion.identity, chunk.transform);

            currentObject.objectPos = chunk.chunkObjects.objectPos;
            currentObject.numHits = chunk.chunkObjects[i].numHits;
            currentObject.numHitsTillBreak = chunk.chunkObjects[i].numHitsTillBreak;
            currentObject.broken = chunk.chunkObjects[i].broken;
            currentObject.objectType = chunk.chunkObjects[i].objectType;
            currentObject.savedObject = chunk.chunkObjects[i].savedObject;

            //Increments to the next column when it reaches the end of the chunksize(row)
            if (row == chunkSize - 1) {
                column++;
                row = 0;
            }
            else {
                row++;
            }
        }
    }
    */
    /*
     * Sets the seed to a random integer and generates a new map
     */
    public int RandomSeed() {
        seed = Random.Range(0, int.MaxValue);

        return seed;
    }

    /*
     * Saves the visible chunks to file
     */
    public void SaveChunks(string worldName) {
        //Loops through each loaded chunk and saves it to file
        foreach (KeyValuePair<Vector2, Chunk> chunk in allChunks) {
            int x = (int)chunk.Key.x;
            int y = (int)chunk.Key.y;

            //Saves the current chunk
            SaveLoadData.SaveChunk(persistanceDataPath, x, y, (SaveChunk)chunk.Value, worldName);
        }
    }

}