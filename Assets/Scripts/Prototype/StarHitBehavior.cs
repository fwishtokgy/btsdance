using UnityEngine;

/// <summary>
/// Connects a collider with the starblock behavior.
/// </summary>
public class StarHitBehavior : MonoBehaviour
{
    public starblock StarBehavior;

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        StarBehavior.CollisionCall();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == StarBehavior.MyHandler)
        {
            StarBehavior.CollisionCall();
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.tag == StarBehavior.MyHandler)
        {
            StarBehavior.CollisionCall();
        }
    }
}