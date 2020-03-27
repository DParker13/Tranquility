[System.Serializable]
public class SaveTile
{
    public int id;
    public int layer;
    public int posX;
    public int posY;
    public Type type;
    public int[] neighbors = new int[4];
    public int autotileID;

    public static explicit operator SaveTile(Tile tile)
    {
        SaveTile saveTile = new SaveTile();

        saveTile.id = tile.tileID;
        saveTile.layer = tile.tileLayer;
        saveTile.posX = tile.tilePos.x;
        saveTile.posY = tile.tilePos.y;
        saveTile.neighbors = tile.tileNeighbors;
        saveTile.type = tile.tileType;
        saveTile.autotileID = tile.tileAutotileID;

        return saveTile;
    }
}