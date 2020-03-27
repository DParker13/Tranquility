using UnityEngine;

[System.Serializable]
public class SaveObject
{
    public Vector3 objectPos;
    public int numHits;
    public int numHitsTillBreak;
    public bool broken;
    public ObjectType objectType;
}