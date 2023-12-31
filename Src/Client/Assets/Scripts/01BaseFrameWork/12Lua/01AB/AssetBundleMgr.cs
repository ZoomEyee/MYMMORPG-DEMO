using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AssetBundleMgr : MonoSingleton<AssetBundleMgr>
{
    private AssetBundle mainAB = null;
    private AssetBundleManifest manifest = null;
    private Dictionary<string, AssetBundle> abDic = new Dictionary<string, AssetBundle>();
    public string pathAB = Application.streamingAssetsPath + "/AssetBundles/";
    
    private string MainName
    {
        get
        {
#if UNITY_IOS
            return "IOS";
#elif UNITY_ANDROID
            return "Android";
#else
            return "PC";
#endif
        }
    }

    private void LoadMainAB()
    {
        if (mainAB == null)
        {
            mainAB = AssetBundle.LoadFromFile(pathAB + MainName);
            manifest = mainAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }
    }
    private void LoadDependencies(string abName)
    {
        LoadMainAB();
        string[] strs = manifest.GetAllDependencies(abName);
        for (int i = 0; i < strs.Length; i++)
        {
            if (!abDic.ContainsKey(strs[i]))
            {
                AssetBundle ab = AssetBundle.LoadFromFile(pathAB + strs[i]);
                abDic.Add(strs[i], ab);
            }
        }
    }

    public Object LoadRes(string abName, string resName)
    {
        LoadDependencies(abName);
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(pathAB + abName);
            abDic.Add(abName, ab);
        }
        Object obj = abDic[abName].LoadAsset(resName);
        if (obj is GameObject)
            return Instantiate(obj);
        else
            return obj;
    }
    public T LoadRes<T>(string abName, string resName) where T : Object
    {
        LoadDependencies(abName);
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(pathAB + abName);
            abDic.Add(abName, ab);
        }
        T obj = abDic[abName].LoadAsset<T>(resName);
        if (obj is GameObject)
            return Instantiate(obj);
        else
            return obj;
    }
    public Object LoadRes(string abName, string resName, System.Type type)
    {
        LoadDependencies(abName);
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(pathAB + abName);
            abDic.Add(abName, ab);
        }
        Object obj = abDic[abName].LoadAsset(resName, type);
        if (obj is GameObject)
            return Instantiate(obj);
        else
            return obj;
    }
    public void LoadResAsync(string abName, string resName, UnityAction<Object> callBack)
    {
        StartCoroutine(ReallyLoadResAsync(abName, resName, callBack));
    }
    public void LoadResAsync<T>(string abName, string resName, UnityAction<T> callBack) where T : Object
    {
        StartCoroutine(ReallyLoadResAsync<T>(abName, resName, callBack));
    }
    public void LoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callBack)
    {
        StartCoroutine(ReallyLoadResAsync(abName, resName, type, callBack));
    }
    private IEnumerator ReallyLoadResAsync(string abName, string resName, UnityAction<Object> callBack)
    {
        LoadDependencies(abName);
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(pathAB + abName);
            abDic.Add(abName, ab);
        }
        AssetBundleRequest abq = abDic[abName].LoadAssetAsync(resName);
        yield return abq;

        if (abq.asset is GameObject)
            callBack(Instantiate(abq.asset));
        else
            callBack(abq.asset);
    }
    private IEnumerator ReallyLoadResAsync<T>(string abName, string resName, UnityAction<T> callBack) where T : Object
    {
        LoadDependencies(abName);
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(pathAB + abName);
            abDic.Add(abName, ab);
        }
        AssetBundleRequest abq = abDic[abName].LoadAssetAsync<T>(resName);
        yield return abq;
        if (abq.asset is GameObject)
            callBack(Instantiate(abq.asset) as T);
        else
            callBack(abq.asset as T);
    }
    private IEnumerator ReallyLoadResAsync(string abName, string resName, System.Type type, UnityAction<Object> callBack)
    {
        LoadDependencies(abName);
        if (!abDic.ContainsKey(abName))
        {
            AssetBundle ab = AssetBundle.LoadFromFile(pathAB + abName);
            abDic.Add(abName, ab);
        }
        AssetBundleRequest abq = abDic[abName].LoadAssetAsync(resName, type);
        yield return abq;
        if (abq.asset is GameObject)
            callBack(Instantiate(abq.asset));
        else
            callBack(abq.asset);
    }

    public void UnLoadAB(string abName, bool unloadAllLoadedObjects = false)
    {
        if (abDic.ContainsKey(abName))
        {
            abDic[abName].Unload(unloadAllLoadedObjects);
            abDic.Remove(abName);
        }
    }
    public void ClearAB(bool unloadAllObjects = false)
    {
        AssetBundle.UnloadAllAssetBundles(unloadAllObjects);
        abDic.Clear();
        mainAB = null;
    }
}
