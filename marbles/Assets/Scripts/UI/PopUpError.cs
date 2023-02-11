using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopUpError : MonoBehaviour
{
    public TextMeshProUGUI ErrorText;

    public void SetErrorText(string txt)
    {
        ErrorText.text = "ERROR: " + txt;
    }

    public void OpenErrorMessage()
    {
        GetComponent<Canvas>().enabled = true;
    }

    public void CloseErrorMessage()
    {
        GetComponent<Canvas>().enabled = false;
    }
}