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

        Queue<SaveObject> saveLoadedObjects = new Queue<SaveObject>();
        while(chunk.loadedObjects.Count > 0)
        {
            SaveObject saveObject = new SaveObject();
            ObjectInfo currentObject = chunk.loadedObjects.Dequeue();

            saveObject.objectPos = currentObject.objectPos;
            saveObject.numHits = currentObject.numHits;
            saveObject.numHitsTillBreak = currentObject.numHitsTillBreak;
            saveObject.broken = currentObject.broken;
            saveObject.objectType = currentObject.objectType;

            saveLoadedObjects.Enqueue(saveObject);
        }

        sc.objects = saveLoadedObjects.ToArray();

        return sc;
    }

    public override string ToString()
    {
        return "Size: " + size + " Pos: (" + posX + "," + posY + ") Num Tiles: " + tiles.Length + " Num Objects: " + objects.Length;
    }
}