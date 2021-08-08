using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetPlaySpace : MonoBehaviour
{
    protected bool wasGeometryRetrieved;
    public bool WasGeometryCalculated
    {
        get
        {
            return wasGeometryRetrieved;
        }
    }

    public GameObject BoundsMarker;
    public CustomMeshObject StageMesh;

    public Transform PodiumBase;

    public bool debugOn;
    public bool inEditor;

    protected float heightOffset;

    [SerializeField]
    protected Transform StageCenter;
    public Transform Center
    {
        get
        {
            return StageCenter;
        }
    }

    public float HeightOffset
    {
        get
        {
            return heightOffset;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        wasGeometryRetrieved = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!wasGeometryRetrieved && OVRManager.boundary != null && OVRManager.boundary.GetConfigured())
        {
            Vector3[] points = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
            wasGeometryRetrieved = true;
            ParseBounds(points);
        }
        if (inEditor && OVRManager.boundary == null && !wasGeometryRetrieved)
        {
            wasGeometryRetrieved = true;
            var points = new Vector3[] { new Vector3(.4f, 0, 1.1f), new Vector3(-1.2f, 0, .1f), new Vector3(-.4f, 0, -1.1f), new Vector3(1.2f, 0, -.1f) };
            ParseBounds(points);
        }
        //also add a call to 'ParseBounds' when the bounds get redrawn...
    }
    void ParseBounds (Vector3[] points)
    {
        float averageY = 0f;
        if (points.Length == 0)
        {
            return;
        }
        if (debugOn)
        {
            Debug.Log("POINTS: -----------");
            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
            {
                var newMarker = GameObject.Instantiate(BoundsMarker);
                newMarker.transform.position = points[pointIndex];
                float ratio = (float)pointIndex / points.Length;
                var markerColor = Color.HSVToRGB(ratio, 1, 1);
                //var pointRenderer = BoundsMarker.transform.GetChild(0).GetComponent<Renderer>();
                //if (pointRenderer.sharedMaterials.Length > 0)
                //{
                //    var newMaterial = new Material(pointRenderer.sharedMaterials[0]);
                //    newMaterial.color = markerColor;
                //    pointRenderer.material = newMaterial;
                //}
                var pointLabel = BoundsMarker.transform.GetChild(1).GetComponent<TextMesh>();
                pointLabel.text = pointIndex.ToString();
                Debug.Log("Point " + pointIndex + ": " + points[pointIndex].ToString());

                averageY = ((averageY * pointIndex) + points[pointIndex].y) / (pointIndex + 1);
            }
        }
        var boundpoints = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
        OVRManager.boundary.SetVisible(true);

        if (boundpoints.Length > 0)
        {
            heightOffset = boundpoints[0].y;
        }

        StageMesh.SetMeshPoints(points);
        var newscale = StageMesh.BogusDoubledRadius();
        PodiumBase.localScale = new Vector3(newscale, PodiumBase.localScale.y, newscale);
        var newcenter = StageMesh.Center();
        PodiumBase.localPosition = new Vector3(newcenter.x, PodiumBase.localPosition.y, newcenter.z);
        StageCenter.position = newcenter;
        //this.transform.position = Vector3.zero;
    }
}
