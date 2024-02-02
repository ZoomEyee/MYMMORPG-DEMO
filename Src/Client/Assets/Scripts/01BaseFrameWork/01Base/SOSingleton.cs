using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SOSingleton<T> : ScriptableObject where T : ScriptableObject
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<T>("ScriptableObject/" + typeof(T).Name);
            if (instance == null)
                instance = CreateInstance<T>();
            return instance;
        }
    }
}
