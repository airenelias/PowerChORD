using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FlashScript : MonoBehaviour
{
    public void Flash(int number)
    {
        switch (number)
        {
            case 0:
                {
                    gameObject.GetComponent<Image>().color = new Color32(20, 226, 20, 0);
                    break;
                }
            case 1:
                {
                    gameObject.GetComponent<Image>().color = new Color32(230, 28, 28, 0);
                    break;
                }
            case 2:
                {
                    gameObject.GetComponent<Image>().color = new Color32(255, 225, 32, 0);
                    break;
                }
            case 3:
                {
                    gameObject.GetComponent<Image>().color = new Color32(0, 89, 178, 0);
                    break;
                }
            case 4:
                {
                    gameObject.GetComponent<Image>().color = new Color32(240, 144, 40, 0);
                    break;
                }
        }
        StopAllCoroutines();
        StartCoroutine(ColorFadeInCoroutine());
    }

    IEnumerator ColorFadeInCoroutine()
    {
        while (gameObject.GetComponent<Image>().color.a < ((1f / 255f) * 30f))
        {
            gameObject.GetComponent<Image>().color = new Color(gameObject.GetComponent<Image>().color.r, gameObject.GetComponent<Image>().color.g, gameObject.GetComponent<Image>().color.b, gameObject.GetComponent<Image>().color.a + 0.03f);
            yield return new WaitForSeconds(0.01f);
        }
        StartCoroutine(ColorFadeOutCoroutine());
    }

    IEnumerator ColorFadeOutCoroutine()
    {
        while (gameObject.GetComponent<Image>().color.a > 0)
        {
            gameObject.GetComponent<Image>().color = new Color(gameObject.GetComponent<Image>().color.r, gameObject.GetComponent<Image>().color.g, gameObject.GetComponent<Image>().color.b, gameObject.GetComponent<Image>().color.a - 0.03f);
            yield return new WaitForSeconds(0.03f);
        }
    }
}
