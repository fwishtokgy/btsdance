using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class HeightToPlayerAdjustor : MonoBehaviour
{
    public GetPlaySpace PlaySpaceCollector;

    protected bool heightCorrectionReceived;

    public CharacterController MyCharacterController;

    public CharacterCameraConstraint characterCamera;

    public TrackingOriginModeFlags TrackingOriginMode;

    protected DebugText debug;

    protected bool wasGeometryRetrieved;

    public Text debugtet;

    protected float heightCalculated;

    public Transform heightDebugView;

    public TrackedPoseDriver trackedPoseDriver;

    protected bool EditorDebug;

    // Start is called before the first frame update
    void Start()
    {
        heightCorrectionReceived = false;
        debug = GameObject.FindObjectOfType<DebugText>();

        if (OVRManager.profile != null)
        {
            heightCalculated = OVRManager.profile.eyeHeight;
            debugtet.text = "profile eyeheight: " + heightCalculated;
            Debug.Log(debugtet.text);
        }
        if (heightCalculated == 0f && OVRManager.instance != null)
        {
            heightCalculated = OVRManager.instance.headPoseRelativeOffsetTranslation.y;
            debugtet.text = "headPoseRelativeOffsetTranslation: " + heightCalculated;
            Debug.Log(debugtet.text);
        }

        EditorDebug = false;
#if UNITY_EDITOR
        EditorDebug = true;
        #endif

        //XRTest();
        wasGeometryRetrieved = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!wasGeometryRetrieved && OVRManager.boundary != null)
        {
            debugtet.text = "Goodie: " + heightCalculated;
            CorrectPlayerHeight();
            wasGeometryRetrieved = true;
        }
    }

    private void OnDestroy()
    {
        wasGeometryRetrieved = false;
    }
    protected void PrintToWorld(string message)
    {
        if (debug != null)
        {
            debug.SetLabel("Debug");
            debug.SetMessage(message);
        }
    }

    protected void XRTest()
    {
        var xrSettings = XRGeneralSettings.Instance;
        if (xrSettings == null)
        {
            Debug.Log($"XRGeneralSettings is null.");
            return;
        }

        var xrManager = xrSettings.Manager;
        if (xrManager == null)
        {
            Debug.Log($"XRManagerSettings is null.");
            return;
        }

        var xrLoader = xrManager.activeLoader;
        if (xrLoader == null)
        {
            Debug.Log($"XRLoader is null.");
            return;
        }

        Debug.Log($"Loaded XR Device: {xrLoader.name}");

        var xrDisplay = xrLoader.GetLoadedSubsystem<XRDisplaySubsystem>();
        Debug.Log($"XRDisplay: {xrDisplay != null}");

        if (xrDisplay != null)
        {
            if (xrDisplay.TryGetDisplayRefreshRate(out float refreshRate))
            {
                Debug.Log($"Refresh Rate: {refreshRate}hz");
            }
        }

        var xrInput = xrLoader.GetLoadedSubsystem<XRInputSubsystem>();
        Debug.Log($"XRInput: {xrInput != null}");

        if (xrInput != null)
        {
            xrInput.TrySetTrackingOriginMode(TrackingOriginMode);
            xrInput.TryRecenter();

            List<Vector3> points = new List<Vector3>();
            xrInput.TryGetBoundaryPoints(points);
            Debug.Log("XR INPUT ==========================");
            if (points.Count > 0)
            {
                foreach (var point in points)
                {
                    Debug.Log("-> " + point.ToString());
                }
            }
            else
            {
                var headPosition = OVRManager.boundary.TestNode(OVRBoundary.Node.Head, OVRBoundary.BoundaryType.OuterBoundary);
                Debug.Log("headPosition: " + headPosition.ClosestDistance + " heightCalculated " + heightCalculated);
            }
        }
        
        var xrMesh = xrLoader.GetLoadedSubsystem<XRMeshSubsystem>();
        Debug.Log($"XRMesh: {xrMesh != null}");
    }

    protected void CorrectPlayerHeight()
    {
        //List<InputDevice> inputDevices = new List<InputDevice>();
        //InputDeviceCharacteristics controlledFilter = InputDeviceCharacteristics.HeadMounted | InputDeviceCharacteristics.TrackedDevice;
        //InputDevices.GetDevicesWithCharacteristics(controlledFilter, inputDevices);
        ////InputDevices.GetDevicesWithRole(InputDeviceRole.Generic, inputDevices);
        //InputDevice hMDDevice = inputDevices[0];
        //XRInputSubsystem XRIS = hMDDevice.subsystem;
        //List<Vector3> boundaryPoints = new List<Vector3>();
        //var pointsFound = XRIS.TryGetBoundaryPoints(boundaryPoints);

        //var pointsFound = OVRManager.boundary.GetGeometry(boundaryType);
        //var offset = 0f;
        //var closestSlopeToHead = headNodeTest.ClosestDistance;

        var boundaryType = OVRBoundary.BoundaryType.OuterBoundary;
        var pointsFound = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
        bool isRoomScale = OVRManager.boundary.GetConfigured() || EditorDebug;

        //if ( pointsFound.Length > 0)
        //{

        if (isRoomScale)
        {
            var headNodeTest = OVRManager.boundary.TestNode(OVRBoundary.Node.Head, boundaryType);

            Debug.Log("Let's check:");
            debugtet.text = "Closest point A: " + headNodeTest.ClosestPoint.ToString();
            Debug.Log(debugtet.text);
            heightCalculated = -headNodeTest.ClosestPoint.y;

            if (EditorDebug)
            {
                heightCalculated = 1.5f;
            }
            //foreach (var point in pointsFound)
            //{
            //    Debug.Log("-> " + point.ToString());
            //    if (headNodeTest.ClosestPoint == point)
            //    {
            //        var groundVector = Vector3.zero - point;
            //        var largeOvershotAngle = Vector3.Angle(headNodeTest.ClosestPointNormal, groundVector);
            //        var trueGroundAngle = largeOvershotAngle - 90f;
            //        heightCalculated = Mathf.Sin(trueGroundAngle) * headNodeTest.ClosestDistance;
            //        var message = "  ---> Math Time. Calculated height:" + heightCalculated.ToString() + " from angle " + trueGroundAngle;
            //        PrintToWorld(message);
            //        Debug.Log(message);
            //        debugtet.text = message;
            //    }
            //}
            //}
            //trackedPoseDriver.transform.localPosition = new Vector3(trackedPoseDriver.transform.localPosition.x, heightCalculated, trackedPoseDriver.transform.localPosition.z);
        }
        else if (pointsFound.Length > 0)
        {
            heightCalculated = -pointsFound[0].y;
            debugtet.text = "Closest point B: " + heightCalculated;
            Debug.Log(debugtet.text);
        }
        else
        {
            var headNodeTest = OVRManager.boundary.TestNode(OVRBoundary.Node.Head, OVRBoundary.BoundaryType.PlayArea);
            XRTest();
            //var pointsFound = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
            //if (pointsFound.Length > 0)
            //{
            //    heightCalculated = -pointsFound[0].y;
            //    debugtet.text = "Closest point: " + heightCalculated;
            //    Debug.Log(debugtet.text);
            //}
            //heightCalculated = -headNodeTest.ClosestDistance;
            debugtet.text = "Closest point C: " + heightCalculated;
        }

        characterCamera.PreferredHeight = heightCalculated;

        MyCharacterController.height = heightCalculated;
        Debug.Log("Setting height " + heightCalculated);
        var newCenterY = heightCalculated / 2f;
        newCenterY = (MyCharacterController.radius > newCenterY)?MyCharacterController.radius:newCenterY;
        MyCharacterController.center = new Vector3(0, newCenterY, 0);
        if (heightDebugView != null)
        {
            heightDebugView.localScale = new Vector3(1, heightCalculated, 1);
        }
        return;

        //MyCharacterController.height = MyCharacterController.height + (PlaySpaceCollector.HeightOffset);
        //OVRProfile.GetPlayerEyeHeight(ref float eyeHeight);


        //if (pointsFound)
        //{
        //    var pointIndex = 0;
        //    foreach (var point in boundaryPoints)
        //    {
        //        Debug.Log("  ---> point " + point.ToString());
        //        offset = ((offset * pointIndex) + point.y) / (pointIndex + 1);
        //        pointIndex++;
        //    }
        //}
        //MyCharacterController.height = MyCharacterController.height + (offset);

        //Debug.Log("Using Height Offset " + PlaySpaceCollector.HeightOffset);
        //Debug.Log("Using Height Offset " + offset);
    }
}
