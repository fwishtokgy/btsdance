using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionDebugger : MonoBehaviour
{
    public Transform ObjectToDebug;
    public TextMesh WorldUI;
    public TextMesh LocalUI;


    // Update is called once per frame
    void Update()
    {
        WorldUI.text = WritePosition("World", ObjectToDebug.position);
        LocalUI.text = WritePosition("Local", ObjectToDebug.localPosition);
        this.transform.LookAt(Camera.main.transform);
    }
    protected string WritePosition(string label, Vector3 position)
    {
        return string.Format("{0}: ({1:F4}, {2:F4}, {3:F4})", label, position.x, position.y, position.z);
    }
}
