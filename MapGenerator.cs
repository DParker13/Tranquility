using System.Collections.Generic;
using UnityEngine;
using LibNoise.Generator;
using System.IO;

public class MapGenerator : MonoBehaviour
{
    [Header("Player")]
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
    public int xOffset;
    public int yOffset;
    public float buffer = 0.1f;

    [Header("Perlin Noise Options")]
    public Perlin perlin = new Perlin();
    public int octaves = 3;
    public int frequency = 2;
    public float persistance = 0.5f;
    public float scale = 0.01f;
    public int seed = 102938;

    [Header("Tree Perlin Noise Options")]
    public Perlin treePerlin = new Perlin();
    public int treeOctaves = 6;
    public int treeFrequency = 4;
    public float treePersistance = 1;

    [Header("Map Objects")]
    public int percBush = 10;
    public GameObject[] objects;

    [Header("Map Layers")]
    public float waterLevel = 0.25f;
    public float sandLevel = 0.5f;
    public float dirtLevel = 0.6f;
    public float grassLevel = 0.7f;
    public float darkGrassLevel = 0.8f;
    public float[] layerHeights;

    [Header("Visualize Map")]
    public GameObject tilePrefab;

    [Header("Map Sprites")]
    public Texture2D darkGrassTexture;
    public Texture2D grassTexture;
    public Texture2D dirtTexture;
    public Texture2D sandTexture;
    public Sprite waterTexture;

    public Dictionary<Vector2, Chunk> chunks;

    void Awake()
    {
        var sceneController = GameObject.FindGameObjectWithTag("Scene Controller");

        worldName = sceneController.GetComponent<SceneController>().worldName;

        //Sets the perlin generator
        perlin.Seed = seed;
        perlin.OctaveCount = octaves;
        perlin.Frequency = frequency;
        perlin.Persistence = persistance;

        chunks = new Dictionary<Vector2, Chunk>();
        layerHeights = new float[] { waterLevel, sandLevel, dirtLevel, grassLevel, darkGrassLevel };
    }

    void Update()
    {
        LoadChunksAroundPlayer();
    }

    /*
     * Loads chunks around the player with an inputed render distance
     */
    public void LoadChunksAroundPlayer()
    {
        FileInfo[] savedChunks = null;

        int posX = (int)player.transform.position.x;
        int posY = (int)player.transform.position.y;

        if (saveMode == true)
        {
            //Gets all of the saved chunk files
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath + "/" + worldName);
            savedChunks = dir.GetFiles("*.dat*");
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
                    for (int i = 0; i < savedChunks.Length; i++)
                    {
                        if (savedChunks[i].Name.Contains(newX + "," + newY))
                        {
                            LoadChunkAt(newX, newY);
                        }
                        else
                        {
                            MakeChunkAt(newX, newY);
                        }
                    }
                }
                else
                {
                    MakeChunkAt(newX, newY);
                }
            }
        }
        DeleteChunks();
    }

    /*
     * Makes a new chunk with tiles and objects at desired position
     */
    public void MakeChunkAt(int x, int y)
    {
        if (!chunks.ContainsKey(new Vector2(x, y)))
        {
            //Creates an empty gameobject chunk to store the tiles
            var chunkGO = Instantiate(chunkEmptyObject, new Vector3(x, y, 0), Quaternion.identity, transform);
            var chunk = chunkGO.GetComponent<Chunk>();
            chunkGO.name = "Chunk(" + x + "," + y + ")";

            //Fills the chunk class component on the chunk GO
            chunk.size = chunkSize;
            chunk.posX = x;
            chunk.posY = y;
            chunk.tiles = new List<Tile>();

            //Creates the tiles on the chunk
            CreateLayers(chunk);

            //Adds chunk to the total list of chunks
            chunks.Add(new Vector2(x, y), chunk);

            //Saves the chunks
            if (saveMode == true)
            {
                SaveChunks(worldName);
            }
        }
    }

    /*
     * Loads a new chunk from file with tiles and objects at desired position
     */
    public void LoadChunkAt(int x, int y)
    {
        if (!chunks.ContainsKey(new Vector2(x, y)))
        {
            SaveLoadData saveLoadData = new SaveLoadData();

            var savedChunk = saveLoadData.LoadChunk(x, y, worldName);

            if(savedChunk != null)
            {
                //Creates an empty gameobject chunk to store the tiles
                var chunkGO = Instantiate(chunkEmptyObject, new Vector3(x, y, 0), Quaternion.identity, transform);
                var newChunk = chunkGO.GetComponent<Chunk>();
                chunkGO.name = "Chunk(" + x + "," + y + ")";

                //Fills the chunk class component on the chunk GO
                newChunk.size = savedChunk.size;
                newChunk.posX = savedChunk.posX;
                newChunk.posY = savedChunk.posY;
                newChunk.tiles = savedChunk.tiles;
                newChunk.objects = savedChunk.objects;

                //Creates the tiles on the chunk
                CreateLayers(newChunk);

                //Adds chunk to the total list of chunks
                chunks.Add(new Vector2(x, y), newChunk);
            }
        }
    }

    /*
     * Deletes chunks around the player with an inputed delete distance
     */
    public void DeleteChunks()
    {
        List<Chunk> deleteChunks = new List<Chunk>(chunks.Values);

        for (int i = 0; i < deleteChunks.Capacity; i++)
        {
            int newX = (int)(player.transform.position.x / chunkSize) * chunkSize;
            int newY = (int)(player.transform.position.y / chunkSize) * chunkSize;

            float distance = Vector2.Distance(new Vector2(newX, newY), deleteChunks[i].gameObject.transform.position);

            if (distance > deleteDistance * chunkSize)
            {
                chunks.Remove(deleteChunks[i].gameObject.transform.position);
                Destroy(deleteChunks[i].gameObject);
            }
        }
    }

    /*
     * Creates every layer of tiles changing by their respected heights
     */
    private void CreateLayers(Chunk chunk)
    {
        for (int i = 0; i < layerHeights.Length; i++)
        {
            CreateTiles(chunk, i);
        }
    }

    /*
     * Creates a new tile object child of the current chunk
     */
    private void CreateTiles(Chunk chunk, int layer)
    {
        int row = 0;
        int column = 0;

        for (int i = 0; i < chunkSize * chunkSize; i++)
        {
            Tile tile = null;

            //Current tile perlin noise height
            float currentHeight = (float)perlin.GetValue((row + chunk.posX) * scale, (column + chunk.posY) * scale, 0);

            if (layer == 0)
            {
                if (currentHeight - buffer <= layerHeights[layer])
                {
                    tile = new Tile(row * column, layer, row + chunk.posX, column + chunk.posY, (Type)layer); //New Tile
                }
            }
            else if (layer == layerHeights.Length - 1)
            {
                if(currentHeight + buffer >= layerHeights[layer])
                {
                    tile = new Tile(row * column, layer, row + chunk.posX, column + chunk.posY, (Type)layer); //New Tile
                }
            }
            else
            {
                if (currentHeight + buffer >= layerHeights[layer] && currentHeight <= layerHeights[layer + 1])
                {
                    //Current tile
                    tile = new Tile(row * column, layer, row + chunk.posX, column + chunk.posY, (Type)layer);
                }
            }

            //Only instantiates the tile if there is one present
            if(tile != null)
            {
                //Finds the neighbors of the current tile to auto tile
                FindNeighbors(chunk, tile, row, column);

                //Fills the tile with the correct sprite
                FillTile(tile, chunk.transform);

                //Spawns objects
                AddObjects(chunk, tile, row, column);

                //Adds the current tile to the total tiles on the current chunk
                chunk.tiles.Add(tile);
            }

            //Increments to the next column when it reaches the end of the chunksize(row)
            if (row == chunkSize - 1)
            {
                column++;
                row = 0;
            }
            else
            {
                row++;
            }
        }
    }

    /*
     * Counts the amount of neighbors the current tile has (will eventually calculate 8 neighbors)
     */
    private void FindNeighbors(Chunk chunk, Tile tile, int row, int column)
    {
        float[] heights = new float[4];
        int[] layers = new int[4];

        //Calculates the neighboring tiles perlin noise height
        //North
        heights[0] = (float)perlin.GetValue((row + chunk.posX) * scale, (column + 1 + chunk.posY) * scale, 0);
        //West
        heights[1] = (float)perlin.GetValue((row - 1 + chunk.posX) * scale, (column + chunk.posY) * scale, 0);
        //East
        heights[2] = (float)perlin.GetValue((row + 1 + chunk.posX) * scale, (column + chunk.posY) * scale, 0);
        //South
        heights[3] = (float)perlin.GetValue((row + chunk.posX) * scale, (column - 1 + chunk.posY) * scale, 0);

        //Loops through the layer heights to check what layer the tiles belong to
        for (int i = 0; i < heights.Length; i++)
        {
            //Resets the layer count
            int currentLayerTotal = 0;

            for (int h = 0; h < layerHeights.Length; h++)
            {
                if (h == 0)
                {
                    if (heights[i] - buffer <= layerHeights[h])
                    {
                        tile.AddNeighbor((Sides)i, layers[i]);
                    }
                }
                else if (h == layerHeights.Length - 1)
                {
                    if (heights[i] + buffer >= layerHeights[h])
                    {
                        tile.AddNeighbor((Sides)i, layers[i]);
                    }
                }
                else
                {
                    if (heights[i] + buffer >= layerHeights[h] && heights[i] <= layerHeights[h + 1])
                    {
                        tile.AddNeighbor((Sides)i, layers[i]);
                    }
                }
                currentLayerTotal++;
                layers[i] = currentLayerTotal;
            }
        }
    }

    /*
     * Instantiates the tile as a gameobject and assigns the appropriate sprite to the sprite renderer
     */
    private void FillTile(Tile tile, Transform chunkPosition)
    {
        //Assigns a gameobject tile with a sprite
        if (tile != null)
        {
            Sprite[] sprites = null;

            //Creates a new tile gameobject
            var go = Instantiate(tilePrefab, new Vector3(tile.tileX, tile.tileY, 0), Quaternion.identity, chunkPosition);
            go.name = "Tile (" + (tile.tileX) + "," + (tile.tileY) + ")";

            var spriteID = tile.autotileID;
            var sr = go.GetComponent<SpriteRenderer>();
            sr.sortingOrder = tile.layer;

            if (tile.type == Type.DarkGrass)
            {
                sprites = Resources.LoadAll<Sprite>("Sprites/Tiles/" + darkGrassTexture.name);
            }
            else if (tile.type == Type.Grass)
            {
                sprites = Resources.LoadAll<Sprite>("Sprites/Tiles/" + grassTexture.name);
            }
            else if (tile.type == Type.Dirt)
            {
                sprites = Resources.LoadAll<Sprite>("Sprites/Tiles/" + dirtTexture.name);
            }
            else if (tile.type == Type.Sand)
            {
                sprites = Resources.LoadAll<Sprite>("Sprites/Tiles/" + sandTexture.name);
                Debug.Log("Sprites/Tiles/" + sandTexture.name);
            }
            else
            {
                sr.sprite = waterTexture;
            }

            if (spriteID >= 0 && tile.type != Type.Water && sprites != null)
            {
                sr.sprite = sprites[spriteID];
            }
        }
    }

    /*
     * (WIP) Adds a random object to specific tiles depending on spawn percentage
     */
    private void AddObjects(Chunk chunk, Tile tile, int row, int column)
    {
        treePerlin = new Perlin();
        treePerlin.Seed = seed;
        treePerlin.OctaveCount = treeOctaves;
        treePerlin.Frequency = treeFrequency;
        treePerlin.Persistence = treePersistance;

        float currentHeight = (float)treePerlin.GetValue((row + chunk.posX) * scale, (column + chunk.posY) * scale, 0);

        if (tile.layer >= 3 && currentHeight >= 0.95f && currentHeight <= 1)
        {
            //Creates the object in the game world
            var currentObject = Instantiate(objects[(int)ObjectType.Bush], new Vector3(row + chunk.posX, column + chunk.posY, tile.tileY / 100), Quaternion.identity, chunk.gameObject.transform);

            ObjectInfo newObject = currentObject.GetComponent<ObjectInfo>();
            newObject.posX = row + chunk.posX;
            newObject.posY = column + chunk.posY;
            newObject.posZ = tile.tileY / 100;
            newObject.objectType = ObjectType.Bush;

            //Saves the new object
            SaveObject saveObject = new SaveObject();
            saveObject.posX = newObject.posX;
            saveObject.posY = newObject.posY;
            saveObject.posZ = newObject.posZ;
            saveObject.objectType = newObject.objectType;

            newObject.savedObject = saveObject;

            chunk.objects.Add(newObject);
        }
        else if (tile.layer >= 3 && currentHeight >= 0.8f && currentHeight < 0.95f)
        {
            //Creates the object in the game world
            var currentObject = Instantiate(objects[(int)ObjectType.Tree], new Vector3(row + chunk.posX, column + chunk.posY, tile.tileY / 100), Quaternion.identity, chunk.gameObject.transform);

            ObjectInfo newObject = currentObject.GetComponent<ObjectInfo>();
            newObject.posX = row + chunk.posX;
            newObject.posY = column + chunk.posY;
            newObject.posZ = tile.tileY / 100;
            newObject.objectType = ObjectType.Tree;

            //Saves the new object
            SaveObject saveObject = new SaveObject();
            saveObject.posX = newObject.posX;
            saveObject.posY = newObject.posY;
            saveObject.posZ = newObject.posZ;
            saveObject.objectType = newObject.objectType;

            newObject.savedObject = saveObject;

            chunk.objects.Add(newObject);
        }
    }

    /*
     * Sets the seed to a random integer and generates a new map
     */
    public void RandomMap()
    {
        seed = Random.Range(0, int.MaxValue);
        perlin.Seed = seed;
        Debug.Log("Created a random map " + seed);
    }

    /*
     * WIP Saves the visible chunks to file
     */
    public void SaveChunks(string worldName)
    {
        //Gets all of the saved chunk files
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] savedChunks = dir.GetFiles("*.dat*");
        SaveLoadData saveLoadData = new SaveLoadData();

        foreach (KeyValuePair<Vector2, Chunk> chunk in chunks)
        {
            float x = chunk.Key.x;
            float y = chunk.Key.y;

            //Saves the current chunk
            saveLoadData.SaveChunk(x, y, chunk.Value, worldName);
        }
    }
}
