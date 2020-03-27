using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ObjectType {
    Bush,
    Tree1,
    Tree2,
    Rock,
    Log,
    LongGrass
}

public class ObjectInfo : MonoBehaviour {
    public Vector3 objectPos;
    public int numHits;
    public int numHitsTillBreak;
    public bool broken;
    public ObjectType objectType;
    public SaveObject savedObject;

    /*
     * Allows to type cast the SaveObject to an Object
     */
    public static explicit operator ObjectInfo(SaveObject savedObject) {
        ObjectInfo newObject = new ObjectInfo();

        newObject.objectPos = savedObject.objectPos;
        newObject.numHits = savedObject.numHits;
        newObject.numHitsTillBreak = savedObject.numHitsTillBreak;
        newObject.broken = savedObject.broken;
        newObject.objectType = savedObject.objectType;
        newObject.savedObject = savedObject;

        return newObject;
    }

    public void SetObject(Vector3 objectPos, int numHits, int numHitsTillBreak, bool broken, ObjectType objectType) {
        this.objectPos = objectPos;
        this.numHits = numHits;
        this.numHitsTillBreak = numHitsTillBreak;
        this.broken = broken;
        this.objectType = objectType;
    }

    public void SetObject(ObjectInfo objectInfo) {
        objectPos = objectInfo.objectPos;
        numHits = objectInfo.numHits;
        numHitsTillBreak = objectInfo.numHitsTillBreak;
        broken = objectInfo.broken;
        objectType = objectInfo.objectType;
    }

}
