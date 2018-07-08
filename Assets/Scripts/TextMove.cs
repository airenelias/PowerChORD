using UnityEngine;
using System.Collections;

public class TextMove : MonoBehaviour
{
    public bool header = false;
    private float x, y;
    private Vector3 initialPosition;

    void Start()
    {
        if (header)
        {
            x = transform.localPosition.x;
            y = transform.localPosition.y;
            initialPosition = new Vector3(x, y, 0f);
            StartCoroutine(MoveInCoroutine());
        }
    }

    IEnumerator MoveInCoroutine()
    {
        x = -1000;
        while (x < -1)
        {
            transform.localPosition = new Vector3(x, transform.localPosition.y, transform.localPosition.z);
            x = x / 1.2f;
            yield return new WaitForSeconds(0.01f);
        }
        transform.localPosition = initialPosition;
    }
}
