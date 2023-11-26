using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoMonoText
{

    public NoMonoText()
    {
        PublicMonoMgr.Instance.AddUpdateListener(Update);
    }
    
    void Update()
    {       
        Debug.Log("1");
    }

}

public class MonoText : MonoBehaviour
{    
    void Start()
    {
        NoMonoText noMonoText = new NoMonoText();
    }
    
}
