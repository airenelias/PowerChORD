using UnityEngine;
using System.Collections;

public class VisualizerFall : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(VisualizerFallCoroutine());
    }

    public void Raise(float height)
    {
        StartCoroutine(VisualizerRaiseCoroutine(height));
    }

    IEnumerator VisualizerFallCoroutine()
    {
        while (true)
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(gameObject.GetComponent<RectTransform>().sizeDelta.x, gameObject.GetComponent<RectTransform>().sizeDelta.y - 15);
            yield return new WaitForSeconds(0.01f);
        }
    }

    IEnumerator VisualizerRaiseCoroutine(float height)
    {
        float y = gameObject.GetComponent<RectTransform>().sizeDelta.y;
        while (y < height)
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(gameObject.GetComponent<RectTransform>().sizeDelta.x, y);
            y += 100;
            yield return new WaitForSeconds(0.01f);
        }
    }
}
