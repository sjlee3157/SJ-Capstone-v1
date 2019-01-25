using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class delayedTextChange : MonoBehaviour
{
    public int delayBeforeChange = 5;
    public string textTwo;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DelayedEdit());
    }

    IEnumerator DelayedEdit()
    {
        yield return new WaitForSeconds(delayBeforeChange);
        TextMeshPro textmeshPro = GetComponent<TextMeshPro>();
        textmeshPro.SetText(textTwo);
    }
}