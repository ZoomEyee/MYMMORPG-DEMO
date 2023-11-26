using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolDelayDisable : MonoBehaviour
{
    public float disableTime = 2.0f;
    void OnEnable()
    {
        Invoke("DelayDisable", disableTime);
    }

    void DelayDisable()
    {
        PoolMgr.Instance.PushObj(this.gameObject);
    }
}
