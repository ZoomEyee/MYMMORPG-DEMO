using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingletonThread<T> : MonoBehaviour where T : MonoBehaviour
{
    private static readonly object lockObject = new object();
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                lock (lockObject)
                {
                    if (instance == null)
                    {
                        GameObject obj = new GameObject(typeof(T).Name);
                        instance = obj.AddComponent<T>();
                        DontDestroyOnLoad(obj);
                    }
                }
            }
            return instance;
        }
    }
}
