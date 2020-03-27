using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScreenFade : MonoBehaviour
{
    public Image splashImage;
    public string sceneName;

    IEnumerator Start()
    {
        splashImage.canvasRenderer.SetAlpha(0);

        FadeIn();
        yield return new WaitForSeconds(2.5f);
        FadeOut();
        yield return new WaitForSeconds(2.5f);
        SceneManager.LoadScene(sceneName);
    }

    private void FadeIn()
    {
        splashImage.CrossFadeAlpha(1.0f, 1.5f, false);
    }

    private void FadeOut()
    {
        splashImage.CrossFadeAlpha(0, 1.5f, false);
    }
}
