using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class P19 : MonoBehaviour
{
    public GameObject p;

    private void FixedUpdate()
    {
        p.transform.transform.Rotate(0, 400 * Time.deltaTime, 0);
    }
}
