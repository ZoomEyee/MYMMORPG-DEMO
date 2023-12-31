using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PublicMonoMgr : MonoSingleton<PublicMonoMgr>
{
    private event UnityAction updateEvents;
    private event UnityAction fixedUpdateEvents;
    private event UnityAction lateUpdateEvents;
    private void Update()
    {
        updateEvents?.Invoke();
    }
    private void FixedUpdate()
    {
        fixedUpdateEvents?.Invoke();
    }
    private void LateUpdate()
    {
        lateUpdateEvents?.Invoke();
    }
    public void AddUpdateListener(UnityAction fun)
    {
        updateEvents += fun;
    }
    public void RemoveUpdateListener(UnityAction fun)
    {
        updateEvents -= fun;
    }
    public void AddFixedUpdateListener(UnityAction fun)
    {
        fixedUpdateEvents += fun;
    }
    public void RemoveFixedUpdateListener(UnityAction fun)
    {
        fixedUpdateEvents -= fun;
    }
    public void AddLateUpdateListener(UnityAction fun)
    {
        lateUpdateEvents += fun;
    }
    public void RemoveLateUpdateListener(UnityAction fun)
    {
        lateUpdateEvents -= fun;
    }
}
