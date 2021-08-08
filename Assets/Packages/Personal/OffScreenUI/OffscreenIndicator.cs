using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffscreenIndicator : MonoBehaviour
{
    [SerializeField]
    protected Sprite SubjectIcon;
    public Sprite IconTexture
    {
        get
        {
            return SubjectIcon;
        }
    }

    protected bool IsVisible;
    public bool IsOffscreen
    {
        get
        {
            return !IsVisible;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        RetrieveReferences();
        Register();
    }

    protected void RetrieveReferences()
    {
        if (OffscreenIndicatorHandler.Instance == null)
        {
            var handler = new GameObject("OffscreenLabels");
            handler.AddComponent<OffscreenIndicatorHandler>();
        }
    }

    protected virtual void Register()
    {
        OffscreenIndicatorHandler.Instance.AddLabel(this);
    }

    protected void OnDestroy()
    {
        OffscreenIndicatorHandler.Instance.RemoveLabel(this);
    }

    protected void OnBecameVisible()
    {
        IsVisible = true;
        OffscreenIndicatorHandler.Instance.SetAsInactive(this);
    }
    protected void OnBecameInvisible()
    {
        IsVisible = false;
        OffscreenIndicatorHandler.Instance.SetAsActive(this);
    }
}
