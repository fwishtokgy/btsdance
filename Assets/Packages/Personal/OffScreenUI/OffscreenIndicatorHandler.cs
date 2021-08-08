using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffscreenIndicatorHandler : Singleton<OffscreenIndicatorHandler>
{
    public GameObject labelTemplate;

    private Dictionary<OffscreenIndicator, OffscreenIconUI> IndicatorInstances;

    protected Canvas mainCameraCanvas;
    protected RectTransform IconContainer;

    public enum TraceType
    {
        COMPASSLOCKED,  // Player view swivels
        VIEWORIENTED,   // Player view always forward. Object location swivels
    }
    public TraceType traceType;

    [SerializeField]
    protected float Floor;
    [SerializeField]
    protected float Ceiling;

    [SerializeField]
    protected float ScaleMinCap;
    [SerializeField]
    protected float ScaleMaxCap;

    /// <summary>
    /// Later we will have two modes. One will use this value, for classic y-positioning.
    /// Floor and Ceiling will be hidden and not used in this mode.
    /// The other mode will use the Floor and Ceiling values. This value will be ignored.
    /// </summary>
    [SerializeField]
    protected float WorldYRange;

    protected float horizontalFieldOfView;

    // Start is called before the first frame update
    void Awake()
    {
        IndicatorInstances = new Dictionary<OffscreenIndicator, OffscreenIconUI>();

        mainCameraCanvas = Camera.main.gameObject.GetComponentInChildren<Canvas>();
        // throw error if no canvas found... or generate a canvas?
        if (mainCameraCanvas == null)
        {
            var newCanvasHolder = new GameObject("CanvasOverlay_AutoGen");
            mainCameraCanvas = newCanvasHolder.AddComponent<Canvas>();
            newCanvasHolder.transform.parent = Camera.main.transform;
        }
        IconContainer = new GameObject("OffscreenIconContainer", typeof(RectTransform)).GetComponent<RectTransform>();
        IconContainer.SetParent(mainCameraCanvas.transform);
        IconContainer.anchorMin = new Vector2(0, 0);
        IconContainer.anchorMax = new Vector2(1, 1);
        IconContainer.sizeDelta = Vector2.zero;
        IconContainer.offsetMin = new Vector2(0, 0);
        IconContainer.offsetMax = new Vector2(0, 0);

        var fovDependentConstant = Mathf.Tan(Mathf.Deg2Rad * Camera.main.fieldOfView * .5f);
        horizontalFieldOfView = 2 * Mathf.Atan(fovDependentConstant * Camera.main.aspect) * Mathf.Rad2Deg;
        //horizontalFieldOfView = 2 * Mathf.Atan((Mathf.Deg2Rad * 1440f / 2f) / fovDependentConstant);
        Debug.Log("horizontalFieldOfView: " + horizontalFieldOfView);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        foreach (var indicator in IndicatorInstances)
        {
            // you may need to ensure that the fixedupdate is called after the two OnBecame calls
            if (indicator.Key.IsOffscreen)
            {
                switch (traceType)
                {
                    case TraceType.COMPASSLOCKED:
                        indicator.Value.SetPlayerView(Camera.main.transform.rotation.eulerAngles.y);
                        indicator.Value.SetObjectArrow(indicator.Key.transform.position, Camera.main.transform, true);
                        break;
                    case TraceType.VIEWORIENTED:
                        indicator.Value.SetObjectArrow(indicator.Key.transform.position, Camera.main.transform);
                        break;
                }

                var distance = Vector3.Distance(indicator.Key.transform.position, Camera.main.transform.position);
                indicator.Value.SetY(Floor, Ceiling, IconContainer, indicator.Key.transform.position.y);
                indicator.Value.SetScale(ScaleMinCap, ScaleMaxCap, distance);
                indicator.Value.SetX(distance, horizontalFieldOfView, IconContainer);
            }
        }
    }

    public void SetAsActive(OffscreenIndicator offscreenIndicator)
    {
        if (IndicatorInstances.ContainsKey(offscreenIndicator))
        {
            return;
        }
        IndicatorInstances[offscreenIndicator].gameObject.SetActive(true);
    }
    public void SetAsInactive(OffscreenIndicator offscreenIndicator)
    {
        if (IndicatorInstances.ContainsKey(offscreenIndicator))
        {
            return;
        }
        IndicatorInstances[offscreenIndicator].gameObject.SetActive(false);
    }

    public GameObject AddLabel(OffscreenIndicator offscreenIndicator, GameObject alternativeTemplate = null)
    {
        if (IndicatorInstances.ContainsKey(offscreenIndicator))
        {
            return null;
        }
        var newLabel = GameObject.Instantiate((alternativeTemplate == null) ? labelTemplate : alternativeTemplate);
        var iconScript = newLabel.GetComponent<OffscreenIconUI>();

        if (iconScript == null)
        {
            return null;
        }

        IndicatorInstances.Add(offscreenIndicator, iconScript);

        iconScript.transform.parent = IconContainer;
        iconScript.SetIcon(offscreenIndicator.IconTexture);

        return newLabel;
    }
    public void RemoveLabel(OffscreenIndicator offscreenIndicator)
    {
        if (IndicatorInstances.ContainsKey(offscreenIndicator))
        {
            var iconToKill = IndicatorInstances[offscreenIndicator];
            Destroy(iconToKill.gameObject);
            IndicatorInstances.Remove(offscreenIndicator);
        }
    }
}
