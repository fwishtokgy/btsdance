using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes the transform this component is attached to face the main camera at all times
/// </summary>
public class ConstantCameraLookAt : MonoBehaviour
{
    /// <summary>
    /// Whether this transform will maintain an upright position or fully face the camera.
    /// </summary>
    public bool MaintainWorldYAxis;
    
    /// <summary>
    /// Holds the height of our current transform and copies it into the value used when facing the camera
    /// </summary>
    protected Vector3 HeightHolder;

    public bool RevertedLookat;
    protected Vector3 referenceVector;

    private void Start()
    {
        referenceVector = RevertedLookat ? Vector3.down : Vector3.up;
    }

    // Update is called once per frame
    void Update()
    {
        var positiondiff = transform.position - Camera.main.transform.position;
        if (MaintainWorldYAxis)
        {
            HeightHolder = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z);
            transform.LookAt(HeightHolder, referenceVector);
            positiondiff = new Vector3(positiondiff.x, 0, positiondiff.z);
        }
        else
        {
            transform.LookAt(Camera.main.transform);
        }
        if (positiondiff != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(positiondiff, Vector3.up);
        }
    }
}
