using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    public TextMesh LabelUI;
    public TextMesh MessageUI;

    public void SetLabel(string label)
    {
        LabelUI.text = label;
    }
    public void SetMessage(string message)
    {
        MessageUI.text = message;
    }
}
