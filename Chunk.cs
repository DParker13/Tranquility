using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public int size;
    public int posX;
    public int posY;
    public List<Tile> tiles;
    public List<ObjectInfo> objects;

    //Save info
    public List<SaveObject> savedObjects;
    public List<SaveTile> savedTiles;
    public SaveChunk savedChunk;

    /*
     * Allows to type cast the SaveChunk to a Chunk
     */
    public static explicit operator Chunk(SaveChunk savedChunk)
    {
        Chunk newChunk = new Chunk();
        newChunk.size = savedChunk.size;
        newChunk.posX = savedChunk.posX;
        newChunk.posY = savedChunk.posY;

        //Type casts all of the tiles
        List<Tile> newTiles = new List<Tile>();
        foreach(SaveTile tile in savedChunk.tiles)
        {
            newTiles.Add((Tile)tile);
        }
        newChunk.tiles = newTiles;
        newChunk.savedTiles = new List<SaveTile>(savedChunk.tiles);

        //Type casts all of the objects
        List<ObjectInfo> newObjects = new List<ObjectInfo>();
        foreach (SaveObject o in savedChunk.objects)
        {
            newObjects.Add((ObjectInfo)o);
        }
        newChunk.objects = newObjects;
        newChunk.savedObjects = new List<SaveObject>(savedChunk.objects);

        return newChunk;
    }
}
