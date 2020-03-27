using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    public GameObject pauseMenuGUI;
    public GameObject inventoryMenuGUI;

    public bool gameIsPaused = false;
    public bool inPauseMenu = false;
    public bool inInventoryMenu = false;


    // Update is called once per frame
    void Update ()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameIsPaused)
            {
                inPauseMenu = false;
                pauseMenuGUI.SetActive(false);
                Resume();
            }
            else
            {
                Pause();
                inPauseMenu = true;
                pauseMenuGUI.SetActive(true);
            }
        }
        if (Input.GetKeyDown(KeyCode.E) && !inPauseMenu)
        {
            if (gameIsPaused)
            {
                inInventoryMenu = false;
                inventoryMenuGUI.SetActive(false);
                Resume();
            }
            else
            {
                Pause();
                inInventoryMenu = true;
                inventoryMenuGUI.SetActive(true);
            }
        }
	}

    public void Resume()
    {
        Time.timeScale = 1;
        gameIsPaused = false;
    }

    public void Pause()
    {
        Time.timeScale = 0;
        gameIsPaused = true;
    }

    public void ChangeScene(string sceneName)
    {
        Resume();
        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
