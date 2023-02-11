using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P18 : MonoBehaviour
{
    public GameObject p1;
    public GameObject p2;

    private void FixedUpdate()
    {
        p1.transform.transform.Rotate(0, -30 * Time.deltaTime, 0);
        p2.transform.transform.Rotate(0, -30 * Time.deltaTime, 0);
    }
}
