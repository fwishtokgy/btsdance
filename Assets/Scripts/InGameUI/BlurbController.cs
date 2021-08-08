using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class BlurbController : Singleton<BlurbController>
{
    public Transform BlurbRoot;

    protected List<UIBlurb> blurbs;

    [SerializeField]
    protected bool _IsVisible;
    public bool IsVisible
    {
        get
        {
            return _IsVisible;
        }
        set
        {
            //foreach (var child in BlurbRoot.GetComponentsInChildren<UIBlurb>())
            //{

            //}
            BlurbRoot.gameObject.SetActive(value);
            _IsVisible = value;
        }
    }

    public void RegisterBlurb(UIBlurb blurb)
    {
        if (blurbs == null)
        {
            blurbs = new List<UIBlurb>();
        }
        blurbs.Add(blurb);
    }
}