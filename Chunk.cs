using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public GameObject tilePrefab;
    public bool runTileUpdate;
    public int chunkSize;
    public Vector2Int chunkPos;
    public List<Tile> chunkLoadedTiles;
    public Queue<Tile> chunkPooledTiles;
    public List<ObjectInfo> chunkObjects;
    public List<Tile> updateTiles;
    public int chunkPooledTileCount;

    //blah
    public SaveChunk saveChunk;

    //Private Variables
    private MapGenerator MG;
    private bool startUp = true;

    private void Awake()
    {
        MG = GetComponentInParent<MapGenerator>();

        chunkLoadedTiles = new List<Tile>();
        chunkObjects = new List<ObjectInfo>();
        chunkPooledTiles = new Queue<Tile>();
        chunkSize = MG.chunkSize;
        updateTiles = new List<Tile>();

        CreateTiles();
    }

    private void Update()
    {
        if(runTileUpdate == true)
        {
            for(int i = 0; i < updateTiles.Count; i++)
            {
                Tile dequeuedTile = updateTiles[i];

                dequeuedTile.UpdateTile();
                dequeuedTile.gameObject.SetActive(true);

                chunkLoadedTiles.Add(dequeuedTile);
            }

            updateTiles.Clear();
            runTileUpdate = false;
        }
    }

    /*
     * Allows to type cast the SaveChunk to a Chunk
     */
    public static explicit operator Chunk(SaveChunk savedChunk)
    {
        //Creates a new Chunk
        Chunk newChunk = new Chunk();
        newChunk.chunkSize = savedChunk.size;
        newChunk.chunkPos = new Vector2Int(savedChunk.posX, savedChunk.posY);

        //Type casts all of the tiles
        List<Tile> newTiles = new List<Tile>();
        foreach (SaveTile savedTile in savedChunk.tiles)
        {
            newTiles.Add((Tile)savedTile);
        }
        newChunk.chunkLoadedTiles = newTiles;

        //Type casts all of the objects
        List<ObjectInfo> newObjects = new List<ObjectInfo>();
        foreach (SaveObject o in savedChunk.objects)
        {
            newObjects.Add((ObjectInfo)o);
        }
        newChunk.chunkObjects = newObjects;

        return newChunk;
    }

    /*
     * Fills all of the layers with the appropriate tiles
     */
    public void GenerateLayers()
    {
        for(int layer = 0; layer < MG.layerHeights.Length; layer++)
        {
            GenerateTiles(layer);
        }
    }

    /*
     * Gets each tile's layer height
     */
    private void GenerateTiles(int layer)
    {
        runTileUpdate = false;

        for (int row = 0; row < chunkSize; row++)
        {
            for (int column = 0; column < chunkSize; column++)
            {
                if(chunkPooledTiles != null)
                {
                    if (chunkPooledTiles.Count > 0)
                    {
                        //Gets the tile from the pool list and adds it to the loaded list
                        Tile tile = chunkPooledTiles.Peek();

                        tile.tileChanged = false;

                        //Current tile perlin noise height
                        float currentHeight = (float)MG.perlin.GetValue((row + chunkPos.x) * MG.scale, (column + chunkPos.y) * MG.scale, 0);

                        if (layer == 0) //Creates bottom layer (water)
                        {
                            tile.SetTile(row * column, layer, row, column, (Type)layer, true);
                        }
                        else //Creates inner layers
                        {
                            if (currentHeight >= MG.layerHeights[layer])
                            {
                                tile.SetTile(row * column, layer, row, column, (Type)layer, true);
                            }
                        }

                        //Only removes the tile from the pool if it was changed
                        if (tile.tileChanged == true)
                        {
                            tile.FindNeighbors(row, column);
                            updateTiles.Add(tile);
                            chunkPooledTiles.Dequeue();
                        }
                    }
                    else
                    {
                        Debug.LogError("Pool is empty and is still trying to be pulled from - Chunk" + chunkPos.ToString());
                    }
                }
                else
                {
                    Debug.LogError("Pool is null");
                }
            }
        }

        runTileUpdate = true;
    }

    /*
     * Adds all the tiles to the current pool for re-use and sets them to inactive
     */
    public void PoolAllTiles()
    {
        for (int i = 0; i < chunkLoadedTiles.Count; i++)
        {
            chunkPooledTiles.Enqueue(chunkLoadedTiles[i]);
            chunkLoadedTiles[i].gameObject.SetActive(false);
            chunkLoadedTiles[i].tileNeighbors = new int[4];
        }
        
        chunkLoadedTiles.Clear();

        chunkPooledTileCount = chunkPooledTiles.Count;
    }

    public void RemoveObjects()
    {
        for(int i = 0; i < chunkObjects.Count; i++)
        {
            GameObject currentObject = chunkObjects[i].gameObject;

            chunkObjects.RemoveAt(i);
            Destroy(currentObject);
        }
    }

    /*
     * Adds a random object to specific tiles depending on spawn percentage
     */
    private void AddObjects(int row, int column, int layer)
    {
        float currentHeight = (float)MG.treePerlin.GetValue((row + chunkPos.x) * MG.scale, (column + chunkPos.y) * MG.scale, 0);

        if (layer >= 3 && currentHeight >= 0.95f && currentHeight <= 1)
        {
            //Creates the object in the game world
            var currentObject = Instantiate(MG.objects[(int)ObjectType.Bush], new Vector3(row + chunkPos.x, column + chunkPos.y, -(row+column) / 100), Quaternion.identity, gameObject.transform);

            ObjectInfo newObject = currentObject.GetComponent<ObjectInfo>();
            newObject.posX = row + (int)chunkPos.x;
            newObject.posY = column + (int)chunkPos.y;
            newObject.posZ = (row+column) / 100;
            newObject.objectType = ObjectType.Bush;

            chunkObjects.Add(newObject);
        }
        else if (layer >= 3 && currentHeight >= 0.8f && currentHeight < 0.95f)
        {
            //Picks random number for random tree type
            int randTree = UnityEngine.Random.Range((int)ObjectType.Tree1, (int)ObjectType.Tree2 + 1);

            //Creates the object in the game world
            var currentObject = Instantiate(MG.objects[randTree], new Vector3(row + chunkPos.x, column + chunkPos.y, -(row + column) / 100), Quaternion.identity, gameObject.transform);

            ObjectInfo newObject = currentObject.GetComponent<ObjectInfo>();
            newObject.posX = row + (int)chunkPos.x;
            newObject.posY = column + (int)chunkPos.y;
            newObject.posZ = (row + column) / 100;
            newObject.objectType = (ObjectType)randTree;

            chunkObjects.Add(newObject);
        }
    }

    /*
     * Creates all of the tiles for the chunk
     */
    private void CreateTiles()
    {
        if(startUp == true)
        {
            //Creates the layers and tiles
            for (int layer = 0; layer < MG.layerHeights.Length; layer++)
            {
                for (int row = 0; row < chunkSize; row++)
                {
                    for (int col = 0; col < chunkSize; col++)
                    {
                        var tileGO = Instantiate(tilePrefab, new Vector3(row, col, 0), Quaternion.identity, transform);
                        chunkPooledTiles.Enqueue(tileGO.GetComponent<Tile>());
                        tileGO.SetActive(true);
                    }
                }
            }
            startUp = false;

        }
    }

    /*
     * Sets the position of the chunk
     */
    public void SetPosition(int x, int y)
    {
        chunkPos = new Vector2Int(x, y);
    }

    public void UpdateChunk()
    {
        transform.position = new Vector3(chunkPos.x, chunkPos.y, 0);
    }

    public override string ToString()
    {
        return "Size: " + chunkSize + " Pos: (" + chunkPos.x + "," + chunkPos.y + ") Num Tiles: " + chunkLoadedTiles.Count + " Num Pooled Tiles: " + chunkPooledTiles.Count + " Num Objects: " + chunkObjects.Count;
    }

    public bool IsEmpty()
    {
        if (chunkSize == 0)
            return true;
        else
            return false;
    }
}
