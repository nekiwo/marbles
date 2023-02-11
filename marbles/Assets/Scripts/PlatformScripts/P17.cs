using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P17 : MonoBehaviour
{
    public GameObject p1;
    public GameObject p2;

    private void FixedUpdate()
    {
        p1.transform.transform.Rotate(50 * Time.deltaTime, 0, 0);
        p2.transform.transform.Rotate(50 * Time.deltaTime, 0, 0);
    }
}
