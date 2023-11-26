using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PoolMgr : Singleton<PoolMgr>
{
    private GameObject RootObj;
    private Dictionary<string, PoolData> poolDic = new Dictionary<string, PoolData>();
    
    public void GetObj(string objPathName, UnityAction<GameObject> callBack)
    {
        if (poolDic.ContainsKey(objPathName) && poolDic[objPathName].poolList.Count > 0)
            callBack(poolDic[objPathName].GetObj());
        else
        {
            ResourcesMgr.Instance.LoadAsync<GameObject>(objPathName, (obj) =>
             {
                 obj.name = objPathName;
                 callBack(obj);
             });
        }
    }

    public void PushObj(GameObject obj)
    {
        if (RootObj == null)
            RootObj = new GameObject("PoolRoot");
        if (!poolDic.ContainsKey(obj.name))
            poolDic.Add(obj.name, new PoolData(obj, RootObj));

        poolDic[obj.name].PushObj(obj);
    }

    public void Clear()
    {
        poolDic.Clear();
        RootObj = null;
    }
}
