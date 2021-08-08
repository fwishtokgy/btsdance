using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckHeight : MonoBehaviour
{
    public Text text;
    public Transform transfom;

    protected float lastHeight = 0f;
    // Start is called before the first frame update
    void Start()
    {
        text = this.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        var currentHeight = transfom.position.y;
        if (currentHeight != lastHeight)
        {
            lastHeight = currentHeight;
            text.text = "Height: " + currentHeight;
            text.SetAllDirty();
        }
    }
}
