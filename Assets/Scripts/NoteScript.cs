using UnityEngine;
using System.Collections;

public class NoteScript : MonoBehaviour
{
    public int number;
    public int tail;
    public bool playing = true;

    void OnEnable()
    {
        StartCoroutine(FadeInCoroutine());
        StartCoroutine(FallCoroutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator FadeInCoroutine()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0);
        if (MainManager.instance.mods["PERFORMANCE"]) yield break;
        while (gameObject.GetComponent<SpriteRenderer>().color.a < 1)
        {
            while (!playing) { yield return 0; }
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, gameObject.GetComponent<SpriteRenderer>().color.a + 0.05f);
            yield return new WaitForSecondsRealtime(0.01f);
        }
        if (MainManager.instance.mods["LONGSIGHT"]) StartCoroutine(FadeOutCoroutine());
        yield break;
    }

    IEnumerator FadeOutCoroutine()
    {
        gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        while (gameObject.GetComponent<SpriteRenderer>().color.a > 0)
        {
            while (!playing) { yield return 0; }
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, gameObject.GetComponent<SpriteRenderer>().color.a - 0.05f);
            yield return new WaitForSecondsRealtime(0.01f);
        }
        yield break;
    }

    IEnumerator FallCoroutine()
    {
        while (transform.localPosition.y > -220f)
        {
            while (!playing) { yield return 0; }
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y - (180 / 100f) * MainManager.instance.speed, transform.localPosition.z - (500 / 100f) * MainManager.instance.speed);
            yield return new WaitForSecondsRealtime(0.01f);
        }
        gameObject.SetActive(false);
    }
}
