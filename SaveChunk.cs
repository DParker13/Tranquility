using System.Collections.Generic;

[System.Serializable]
public class SaveChunk
{
    public int size;
    public int posX;
    public int posY;
    public SaveTile[] tiles;
    public SaveObject[] objects;

    /*
     * Allows for type cast from Chunk to SaveChunk
     */
    public static explicit operator SaveChunk(Chunk chunk)
    {
        SaveChunk sc = new SaveChunk();

        sc.posX = (int)chunk.chunkPos.x;
        sc.posY = (int)chunk.chunkPos.y;
        sc.size = chunk.chunkSize;

        //Type casts all of the chunk's loaded Tiles into a SaveTile
        List<SaveTile> saveTiles = new List<SaveTile>();
        for (int i = 0; i < chunk.chunkLoadedTiles.Count; i++)
        {
            saveTiles.Add((SaveTile)chunk.chunkLoadedTiles[i]);
        }

        sc.tiles = saveTiles.ToArray();

        List<SaveObject> saveObjects = new List<SaveObject>();
        for (int i = 0; i < chunk.chunkObjects.Count; i++)
        {
            SaveObject saveObject = new SaveObject();

            saveObject.posX = chunk.chunkObjects[i].posX;
            saveObject.posY = chunk.chunkObjects[i].posY;
            saveObject.posZ = chunk.chunkObjects[i].posZ;
            saveObject.numHits = chunk.chunkObjects[i].numHits;
            saveObject.numHitsTillBreak = chunk.chunkObjects[i].numHitsTillBreak;
            saveObject.broken = chunk.chunkObjects[i].broken;
            saveObject.objectType = chunk.chunkObjects[i].objectType;

            saveObjects.Add(saveObject);
        }

        sc.objects = saveObjects.ToArray();

        return sc;
    }

    public override string ToString()
    {
        return "Size: " + size + " Pos: (" + posX + "," + posY + ") Num Tiles: " + tiles.Length + " Num Objects: " + objects.Length;
    }
}