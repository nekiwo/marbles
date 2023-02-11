using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCircle : MonoBehaviour
{
    private bool direction = true;

    void Update()
    {
        Image img = GetComponent<Image>();

        if (img.fillAmount == 0)
        {
            direction = true;
            img.fillClockwise = true;
        } else if (img.fillAmount == 1)
        {
            direction = false;
            img.fillClockwise = false;
        }

        if (direction)
        {
            img.fillAmount += Time.deltaTime;
        } else
        {
            img.fillAmount -= Time.deltaTime;
        }
    }
}
