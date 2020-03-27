using UnityEngine;

public enum Sides
{
    //NorthWest,
    North,
    //NorthEast,
    West,
    East,
    //SouthWest,
    South,
    //SouthEast
}

public enum Type
{
    Water,
    Sand,
    Dirt,
    Grass,
    DarkGrass
}

public class Tile : MonoBehaviour
{
    public int tileID;
    public int tileLayer;
    public Vector2Int tilePos;
    public Type tileType;
    public int[] tileNeighbors;
    public int tileAutotileID;
    public bool tileChanged;

    //Parent Classes
    public MapGenerator MG;
    public Chunk parentChunk;

    public Tile(int id, int layer, int x, int y, Type type, int[] neighbors, int autotileID, MapGenerator MG, Chunk parent)
    {
        this.tileID = id;
        this.tileLayer = layer;
        this.tilePos = new Vector2Int(x, y);
        this.tileType = type;
        this.tileNeighbors = neighbors;
        this.tileAutotileID = autotileID;
        this.MG = MG;
        this.parentChunk = parent;
    }

    void Awake()
    {
        if(MG == null)
            MG = GetComponentInParent<MapGenerator>();

        if(parentChunk == null)
            parentChunk = GetComponentInParent<Chunk>();

        tileNeighbors = new int[4];
    }

    /*
     * Allows to type cast the SaveTile to a Tile
     */
    public static explicit operator Tile(SaveTile savedTile)
    {
        return new Tile(savedTile.id, savedTile.layer, savedTile.posX, savedTile.posY, savedTile.type, savedTile.neighbors, savedTile.autotileID, null, null);
    }

    /*
     * Changes the position and sprite of tile when it is loaded
     */
    public void UpdateTile()
    {
        Sprite[] sprites = null;

        //Updates the current tile's location and name
        transform.position = new Vector3(tilePos.x + parentChunk.chunkPos.x, tilePos.y + parentChunk.chunkPos.y, -tileLayer);
        name = "Tile (" + (tilePos.x) + "," + (tilePos.y) + ")";

        var spriteID = tileAutotileID;
        var sr = GetComponent<SpriteRenderer>();

        if (tileType == Type.DarkGrass)
        {
            sprites = Resources.LoadAll<Sprite>("Sprites/Tiles/" + MG.darkGrassTexture.name);
        }
        else if (tileType == Type.Grass)
        {
            sprites = Resources.LoadAll<Sprite>("Sprites/Tiles/" + MG.grassTexture.name);
        }
        else if (tileType == Type.Dirt)
        {
            sprites = Resources.LoadAll<Sprite>("Sprites/Tiles/" + MG.dirtTexture.name);
        }
        else if (tileType == Type.Sand)
        {
            sprites = Resources.LoadAll<Sprite>("Sprites/Tiles/" + MG.sandTexture.name);
        }
        else
        {
            sr.sprite = MG.waterTexture;
        }

        if (spriteID >= 0 && tileType != Type.Water && sprites != null)
        {
            sr.sprite = sprites[spriteID];
        }
    }

    /*
     * Calculates the amount of neighbors the tile has based on neighboring layer heights
     */
    public void AddNeighbor(Sides side, int neighborLayer)
    {
        if(neighborLayer == tileLayer)
        {
            tileNeighbors[(int)side] = 1;

            CalculateAutotileID();
        }
    }

    /*
     * Calculates the tile sprite index based on the number of neighbors
     */
    private void CalculateAutotileID()
    {
        int total = 0;

        for(int i = 0; i < tileNeighbors.Length; i++)
        {
            if(tileNeighbors[i] == 1)
            {
                total += Mathf.RoundToInt(Mathf.Pow(2, i));
            }
        }

        tileAutotileID = total;
    }

    /*
     * Counts the amount of neighbors the current tile has (will eventually calculate 8 neighbors)
     */
    public void FindNeighbors(int row, int column)
    {
        float[] heights = new float[4];
        int[] layers = new int[4];

        //Calculates the neighboring tiles perlin noise height
        //North
        heights[0] = (float)MG.perlin.GetValue((row + parentChunk.chunkPos.x) * MG.scale, (column + 1 + parentChunk.chunkPos.y) * MG.scale, 0);
        //West
        heights[1] = (float)MG.perlin.GetValue((row - 1 + parentChunk.chunkPos.x) * MG.scale, (column + parentChunk.chunkPos.y) * MG.scale, 0);
        //East
        heights[2] = (float)MG.perlin.GetValue((row + 1 + parentChunk.chunkPos.x) * MG.scale, (column + parentChunk.chunkPos.y) * MG.scale, 0);
        //South
        heights[3] = (float)MG.perlin.GetValue((row + parentChunk.chunkPos.x) * MG.scale, (column - 1 + parentChunk.chunkPos.y) * MG.scale, 0);

        //Loops through the layer heights to check what layer the tiles belong to
        for (int i = 0; i < heights.Length; i++)
        {
            //Resets the layer count
            int currentLayerTotal = 0;

            for (int h = 0; h < MG.layerHeights.Length; h++)
            {
                if (h == 0)
                {
                    AddNeighbor((Sides)i, layers[i]);
                }
                else
                {
                    if (heights[i] >= MG.layerHeights[h])
                    {
                        AddNeighbor((Sides)i, layers[i]);
                    }
                }
                currentLayerTotal++;
                layers[i] = currentLayerTotal;
            }
        }
    }

    /*
     * Returns the this Tile object
     */
    public Tile GetTile()
    {
        return new Tile(tileID, tileLayer, tilePos.x, tilePos.y, tileType, tileNeighbors, tileAutotileID, MG, parentChunk);
    }

    /*
     * Sets the Tile object
     */
    public void SetTile(int id, int layer, int x, int y, Type type, bool changed)
    {
        tileID = id;
        tileLayer = layer;
        tilePos = new Vector2Int(x, y);
        tileType = type;
        tileChanged = changed;
    }

    public void SetTile(Tile t)
    {
        tileID = t.tileID;
        tileLayer = t.tileLayer;
        tilePos = new Vector2Int(t.tilePos.x, t.tilePos.y);
        tileType = t.tileType;
        tileChanged = t.tileChanged;
        tileAutotileID = t.tileAutotileID;
    }

    public override string ToString()
    {
        return "ID: " + tileID + " Layer: " + tileLayer + " Pos: (" + tilePos.x + "," + tilePos.y + ")" + " Type: " + tileType.ToString() + " AutoTile ID: " + tileAutotileID;
    }
}
