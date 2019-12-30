using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;
using System.Threading;

public class SaveLoadData
{
    public SaveChunk currentChunk;
    public MapGenerator mapGenerator;

    public SaveLoadData()
    {
        currentChunk = null;
    }

    public SaveLoadData(SaveChunk currentChunk)
    {
        this.currentChunk = currentChunk;
    }

    //------------CHUNK SAVING AND LOADING--------------

    public static void SaveChunk(string persistentDataPath, int x, int y, SaveChunk chunkInfo, string worldName)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        if (File.Exists(persistentDataPath + "/Worlds/" + worldName + "/chunk" + x + "," + y + ".dat"))
        {
            file = File.Open(persistentDataPath + "/Worlds/" + worldName + "/chunk" + x + "," + y + ".dat", FileMode.Open);
        }
        else
        {
            file = File.Create(persistentDataPath + "/Worlds/" + worldName + "/chunk" + x + "," + y + ".dat");
        }

        bf.Serialize(file, chunkInfo);
        file.Close();
    }

    public Chunk LoadChunk(string persistentDataPath, float x, float y, string worldName)
    {
        try
        {
            if (File.Exists(persistentDataPath + "/Worlds/" + worldName + "/chunk" + x + "," + y + ".dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file;

                file = File.Open(persistentDataPath + "/Worlds/" + worldName + "/chunk" + x + "," + y + ".dat", FileMode.Open);

                SaveChunk chunkData = (SaveChunk)bf.Deserialize(file);

                file.Close();

                return (Chunk)chunkData;
            }
            else
            {
                Debug.LogError("Chunk " + x + " " + y + " Not Found");
                return null;
            }
        }
        catch(IOException ex)
        {
            Debug.Log(ex.StackTrace);

            mapGenerator.chunkThread.Abort();

            return null;
        }
    }

    //------------WORLD SAVING--------------

    public static void SaveWorld(int seed, int octaves, int frequency, float persistance, float scale, string worldName)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file;

        Debug.Log("Creating New World!");

        //Creates new Worlds directory if one is not there already
        if (!Directory.Exists(Application.persistentDataPath + "/Worlds"))
        {
            Debug.Log("Worlds directory missing - creating new folder");
            Directory.CreateDirectory(Application.persistentDataPath + "/Worlds");
            Debug.Log("<color=green>Worlds directory created!</color>");
        }

        file = File.Create(Application.persistentDataPath + "/Worlds/" + worldName + ".dat");
        Debug.Log("<color=green>Created New World!</color>");

        SaveWorld worldData = new SaveWorld(worldName, seed, octaves, frequency, persistance, scale);

        bf.Serialize(file, worldData);
        file.Close();

        Debug.Log("<color=green>World Successfully Saved!</color>");
    }

    public static SaveWorld LoadWorld(string worldName)
    {
        if (File.Exists(Application.persistentDataPath + "/Worlds/" + worldName + ".dat"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file;

            file = File.Open(Application.persistentDataPath + "/Worlds/" + worldName + ".dat", FileMode.Open);

            SaveWorld worldData = (SaveWorld)bf.Deserialize(file);

            file.Close();
            Debug.Log("<color=green>Loaded World!</color>");

            return worldData;
        }
        else
        {
            Debug.Log("World " + worldName + " not found");
            return null;
        }
    }

}