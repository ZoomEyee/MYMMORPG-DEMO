using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolText : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PoolMgr.Instance.GetObj("Prefab/Cube", (obj) => { obj.transform.localScale *= 2; });
        }
        if (Input.GetMouseButtonDown(1))
        {
            PoolMgr.Instance.GetObj("Prefab/Sphere", (obj) => { obj.transform.localScale /= 2; });
        }
        Debug.Log("dataPath:" + Application.dataPath);
        Debug.Log("streamingAssetsPath:" + Application.streamingAssetsPath);
        Debug.Log("persistentDataPath:" + Application.persistentDataPath);
    }
}
