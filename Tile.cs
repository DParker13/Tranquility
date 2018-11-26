using System.Collections;
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

public class Tile
{
    public int id;
    public int layer;
    public float tileX;
    public float tileY;
    public Type type;
    public int[] neighbors = new int[4];
    public int autotileID;
    public SaveTile tileSave;

    public Tile(int i, int l, float x, float y, Type t)
    {
        id = i;
        layer = l;
        tileX = x;
        tileY = y;
        type = t;

        //Saves the tile
        tileSave = new SaveTile();
        tileSave.id = i;
        tileSave.layer = l;
        tileSave.posX = x;
        tileSave.posY = y;
        tileSave.type = t;
    }

    /*
     * Allows to type cast the SaveTile to a Tile
     */
    public static explicit operator Tile(SaveTile savedTile)
    {
        Tile newTile = new Tile(savedTile.id, savedTile.layer, savedTile.posX, savedTile.posY, savedTile.type);

        newTile.neighbors = savedTile.neighbors;
        newTile.autotileID = savedTile.autotileID;

        return newTile;
    }

    /*
     * Calculates the amount of neighbors the tile has based on neighboring layer heights
     */
    public void AddNeighbor(Sides side, int neighborLayer)
    {
        if(neighborLayer == layer)
        {
            neighbors[(int)side] = 1;

            CalculateAutotileID();
        }
    }

    /*
     * Calculates the tile sprite index based on the number of neighbors
     */
    private void CalculateAutotileID()
    {
        int total = 0;
        for(int i = 0; i < neighbors.Length; i++)
        {
            if(neighbors[i] == 1)
            {
                total += Mathf.RoundToInt(Mathf.Pow(2, i));
            }
        }

        autotileID = total;

        //Saves more information of the tile
        tileSave.autotileID = total;
        tileSave.neighbors = neighbors;
    }
}
