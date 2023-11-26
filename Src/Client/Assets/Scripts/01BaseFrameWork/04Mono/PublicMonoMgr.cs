using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PublicMonoMgr : MonoSingleton<PublicMonoMgr>
{
    private event UnityAction updateEvents;
    void Update()
    {
        if (updateEvents != null)
            updateEvents();
    }
    public void AddUpdateListener(UnityAction fun)
    {
        updateEvents += fun;
    }
    public void RemoveUpdateListener(UnityAction fun)
    {
        updateEvents -= fun;
    }
}
