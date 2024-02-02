using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XluaTest : MonoBehaviour
{
    private void Start()
    {
        XLuaMgr.Instance.SetPath("luatest");
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            XLuaMgr.Instance.DoLuaFileOrAB("LuaTest");
        }
        if (Input.GetMouseButtonDown(1))
        {           
            XLuaMgr.Instance.DoLuaFileOrAB("LuaTestAB");
        }
    }
}
