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
    public Queue<ObjectInfo> objectsAwaitingActivation;
    public Queue<ObjectInfo> loadedObjects;
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
        objectsAwaitingActivation = new Queue<ObjectInfo>();
        loadedObjects = new Queue<ObjectInfo>();
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
        //newChunk.chunkObjects = newObjects;

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

                            AddObjects(tile, row, column);
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
     * Adds a trees and bushes
     */
    private void AddObjects(Tile tile, int row, int column)
    {
        float currentHeight = (float)MG.treePerlin.GetValue((row + chunkPos.x) * MG.scale, (column + chunkPos.y) * MG.scale, 0);

        if (tile.tileLayer >= 3 && currentHeight >= MG.bushLevel && currentHeight <= MG.bushLevel + MG.objectBuffer)
        {
            ObjectInfo newObject = new ObjectInfo();

            newObject.objectPos = new Vector3(row + (int)chunkPos.x, column + (int)chunkPos.y, 0);
            newObject.objectType = ObjectType.Bush;

            objectsAwaitingActivation.Enqueue(newObject);
        }
        else if (tile.tileLayer >= 3 && currentHeight >= MG.treeLevel && currentHeight <= MG.treeLevel + MG.objectBuffer)
        {
            //Picks random number for random tree type
            //int randTree = Random.Range((int)ObjectType.Tree1, (int)ObjectType.Tree2 + 1);

            ObjectInfo newObject = new ObjectInfo();

            newObject.objectPos = new Vector3(row + (int)chunkPos.x, column + (int)chunkPos.y, 0);
            newObject.objectType = ObjectType.Tree1;

            objectsAwaitingActivation.Enqueue(newObject);
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
        return "Size: " + chunkSize + " Pos: (" + chunkPos.x + "," + chunkPos.y + ") Num Tiles: " + chunkLoadedTiles.Count + " Num Pooled Tiles: " + chunkPooledTiles.Count + " Num Objects: " + loadedObjects.Count;
    }

    public bool IsEmpty()
    {
        if (chunkSize == 0)
            return true;
        else
            return false;
    }
}
