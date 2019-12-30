using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType
{
    Bush,
    Tree1,
    Tree2,
    Rock,
    Log,
    LongGrass
}

public class ObjectInfo : MonoBehaviour
{
    public int posX;
    public int posY;
    public float posZ;
    public int numHits;
    public int numHitsTillBreak;
    public bool broken;
    public ObjectType objectType;
    public SaveObject savedObject;

    /*
     * Allows to type cast the SaveTile to a Tile
     */
    public static explicit operator ObjectInfo(SaveObject savedObject)
    {
        ObjectInfo newObject = new ObjectInfo();

        newObject.posX = savedObject.posX;
        newObject.posY = savedObject.posY;
        newObject.posZ = savedObject.posZ;
        newObject.numHits = savedObject.numHits;
        newObject.numHitsTillBreak = savedObject.numHitsTillBreak;
        newObject.broken = savedObject.broken;
        newObject.objectType = savedObject.objectType;
        newObject.savedObject = savedObject;

        return newObject;
    }

}
