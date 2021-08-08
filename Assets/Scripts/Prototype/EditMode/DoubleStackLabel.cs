using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoubleStackLabel : MonoBehaviour
{
    [SerializeField]
    protected TextMesh TopText;
    [SerializeField]
    protected TextMesh LowerText;
    
    public void SetText(string upperText, string lowerText)
    {
        TopText.text = upperText;
        LowerText.text = lowerText;
    }
}
