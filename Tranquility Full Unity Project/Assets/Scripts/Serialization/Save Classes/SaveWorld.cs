[System.Serializable]
public class SaveWorld
{
    public int seed;
    public int octaves;
    public int frequency;
    public float persistance;
    public float scale;
    public string worldName;

    public SaveWorld(string worldName, int seed, int octaves, int frequency, float persistance, float scale)
    {
        this.worldName = worldName;
        this.seed = seed;
        this.octaves = octaves;
        this.frequency = frequency;
        this.persistance = persistance;
        this.scale = scale;
    }
}
