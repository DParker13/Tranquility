using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeObjects : MonoBehaviour
{
    public float smoothFade = 0.125f;

    private float fadeValue;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            StopCoroutine("FadeIn");
            StartCoroutine("FadeOut");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.name == "Player")
        {
            StopCoroutine("FadeOut");
            StartCoroutine("FadeIn");
        }
    }

    IEnumerator FadeOut()
    {
        for (float i = 1; i > 0.4; i -= smoothFade)
        {
            fadeValue = i;
            var newColor = new Color(1, 1, 1, i);
            GetComponent<SpriteRenderer>().material.color = newColor;
            yield return null;
        }
    }

    IEnumerator FadeIn()
    {
        for (float i = fadeValue; i < 1; i += smoothFade)
        {
            var newColor = new Color(1, 1, 1, i);
            GetComponent<SpriteRenderer>().material.color = newColor;
            yield return null;
        }
    }

}
