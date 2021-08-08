using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBlurbAwake : UIBlurb
{
    private void Start()
    {
        Debug.Log("Spawn Awaken");
        Spawn();
    }
    private void OnEnable()
    {
        Spawn();
    }
    private void OnDisable()
    {
        Despawn();
    }
}
