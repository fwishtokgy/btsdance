using UnityEngine;

/// <summary>
/// Follows behind a prescribed target smoothly
/// </summary>
public class SmoothFollow : MonoBehaviour
{
    /// <summary>
    /// The target transform to follow
    /// </summary>
    public Transform Target;
    /// <summary>
    /// The distance from the target to maintain
    /// </summary>
    public float Distance = 3.0f;
    /// <summary>
    /// The vertical displacement of this object from its target.
    /// </summary>
    public float Height = 3.0f;
    /// <summary>
    /// The lag between the position lerping
    /// </summary>
    public float Damping = 5.0f;
    /// <summary>
    /// Whether the object should follow behind the target rather than in-front
    /// </summary>
    public bool FollowBehind = true;
    /// <summary>
    /// Whether the object's Y-position should be locked.
    /// </summary>
    public bool LockY = false;

    private void Awake()
    {
        if (Target != null)
        {
            Distance = (FollowBehind ? -Distance : Distance);
        }
    }
    void Update()
    {
        if (Target != null)
        {
            Vector3 wantedPosition;
            
            if (LockY)
            {
                wantedPosition = Target.TransformPoint(0, Height, Distance);
                wantedPosition = new Vector3(wantedPosition.x, Height, wantedPosition.z);
            }
            else
            {
                wantedPosition = Target.TransformPoint(0, Target.position.y + Height, Distance);
            }
            transform.position = Vector3.Lerp(transform.position, wantedPosition, Time.deltaTime * Damping);
        }
    }
}
