using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using XLua;

public class XLuaMgr : Singleton<XLuaMgr>
{
    private LuaEnv luaEnv;
    private string luaPath = Application.dataPath + "/Data/Lua/";
    private string luaABName = "lua";

    public LuaTable Global
    {
        get
        {
            return luaEnv.Global;
        }
    }
    public XLuaMgr()
    {
        Init();
    }
    private void Init()
    {
        if (luaEnv != null)
            Dispose();
        luaEnv = new LuaEnv();
        luaEnv.AddLoader(FileLoader);
        luaEnv.AddLoader(ABLoader);       
    }
    private byte[] FileLoader(ref string luaFileName)
    {
        string path = luaPath + luaFileName + ".lua";
        if (File.Exists(path))
            return File.ReadAllBytes(path);
        else
            Debug.Log("(FileLoader)重定向失败，文件名为" + luaFileName);
        return null;
    }
    private byte[] ABLoader(ref string luaFileName)
    {
        TextAsset lua = AssetBundleMgr.Instance.LoadRes<TextAsset>(luaABName, luaFileName + ".lua");
        if (lua != null)
            return lua.bytes;
        else
            Debug.Log("(ABLoader)重定向失败，文件名为：" + luaFileName);
        return null;
    }
    public void DoLuaFileOrAB(string luaFileName)
    {
        string str = string.Format("require('{0}')", luaFileName);
        DoString(str);
    }
    public void DoString(string luaScriptStr)
    {
        if (luaEnv == null)
        {
            Debug.Log("(DoString)解析器未初始化");
            return;
        }
        luaEnv.DoString(luaScriptStr);
    }
    public void SetPath(string luaABName = null, string abPath = null, string luaPath = null)
    {
        if (luaABName != null)
            this.luaABName = luaABName;
        if (abPath != null)
            AssetBundleMgr.Instance.setABPath(abPath);
        if (luaPath != null)
            this.luaPath = luaPath;
        if (luaABName != null || abPath != null || luaPath != null)
            Init();
    }
    public void Tick()
    {
        if (luaEnv == null)
        {
            Debug.Log("(Tick)解析器未初始化");
            return;
        }
        luaEnv.Tick();
    }
    public void Dispose()
    {
        if (luaEnv == null)
        {
            Debug.Log("(Dispose)解析器未初始化");
            return;
        }
        try
        {
            luaEnv.Dispose();
            luaEnv = null;
            Debug.Log("解析器销毁成功");
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }

    }
}
