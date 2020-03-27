using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class SceneController : MonoBehaviour {

    public string worldName;
    public GameObject worldButton;

    // Use this for initialization
    void Start ()
    {
        //Makes this game object stay between scene changes
        DontDestroyOnLoad(gameObject);

        //Sets the user settings
        var musicGO = GameObject.FindGameObjectWithTag("Music");
        var muteGO = GameObject.FindGameObjectWithTag("Mute");
        musicGO.GetComponent<AudioSource>().mute = System.Convert.ToBoolean(PlayerPrefs.GetInt("Menu Music"));

        //Changes sprite for mute button
        if (musicGO.GetComponent<AudioSource>().mute == true)
        {
            muteGO.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Buttons/MusicButton_Off");
        }
        else
        {
            muteGO.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Buttons/MusicButton_On");
        }
	}

    /*
     * Creates a new map and changes to the main scene
     */
    public void CreateMap(string sceneName)
    {
        worldName = GameObject.FindGameObjectWithTag("Map Name Input").GetComponent<InputField>().text;

        if(worldName.Trim() == "")
        {
            worldName = "New World";
        }
        
        Directory.CreateDirectory(Application.persistentDataPath + "/" + worldName);
        ChangeToScene(sceneName);
    }

    public void ToggleMusic()
    {
        GameObject musicGO = GameObject.FindGameObjectWithTag("Music");
        GameObject muteGO = GameObject.FindGameObjectWithTag("Mute");

        if (musicGO.GetComponent<AudioSource>().mute == true)
        {
            //Un-mutes the menu music
            musicGO.GetComponent<AudioSource>().mute = false;
            muteGO.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Buttons/MusicButton_On");
            PlayerPrefs.SetInt("Menu Music", 0);
        }
        else
        {
            //Mutes the menu music
            musicGO.GetComponent<AudioSource>().mute = true;
            muteGO.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/Buttons/MusicButton_Off");
            PlayerPrefs.SetInt("Menu Music", 1);
        }
        PlayerPrefs.Save();
    }

    public void LoadMap(string sceneName)
    {
        if (Directory.Exists(Application.persistentDataPath + "/Worlds"))
        {
            
        }
    }

    /*
     * Changes to the desired scene
     */
    public void ChangeToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /*
     * Pretty easy to understand what this does
     */
    public void Quit()
    {
        Application.Quit();
    }

    /* WIP for loading bar
     * 
    public void ChangeToSceneSlider(string sceneName)
    {
        slider.SetActive(true);
        StartCoroutine(LoadAsynchronously(sceneName));
    }

    IEnumerator LoadAsynchronously(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            slider.GetComponent<Slider>().value = progress;

            yield return null;
        }
    }
    */
}
