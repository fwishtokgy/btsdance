using UnityEngine;

/// <summary>
/// Behavior for editor level instances of a star
/// </summary>
public class StarBitEditable : MonoBehaviour
{
    /// <summary>
    /// Reference to the parent transform
    /// </summary>
    protected GameObject ObjectRoot;

    protected StarWandEditor EditorWand;

    /// <summary>
    /// Reference to the rotation behavior
    /// </summary>
    public SimpleRotate RotationHandler;

    protected float normalScale;
    protected float bloatedScale;

    public SmoothFollow smoothFollowScript;

    [SerializeField]
    protected int MyTimeSampleIndex;
    public int TimeSampleIndex
    {
        get
        {
            return MyTimeSampleIndex;
        }
    }

    void Awake()
    {
        ObjectRoot = this.transform.parent.gameObject;
        normalScale = RotationHandler.speed.y;
        bloatedScale = normalScale * 3f;
    }

    public void Initiate(int SampleIndex, StarWandEditor editor)
    {
        MyTimeSampleIndex = SampleIndex;
        EditorWand = editor;
        smoothFollowScript.enabled = false;
    }

    // Update is called once per frame
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == EditorWand.SpawnPoint.gameObject && !EditorWand.IsCurrentlyEngaged)
        {
            if (EditorWand.CurrentFocus != null && EditorWand.CurrentFocus != this)
            {
                RotationHandler.speed = new Vector3(0, normalScale, 0);
            }
            EditorWand.SetTarget(this);
            RotationHandler.speed = new Vector3(0, bloatedScale, 0);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == EditorWand.SpawnPoint.gameObject && EditorWand.CurrentFocus == null)
        {
            EditorWand.SetTarget(this);
            RotationHandler.speed = new Vector3(0, bloatedScale, 0);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == EditorWand.SpawnPoint.gameObject && !(EditorWand.IsCurrentlyEngaged && EditorWand.CurrentFocus == this))
        {
            EditorWand.ReleaseTarget(this);
            RotationHandler.speed = new Vector3(0, normalScale, 0);
        }
    }

    public void ExecuteTrigger()
    {
        switch (EditorWand.CurrentMode)
        {
            case StarEdit.Mode.MOVE:
                StartMove();
                break;
            case StarEdit.Mode.DELETE:
                Delete();
                break;
            default:
                break;
        }
    }
    public void ReleaseTrigger()
    {
        switch (EditorWand.CurrentMode)
        {
            case StarEdit.Mode.MOVE:
                EndMove();
                break;
            default:
                break;
        }
    }

    protected void StartMove()
    {
        var distance = Vector3.Distance(ObjectRoot.transform.position, EditorWand.SpawnPoint.position);
        smoothFollowScript.Target = EditorWand.SpawnPoint;
        smoothFollowScript.Distance = distance;
        smoothFollowScript.enabled = true;
    }
    protected void EndMove()
    {
        EditorWand.AlertToMove(this);
        smoothFollowScript.enabled = false;
    }

    public void Set()
    {
        EndMove();
    }

    protected void Delete()
    {
        EditorWand.AlertToDeath(this);
        GameObject.Destroy(ObjectRoot);
    }
}
