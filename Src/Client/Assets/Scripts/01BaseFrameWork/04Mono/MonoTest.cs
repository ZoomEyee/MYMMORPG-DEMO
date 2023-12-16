using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoMonoTest
{

    public NoMonoTest()
    {
        PublicMonoMgr.Instance.AddUpdateListener(Update);
    }
    
    void Update()
    {       
        Debug.Log("1");
    }

}

public class MonoTest : MonoBehaviour
{    
    void Start()
    {
        NoMonoTest noMonoText = new NoMonoTest();
    }
    
}
