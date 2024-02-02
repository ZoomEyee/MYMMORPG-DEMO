using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


public class AddressablesInfo
{
    public AsyncOperationHandle handle;
    public uint count;
    public AddressablesInfo(AsyncOperationHandle handle)
    {
        this.handle = handle;
        count += 1;
    }
}

public class AddressablesMgr : Singleton<AddressablesMgr>
{
    public Dictionary<string, AddressablesInfo> resDic = new Dictionary<string, AddressablesInfo>();
    public void LoadAssetAsync<T>(string name, Action<AsyncOperationHandle<T>> callBack)
    {
        string keyName = name + "_" + typeof(T).Name;
        AsyncOperationHandle<T> handle;
        if (resDic.ContainsKey(keyName))
        {
            handle = resDic[keyName].handle.Convert<T>();
            resDic[keyName].count += 1;
            if (handle.IsDone)
            {
                callBack(handle);
            }
            else
            {
                handle.Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                        callBack(obj);
                };
            }
            return;
        }
        handle = Addressables.LoadAssetAsync<T>(name);
        handle.Completed += (obj) =>
        {
            if (obj.Status == AsyncOperationStatus.Succeeded)
                callBack(obj);
            else
            {
                Debug.LogWarning(keyName + "资源加载失败");
                if (resDic.ContainsKey(keyName))
                    resDic.Remove(keyName);
            }
        };
        AddressablesInfo info = new AddressablesInfo(handle);
        resDic.Add(keyName, info);
    }
    public void LoadAssetAsync<T>(Addressables.MergeMode mode, Action<T> callBack, params string[] keys)
    {
        List<string> list = new List<string>(keys);
        string keyName = "";
        foreach (string key in list)
            keyName += key + "_";
        keyName += typeof(T).Name;
        AsyncOperationHandle<IList<T>> handle;
        if (resDic.ContainsKey(keyName))
        {
            handle = resDic[keyName].handle.Convert<IList<T>>();
            resDic[keyName].count += 1;
            if (handle.IsDone)
            {
                foreach (T item in handle.Result)
                    callBack(item);
            }
            else
            {
                handle.Completed += (obj) =>
                {
                    if (obj.Status == AsyncOperationStatus.Succeeded)
                    {
                        foreach (T item in handle.Result)
                            callBack(item);
                    }
                };
            }
            return;
        }
        handle = Addressables.LoadAssetsAsync<T>(list, callBack, mode);
        handle.Completed += (obj) =>
        {
            if (obj.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError("资源加载失败" + keyName);
                if (resDic.ContainsKey(keyName))
                    resDic.Remove(keyName);
            }
        };
        AddressablesInfo info = new AddressablesInfo(handle);
        resDic.Add(keyName, info);
    }
    public void LoadAssetAsync<T>(Addressables.MergeMode mode, Action<AsyncOperationHandle<IList<T>>> callBack, params string[] keys)
    {

    }
    public void Release<T>(string name)
    {
        string keyName = name + "_" + typeof(T).Name;
        if (resDic.ContainsKey(keyName))
        {
            resDic[keyName].count -= 1;
            if (resDic[keyName].count == 0)
            {
                AsyncOperationHandle<T> handle = resDic[keyName].handle.Convert<T>();
                Addressables.Release(handle);
                resDic.Remove(keyName);
            }
        }
    }
    public void Release<T>(params string[] keys)
    {
        List<string> list = new List<string>(keys);
        string keyName = "";
        foreach (string key in list)
            keyName += key + "_";
        keyName += typeof(T).Name;

        if (resDic.ContainsKey(keyName))
        {
            resDic[keyName].count -= 1;
            if (resDic[keyName].count == 0)
            {
                AsyncOperationHandle<IList<T>> handle = resDic[keyName].handle.Convert<IList<T>>();
                Addressables.Release(handle);
                resDic.Remove(keyName);
            }

        }
    }
    public void Clear()
    {
        foreach (var item in resDic.Values)
        {
            Addressables.Release(item.handle);
        }
        resDic.Clear();
        AssetBundle.UnloadAllAssetBundles(true);
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
}
