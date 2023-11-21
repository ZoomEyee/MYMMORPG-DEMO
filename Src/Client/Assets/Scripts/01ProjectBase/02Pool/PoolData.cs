using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolData
{
    private GameObject fatherObj;
    public List<GameObject> poolList;

    public PoolData(GameObject obj, GameObject RootObj)
    {
        fatherObj = new GameObject(obj.name);
        fatherObj.transform.parent = RootObj.transform;
        poolList = new List<GameObject>() { };
    }

    public GameObject GetObj()
    {
        GameObject obj = null;
        obj = poolList[0];
        poolList.RemoveAt(0);
        obj.SetActive(true);
        obj.transform.parent = null;
        return obj;
    }

    public void PushObj(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.parent = fatherObj.transform;
        poolList.Add(obj);
    }
}
