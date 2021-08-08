using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBlurb : MonoBehaviour
{
    protected enum BlurbState
    {
        IsSpawning,
        Spawned,
        IsDespawning,
        Invisible
    }
    protected BlurbState state;

    protected BlurbController ParentController;

    [SerializeField]
    protected Animator BlurbAnimator;

    [SerializeField]
    protected GameObject Backdrop;

    [SerializeField]
    protected GameObject MessageHolder;

    void Awake()
    {
        ParentController = BlurbController.Instance;
        ParentController.RegisterBlurb(this);

        Backdrop.SetActive(false);
        MessageHolder.SetActive(false);

        state = BlurbState.Invisible;
    }

    public void Spawn()
    {
        if (state == BlurbState.IsSpawning)
        {
            return;
        }
        StopAllCoroutines();
        Backdrop.SetActive(true);
        MessageHolder.SetActive(true);
        state = BlurbState.IsSpawning;
        BlurbAnimator.SetTrigger("SpawnTrigger");
    }
    public void Despawn()
    {
        if (state == BlurbState.IsDespawning || state == BlurbState.Invisible)
        {
            return;
        }
        state = BlurbState.IsDespawning;
        BlurbAnimator.SetTrigger("DespawnTrigger");
    }
    protected void DespawnComplete()
    {
        Backdrop.SetActive(false);
        MessageHolder.SetActive(false);
        state = BlurbState.Invisible;
    }
}