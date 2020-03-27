using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChangeScene : MonoBehaviour {

    public GameObject slider;

    public void ChangeToScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

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
}
