using UnityEngine;
using System.Collections;

public class TextScript : MonoBehaviour
{
    private float x, y;
    private bool transformed = false;
    private Vector3 initialPosition;
    public void Start()
    {
        Shake();
    }

    public void Shake()
    {
        x = transform.localPosition.x;
        y = transform.localPosition.y;
        initialPosition = new Vector3(x, y, 0f);
        StartCoroutine(ShakeCoroutine(2f));
    }

    IEnumerator ShakeCoroutine(float range)
    {
        while (true)
        {
            if (gameObject.GetComponent<TextScript>().enabled == true)
                if (!transformed)
                {
                    transform.localPosition = new Vector3(initialPosition.x + Random.Range(-range, range), initialPosition.y + Random.Range(-range, range), initialPosition.z);
                    transformed = true;
                }
                else
                {
                    transform.localPosition = initialPosition;
                    transformed = false;
                }
            yield return new WaitForSeconds(0.05f);
        }
    }
}
