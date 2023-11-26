using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResourcesMgr : Singleton<ResourcesMgr>
{
    public T Load<T>(string resName) where T : Object
    {
        T res = Resources.Load<T>(resName);
        if (res is GameObject)
            return GameObject.Instantiate(res);
        else
            return res;
    }
    public void LoadAsync<T>(string resName, UnityAction<T> callBack) where T : Object
    {
        PublicMonoMgr.Instance.StartCoroutine(CoroutineLoadAsync(resName, callBack));
    }
    private IEnumerator CoroutineLoadAsync<T>(string resName, UnityAction<T> callBack) where T : Object
    {
        ResourceRequest rr = Resources.LoadAsync<T>(resName);
        yield return rr;

        if (rr.asset is GameObject)
            callBack(GameObject.Instantiate(rr.asset) as T);
        else
            callBack(rr.asset as T);
    }


}
